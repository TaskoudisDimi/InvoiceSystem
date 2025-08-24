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
        // Check and process Authorization header
        if (context.Request.Headers.TryGetValue("Authorization", out var tokenValues) && tokenValues.Count > 0)
        {
            var token = tokenValues[0]?.Trim();
            if (token != null)
            {
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var tokenValue = token["Bearer ".Length..].Trim();
                    if (tokenValue == "demo-token-compA")
                    {
                        context.Items["CompanyId"] = "compA";
                    }
                }
            }
        }
        await _next(context);
    }
}