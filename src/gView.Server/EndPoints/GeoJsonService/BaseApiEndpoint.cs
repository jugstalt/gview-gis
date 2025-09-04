#nullable enable

using gView.Endpoints.Abstractions;
using gView.Framework.Common;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Data.Extensions;
using gView.Framework.GeoJsonService.Request;
using gView.GeoJsonService;
using gView.GeoJsonService.DTOs;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.GeoJsonService;

public class BaseApiEndpoint : IApiEndpoint
{
    private static GeoJsonServiceRequestInterpreter RequestInterpreter = new GeoJsonServiceRequestInterpreter();
    private readonly string[] _routes;
    private readonly Delegate _delegate;
    private readonly HttpMethod _httpMethods;

    protected BaseApiEndpoint(string route, Delegate @delegate, HttpMethod httpMethods = HttpMethod.Get)
    {
        _routes = [route];
        _delegate = @delegate;
        _httpMethods = httpMethods;
    }

    protected BaseApiEndpoint(string[] routes, Delegate @delegate, HttpMethod httpMethods = HttpMethod.Get)
    {
        _routes = routes;
        _delegate = @delegate;
        _httpMethods = httpMethods;
    }

    virtual protected RouteHandlerBuilder BuildEndpoint(RouteHandlerBuilder builder)
        => builder;

    public void Register(IEndpointRouteBuilder app)
    {
        if (_httpMethods.HasFlag(HttpMethod.Get))
        {
            foreach (var route in _routes)
            {
                BuildEndpoint(app.MapGet(route, _delegate));
            }
        }
        if (_httpMethods.HasFlag(HttpMethod.Post))
        {
            foreach (var route in _routes)
            {
                BuildEndpoint(app.MapPost(route, _delegate));
            }
        }
        if (_httpMethods.HasFlag(HttpMethod.Put))
        {
            foreach (var route in _routes)
            {
                BuildEndpoint(app.MapPut(route, _delegate));
            }
        }
        if (_httpMethods.HasFlag(HttpMethod.Delete))
        {
            foreach (var route in _routes)
            {
                BuildEndpoint(app.MapDelete(route, _delegate));
            }
        }
    }

    async static protected Task<object> HandleSecureAsync(
                HttpContext httpContext,
                MapServiceManager mapServiceManager,
                LoginManager loginManagerService,
                ILogger logger,
                string folder,
                string service,
                Func<IServiceRequestContext?, IMapService?, Identity, Task<object>> action)
    {
        object? result = null;
        int? statusCode = null;

        try
        {
            var authToken = loginManagerService.GetAuthToken(httpContext.Request);
            var identity = new Identity(authToken.Username, authToken.IsManageUser);


            ServiceRequest serviceRequest = ServiceRequest.CreateGerneral(
                    method: httpContext.Request.Path.Value?.Split("/").Last().ToLowerInvariant(),
                    identity: identity);

            IMapService? mapService = null;
            IServiceRequestContext? serviceRequestContext = null;

            if (!String.IsNullOrEmpty(service))
            {
                serviceRequestContext = await ServiceRequestContext.TryCreate(
                    mapServiceManager.Instance,
                    RequestInterpreter,
                    serviceRequest, checkSecurity: false);

                // todo: Folder specific settings?
                serviceRequest.OnlineResource = mapServiceManager.Options.OnlineResource;
                serviceRequest.OutputUrl = mapServiceManager.Options.OutputUrl;

                mapService = mapServiceManager.Instance.GetMapService(service, folder);
                if (mapService == null)
                {
                    throw new MapServerException("Unknown service");
                }

                await mapService.CheckAccess(serviceRequestContext);
            }

            result = action(serviceRequestContext, mapService, identity).GetAwaiter().GetResult();
        }
        catch (MapServerAuthException)
        {
            throw; // Handled in AuthenticationExceptionMiddleware
        }
        catch (MapServerException mse)
        {
            logger.LogWarning("Handle GeoJson Service Request: {message}", mse.Message);

            statusCode = 400;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = mse.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Handle GeoJson Service Request: {message}", ex.Message);

            statusCode = 500;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = "internal server error"
            };
        }

        return Results.Json(result, GeoJsonSerializer.JsonSerializerOptions, statusCode: statusCode);
    }

    async static protected Task<object> HandleSecureAsync<T>(
                HttpContext httpContext,
                MapServiceManager mapServiceManager,
                LoginManager loginManagerService,
                ILogger logger,
                string folder,
                string service,
                Func<IServiceRequestContext, IMapService, Identity, T, Task<object>> action)
    {
        object? result = null;
        int? statusCode = null;

        try
        {
            var authToken = loginManagerService.GetAuthToken(httpContext.Request);
            var identity = new Identity(authToken.Username, authToken.IsManageUser);

            T? model = await httpContext.GetModel<T>();

            ServiceRequest serviceRequest = ServiceRequest.CreateGerneral(
                    method: httpContext.Request.Path.Value?.Split("/").Last().ToLowerInvariant(),
                    identity: identity);

            // todo: Folder specific settings?
            serviceRequest.OnlineResource = mapServiceManager.Options.OnlineResource;
            serviceRequest.OutputUrl = mapServiceManager.Options.OutputUrl;

            var serviceRequestContext = await ServiceRequestContext.TryCreate(
                mapServiceManager.Instance,
                RequestInterpreter,
                serviceRequest, checkSecurity: false);

            var mapService = mapServiceManager.Instance.GetMapService(service, folder);
            if (mapService == null)
            {
                throw new MapServerException("Unknown service");
            }

            await mapService.CheckAccess(serviceRequestContext);

            result = await action(serviceRequestContext, mapService, identity, model!);
        }
        catch (MapServerAuthException)
        {
            throw; // Handled in AuthenticationExceptionMiddleware
        }
        catch (MapServerException mse)
        {
            logger.LogWarning("Handle GeoJson Service Request: {message}", mse.Message);

            statusCode = mse.StatusCode ?? 400;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = mse.Message
            };
        }
        catch (SqlDangerousStatementExceptions dse)
        {
            logger.LogWarning("Handle GeoJson Service Request: {message}", dse.Message);

            statusCode = 400;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = dse.Message
            };
        }
        catch (FormatException fex)
        {
            logger.LogError("Handle GeoJson Service Request: {message}", fex.Message);

            statusCode = 400;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = fex.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Handle GeoJson Service Request: {message}", ex.Message);

            statusCode = 500;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = "internal server error"
            };
        }

        return result is IResult
            ? result
            : Results.Json(result, GeoJsonSerializer.JsonSerializerOptions, statusCode: statusCode);
    }

    async static protected Task<object> HandleSecureAsync<T>(
                HttpContext httpContext,
                ILogger logger,
                Func<T, Task<object>> action)
    {
        object? result = null;
        int? statusCode = null;

        try
        {
            T? model = await httpContext.GetModel<T>();

            result = await action(model!);
        }
        catch (MapServerAuthException)
        {
            throw; // Handled in AuthenticationExceptionMiddleware
        }
        catch (MapServerException mse)
        {
            logger.LogWarning("Handle GeoJson Service Request: {message}", mse.Message);

            statusCode = 400;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = mse.Message
            };
        }
        catch (SqlDangerousStatementExceptions dse)
        {
            logger.LogWarning("Handle GeoJson Service Request: {message}", dse.Message);

            statusCode = 400;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = dse.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Handle GeoJson Service Request: {message}", ex.Message);

            statusCode = 500;
            result = new ErrorResponse()
            {
                ErrorCode = statusCode.Value,
                ErrorMessage = "internal server error"
            };
        }

        return result is IResult
            ? result
            : Results.Json(result, GeoJsonSerializer.JsonSerializerOptions, statusCode: statusCode);
    }
}
