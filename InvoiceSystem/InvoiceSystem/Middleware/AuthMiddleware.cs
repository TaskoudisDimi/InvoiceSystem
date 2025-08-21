using InvoiceSystem;

namespace InvoicingSystem.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly InMemoryDataService _dataService;

    public AuthMiddleware(RequestDelegate next, InMemoryDataService dataService)
    {
        _next = next;
        _dataService = dataService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var token) || token != "Bearer demo-token-compA")
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        context.Items["CompanyId"] = "compA";

        await _next(context);
    }
}