using Microsoft.AspNetCore.Routing.Patterns;
using System.Reflection;

public class StudentClass : ClassRequestHandler,IClassDelegateRequestHandler
{
    public IResult List() {
        return Results.Content("学生列表");
    }
    public IResult Get(int id)
    {
        return Results.Content($"学生：{id}");
    }

    public Delegate? MapDelegate(RoutePattern pattern, RouteData route)
    {
        var action = (route.Values["action"]+"").ToLower();
        return action switch
        {
            "list" => List,
            "get"=>Get,
            _ => null
        };
    }
}