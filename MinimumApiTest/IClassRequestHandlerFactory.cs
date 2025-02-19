using Microsoft.AspNetCore.Routing.Patterns;

public interface IClassRequestHandlerFactory
{
    RequestDelegate CreateHandler<T>(RoutePattern pattern) where T: IClassRequestHandler;
}
