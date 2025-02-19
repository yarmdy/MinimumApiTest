using Microsoft.AspNetCore.Routing.Patterns;
using System.Reflection;

public class StudentClass : IClassRequestHandler
{
    public MethodInfo? MapMethodInfo(RoutePattern pattern, RouteData route)
    {
        var action = route.Values["action"];
        return action switch
        {
            "List" => GetType().GetMethod("List"),
            _ => null
        };
    }
    public IResult List() {
        return Results.Content("学生列表");
    }
}