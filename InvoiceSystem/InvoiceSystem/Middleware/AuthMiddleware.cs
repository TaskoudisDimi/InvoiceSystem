using InvoicingSystem.Services;

namespace InvoicingSystem.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDataService _dataService;

    public AuthMiddleware(RequestDelegate next, IDataService dataService)
    {
        _next = next;
        _dataService = dataService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only set CompanyId if token is valid; otherwise, proceed without it
        if (context.Request.Headers.TryGetValue("Authorization", out var token) && token == "Bearer demo-token-compA")
        {
            context.Items["CompanyId"] = "compA";
        }
        await _next(context);
    }
}