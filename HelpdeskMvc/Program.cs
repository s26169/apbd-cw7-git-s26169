using HelpdeskMvc.Data;
using HelpdeskMvc.Middleware;
using HelpdeskMvc.Repositories;
using HelpdeskMvc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Layers registered in DI: Controller -> Service -> Repository -> DbContext.
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();

var app = builder.Build();

// Make sure the database/schema exists (no migrations needed for this assignment).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Middleware order matters: each app.Use* call wraps everything registered after it.
// Exception handling goes first so it can catch errors from anything below it,
// including the timing middleware and MVC itself.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=Index}/{id?}");

app.Run();

// Needed so the test project (or WebApplicationFactory-style tests) can reference Program.
public partial class Program { }
