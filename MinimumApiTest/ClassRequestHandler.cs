using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Patterns;
using System.Collections.Concurrent;
using System.Reflection;

public abstract class ClassRequestHandler : IClassRequestHandler
{
    private static ConcurrentDictionary<string,MethodInfo?> methods = new ConcurrentDictionary<string,MethodInfo?>();
    private static readonly HashSet<string> noMapMethodNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
        nameof(IClassDelegateRequestHandler.MapDelegate),
        nameof(IClassRequestHandler.MapMethodInfo),
    };
    public virtual MethodInfo? MapMethodInfo(HttpContext context, RoutePattern pattern, RouteData route)
    {
        var patternName = pattern.Parameters.Where(a => a.IsParameter && !a.IsOptional && !a.IsCatchAll).LastOrDefault()?.Name;
        if (patternName == null)
        {
            return null;
        }
        var action = route.Values[patternName];
        if (action == null)
        {
            return null;
        }
        if (noMapMethodNames.Contains(action + ""))
        {
            return null;
        }
        return methods.GetOrAdd(context.Request.Path, path => {
            var method =  GetType().GetMethod(action+"", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (method == null)
            {
                return null;
            }
            var attr = method.GetCustomAttribute<NonActionAttribute>(true);
            if (attr != null)
            {
                return null;
            }
            return method;
        });
   }
}
