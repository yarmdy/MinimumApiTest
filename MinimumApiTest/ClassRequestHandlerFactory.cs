using Microsoft.AspNetCore.Mvc.Filters;
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
        T? obj = default;
        return async context => {
            if (obj == null)
            {
                lock (this)
                {
                    if (obj == null)
                    {
                        obj = context.RequestServices.GetRequiredService<T>();
                    }
                }
            }
            
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
            T obj2 = context.RequestServices.GetRequiredService<T>();
            var itemName = $"RequestDelegateObject_{obj2.GetType().FullName}_{context.Request.Path}";
            context.Items[itemName] = obj2;
            var handler = requestDelegates.GetOrAdd(context.Request.Path.ToString(), createRequestDelegate);
            await handler(context);
            RequestDelegate createRequestDelegate(string path)
            {
                var builder = new RouteEndpointBuilder(null, pattern, 0);
                if(obj is IEndpointFilter)
                {
                    builder.FilterFactories.Add((context, next) => async context => await (context.HttpContext.Items[itemName] as IEndpointFilter)!.InvokeAsync(context, next));
                }
                var options = new RequestDelegateFactoryOptions
                {
                    ServiceProvider = _provider,
                    RouteParameterNames = pattern.Parameters.Select(a => a.Name),
                    ThrowOnBadRequest = _options?.Value.ThrowOnBadRequest ?? false,
                    DisableInferBodyFromParameters = false,
                    EndpointBuilder = builder
                };
                
                RequestDelegateResult result;
                if (methodDelegate != null)
                {
                    result= RequestDelegateFactory.Create(methodDelegate!.Method, context => context.Items[itemName]!, options);
                }else
                if (methodInfo != null)
                {
                    result= RequestDelegateFactory.Create(methodInfo!, context => context.Items[itemName]!, options);
                }
                else
                {
                    result = RequestDelegateFactory.Create(() => Results.NotFound("未找到"), options);
                }
                
                return result.RequestDelegate;   
            }
        };
    }
}
