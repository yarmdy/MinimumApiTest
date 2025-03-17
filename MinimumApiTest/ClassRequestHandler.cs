﻿using Microsoft.AspNetCore.Routing.Patterns;
using System.Collections.Concurrent;
using System.Reflection;

public abstract class ClassRequestHandler : IClassRequestHandler
{
    private ConcurrentDictionary<string,MethodInfo?> methods = new ConcurrentDictionary<string,MethodInfo?>();
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
        return methods.GetOrAdd(action + "", action => GetType().GetMethod(action, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
   }
}
