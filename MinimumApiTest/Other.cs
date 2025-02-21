using Microsoft.AspNetCore.Routing.Patterns;
using System.Reflection;

public class Other : ClassRequestHandler,IClassDelegateRequestHandler
{
    AsyncLocal<DateTime> _date;
    public Other()
    {
        _date = new AsyncLocal<DateTime>();
        _date.Value = DateTime.Now;
    }
    public IResult Index(string? afterUrl)
    {
        return Results.Content($"URL后缀（{_date.Value}）：{afterUrl}");
    }

    public Delegate? MapDelegate(HttpContext context, RoutePattern pattern, RouteData route)
    {
        return Index;
    }
}