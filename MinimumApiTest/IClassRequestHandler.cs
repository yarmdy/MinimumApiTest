using Microsoft.AspNetCore.Routing.Patterns;
using System.Reflection;

public interface IClassRequestHandler
{
    MethodInfo? MapMethodInfo(RoutePattern pattern, RouteData route);
}
