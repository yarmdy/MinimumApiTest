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

    public IResult About(string content)
    {
        return Results.Content(content);
    }
}
