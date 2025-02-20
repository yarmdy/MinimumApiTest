using Microsoft.AspNetCore.Routing.Patterns;
using System.Reflection;

public class Other : ClassRequestHandler,IClassDelegateRequestHandler
{
    public IResult Index(string? afterUrl)
    {
        return Results.Content($"URL后缀：{afterUrl}");
    }

    public Delegate? MapDelegate(HttpContext context, RoutePattern pattern, RouteData route)
    {
        return Index;
    }
}