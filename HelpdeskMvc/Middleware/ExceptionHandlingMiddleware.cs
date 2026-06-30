namespace HelpdeskMvc.Middleware;

// Catches any unhandled exception from everything further down the pipeline,
// logs the real details server-side, and shows the user a generic message
// instead of a stack trace.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync(
                "<html><body>" +
                "<h1>Something went wrong</h1>" +
                "<p>An unexpected error occurred while processing your request. Please try again later.</p>" +
                "<p><a href=\"/Tickets\">Back to ticket list</a></p>" +
                "</body></html>");
        }
    }
}
