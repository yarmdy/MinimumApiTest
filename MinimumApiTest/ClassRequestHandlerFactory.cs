using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

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
            var obj = context.RequestServices.GetRequiredService<T>();
            MethodInfo? methodInfo = null;
            Delegate? methodDelegate = null;
            if(obj is IClassDelegateRequestHandler objDelegate)
            {
                methodDelegate = objDelegate.MapDelegate(context, pattern, context.GetRouteData());
            }
            else
            {
                methodInfo = obj.MapMethodInfo(context, pattern, context.GetRouteData());
            }
            var itemName = $"RequestDelegateObject_{obj.GetType().FullName}_context.Request.Path";
            context.Items[itemName] = obj;
            var handler = requestDelegates.GetOrAdd(context.Request.Path.ToString(), createRequestDelegate);
            await handler(context);
            RequestDelegate createRequestDelegate(string path)
            {
                var options = new RequestDelegateFactoryOptions
                {
                    ServiceProvider = _provider,
                    RouteParameterNames = pattern.Parameters.Select(a => a.Name),
                    ThrowOnBadRequest = _options?.Value.ThrowOnBadRequest ?? false,
                    DisableInferBodyFromParameters = false,
                };
                if (methodDelegate != null)
                {
                    return RequestDelegateFactory.Create(methodDelegate!, options).RequestDelegate;
                }
                if (methodInfo != null)
                {
                    return RequestDelegateFactory.Create(methodInfo!, context => context.Items[itemName]!, options).RequestDelegate;
                }
                return RequestDelegateFactory.Create(() => Results.NotFound("未找到"), options).RequestDelegate;
                
            }
        };
    }
}
