using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

public class ClassRequestHandlerFactory : IClassRequestHandlerFactory
{
    private IOptions<RouteHandlerOptions> _options;
    private IServiceProvider _provider;
    public ClassRequestHandlerFactory(IOptions<RouteHandlerOptions> options,IServiceProvider serviceProvider)
    {
        _options = options;
        _provider = serviceProvider;
    }
    ConcurrentDictionary<string,RequestDelegate> requestDelegates = new ConcurrentDictionary<string,RequestDelegate>(StringComparer.InvariantCultureIgnoreCase);
    public RequestDelegate CreateHandler<T>(RoutePattern pattern) where T : IClassRequestHandler
    {
        return async context => {
            var stop = context.RequestServices.GetRequiredService<Stopwatch>();
            stop.Start();
            var obj = context.RequestServices.GetRequiredService<T>();
            var methodInfo = obj.MapMethodInfo(pattern, context.GetRouteData());
            var handler = requestDelegates.GetOrAdd(context.Request.Path.ToString(), createRequestDelegate);
            await handler(context);
            Console.WriteLine($"{context.Request.Method} Request:{context.Request.Path} {stop.ElapsedMilliseconds}ms");
            RequestDelegate createRequestDelegate(string path)
            {
                var options = new RequestDelegateFactoryOptions
                {
                    ServiceProvider = _provider,
                    RouteParameterNames = pattern.Parameters.Select(a => a.Name),
                    ThrowOnBadRequest = _options?.Value.ThrowOnBadRequest ?? false,
                    DisableInferBodyFromParameters = false,
                };
                if (methodInfo == null)
                {
                    return RequestDelegateFactory.Create(() => Results.NotFound("未找到"), options).RequestDelegate;
                }
                return RequestDelegateFactory.Create(methodInfo!,context=> obj, options).RequestDelegate;
            }
        };
    }
}
