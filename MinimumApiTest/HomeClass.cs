using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class HomeClass : ClassRequestHandler,IEndpointFilter
{
    AsyncLocal<DateTime> _date;
    DateTime dt;
    public HomeClass() 
    {
        _date=new AsyncLocal<DateTime>();
        _date.Value = DateTime.Now;
        dt = DateTime.Now;
    }
    public async Task<IResult> Index()
    {
        return await Task.FromResult(Results.Content($"首页{_date.Value}"));
    }
    private async Task asdAsync()
    {
        await Task.Delay(1000);
    }
    public IResult About([StringLength(10,ErrorMessage = "不能超10位")] string content)
    {
        return Results.Content(content);
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        Console.WriteLine(dt);
        return result;
    }
}
