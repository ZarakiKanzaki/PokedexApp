namespace PokedexApp.WebApi.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed");
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.StatusCode == System.Net.HttpStatusCode.NotFound 
                    ? "Resource not found." 
                    : "An error occurred while processing your request."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            await context.Response.WriteAsJsonAsync(new
            {
                message = "An unexpected error occurred."
            });
        }
    }
}