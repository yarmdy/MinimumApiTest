public class HomeClass : ClassRequestHandler
{
    public async Task<IResult> Index()
    {
        return await Task.FromResult(Results.Content("首页"));
    }

    public IResult About(string content)
    {
        return Results.Content(content);
    }
}
