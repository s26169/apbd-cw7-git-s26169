using HelpdeskMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskMvc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Status).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.Property(c => c.Author).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Content).IsRequired();

            entity.HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
