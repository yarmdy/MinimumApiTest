using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IClassRequestHandlerFactory, ClassRequestHandlerFactory>();
builder.Services.AddTransient<HomeClass>();
builder.Services.AddTransient<StudentClass>();
builder.Services.AddScoped<Stopwatch>();

// Add services to the container.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.MapGet("help",()=>Results.Content("帮助"));
app.MapClass<HomeClass>("home/{action}");
app.MapClass<StudentClass>("student/{action}");
app.Run();




public static class MinmumApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapClass<T>(this IEndpointRouteBuilder builder,string pattern) where T : IClassRequestHandler
    {
        var factory = builder.ServiceProvider.GetRequiredService<IClassRequestHandlerFactory>();
        return builder.Map(pattern,factory.CreateHandler<T>(RoutePatternFactory.Parse(pattern)));
    }
}

public interface IClassRequestHandlerFactory
{
    RequestDelegate CreateHandler<T>(RoutePattern pattern) where T: IClassRequestHandler;
}
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
public interface IClassRequestHandler
{
    MethodInfo? MapMethodInfo(RoutePattern pattern, RouteData route);
}

public abstract class ClassRequestHandler : IClassRequestHandler
{
    private ConcurrentDictionary<string,MethodInfo?> methods = new ConcurrentDictionary<string,MethodInfo?>();
    public virtual MethodInfo? MapMethodInfo(RoutePattern pattern, RouteData route)
    {
        var patternName = pattern.Parameters.Where(a => a.IsParameter && !a.IsOptional && !a.IsCatchAll).LastOrDefault()?.Name;
        if (patternName == null)
        {
            return null;
        }
        var action = route.Values[patternName];
        if (action == null)
        {
            return null;
        }
        return methods.GetOrAdd(action + "", action => {
            var method = GetType().GetMethod(action, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (method == null)
            {
                return null;
            }
            var rt = method.ReturnType;
            if (rt.IsAssignableFrom(typeof(IResult)))
            {
                return method;
            }
            if (!rt.IsAssignableTo(typeof(Task)))
            {
                return null;
            }
            if (!rt.IsConstructedGenericType || rt.GenericTypeArguments.Length != 1)
            {
                return null;
            }
            if (!rt.GenericTypeArguments[0].IsAssignableFrom(typeof(IResult)))
            {
                return null;
            }
            return method;
        });
   }
}

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