using Microsoft.AspNetCore.Routing.Patterns;
using System.Reflection;

public interface IClassRequestHandler
{
    MethodInfo? MapMethodInfo(HttpContext context, RoutePattern pattern, RouteData route);
}
public interface IClassDelegateRequestHandler
{
    Delegate? MapDelegate(HttpContext context, RoutePattern pattern, RouteData route);
}
