using Microsoft.AspNetCore.Routing.Patterns;

public static class MinmumApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapClass<T>(this IEndpointRouteBuilder builder,string pattern) where T : IClassRequestHandler
    {
        var factory = builder.ServiceProvider.GetRequiredService<IClassRequestHandlerFactory>();
        return builder.Map(pattern,factory.CreateHandler<T>(RoutePatternFactory.Parse(pattern)));
    }
}
