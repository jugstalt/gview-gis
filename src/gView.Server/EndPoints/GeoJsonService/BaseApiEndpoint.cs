#nullable enable

using gView.Endpoints.Abstractions;
using gView.Framework.Common;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService;
using gView.GeoJsonService.DTOs;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.GeoJsonService;

public class BaseApiEndpoint : IApiEndpoint
{
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

    public void Register(IEndpointRouteBuilder app)
    {
        if (_httpMethods.HasFlag(HttpMethod.Get))
        {
            foreach (var route in _routes)
            {
                app.MapGet(route, _delegate);
            }
        }
        if (_httpMethods.HasFlag(HttpMethod.Post))
        {
            foreach (var route in _routes)
            {
                app.MapPost(route, _delegate);
            }
        }
        if (_httpMethods.HasFlag(HttpMethod.Put))
        {
            foreach (var route in _routes)
            {
                app.MapPut(route, _delegate);
            }
        }
        if (_httpMethods.HasFlag(HttpMethod.Delete))
        {
            foreach (var route in _routes)
            {
                app.MapDelete(route, _delegate);
            }
        }
    }

    static protected object HandleSecureAsync(
                HttpContext httpContext,
                LoginManager loginManagerService,
                Func<Identity, Task<object>> action)
    {
        object? result = null;

        try
        {
            var authToken = loginManagerService.GetAuthToken(httpContext.Request);
            var identity = new Identity(authToken.Username, authToken.IsManageUser);

            result = action(identity).GetAwaiter().GetResult();
        }
        catch (MapServerAuthException)
        {
            throw; // Handled in AuthenticationExceptionMiddleware
        }
        catch (MapServerException mse)
        {
            result = new ErrorResponse()
            {
                ErrorCode = 999,
                ErrorMessage = mse.Message
            };
        }
        catch (Exception)
        {
            result = new ErrorResponse()
            {
                ErrorCode = 999,
                ErrorMessage = "Unknow error"
            };
        }

        return Results.Json(result, GeoJsonSerializer.JsonSerializerOptions);
    }

    static protected object HandleSecureAsync<T>(
                HttpContext httpContext,
                LoginManager loginManagerService,
                Func<Identity, T, Task<object>> action)
    {
        object? result = null;

        try
        {
            var authToken = loginManagerService.GetAuthToken(httpContext.Request);
            var identity = new Identity(authToken.Username, authToken.IsManageUser);

            T? model;

            if (httpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                model = Activator.CreateInstance<T>();

                foreach (var propertyInfo in typeof(T).GetProperties())
                {
                    string? parameter = httpContext.Request.Query[propertyInfo.Name];
                    if (!String.IsNullOrEmpty(parameter))
                    {
                        object? val = propertyInfo.PropertyType switch
                        {
                            Type t when t == typeof(int) => int.Parse(parameter),
                            Type t when t == typeof(float) => parameter.ToFloat(),
                            Type t when t == typeof(double) => parameter.ToDouble(),
                            Type t when t == typeof(long) => long.Parse(parameter),
                            Type t when t == typeof(bool) => bool.Parse(parameter),
                            Type t when t == typeof(string) => parameter,
                            _ => JsonSerializer.Deserialize(parameter, propertyInfo.PropertyType, GeoJsonSerializer.JsonDeserializerOptions)
                        };

                        propertyInfo.SetValue(model, val);
                    }
                }
            }
            else
            {
                using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
                string requestBody = reader.ReadToEndAsync().GetAwaiter().GetResult();
                //httpContext.Request.Body.Position = 0;

                try
                {
                    model = GeoJsonSerializer.Deserialize<T>(requestBody)!;
                }
                catch (Exception ex)
                {
                    throw new MapServerException($"Parse Error: {ex.Message}");
                }
            }

            result = action(identity, model).GetAwaiter().GetResult();
        }
        catch (MapServerAuthException)
        {
            throw; // Handled in AuthenticationExceptionMiddleware
        }
        catch (MapServerException mse)
        {
            result = new ErrorResponse()
            {
                ErrorCode = 999,
                ErrorMessage = mse.Message
            };
        }
        catch (Exception)
        {
            result = new ErrorResponse()
            {
                ErrorCode = 999,
                ErrorMessage = "Unknow error"
            };
        }

        return result is IResult
            ? result
            : Results.Json(result, GeoJsonSerializer.JsonSerializerOptions);
    }
}
