using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class HomeClass : ClassRequestHandler
{
    AsyncLocal<DateTime> _date;
    public HomeClass() 
    {
        _date=new AsyncLocal<DateTime>();
        _date.Value = DateTime.Now;
    }
    public async Task<IResult> Index()
    {
        return await Task.FromResult(Results.Content($"首页{_date.Value}"));
    }

    public IResult About([StringLength(10,ErrorMessage = "不能超10位")] string content)
    {
        return Results.Content(content);
    }
}
