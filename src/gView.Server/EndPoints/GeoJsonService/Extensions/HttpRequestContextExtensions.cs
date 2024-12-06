#nullable enable

using gView.Framework.Common;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class HttpRequestContextExtensions
{
    async static public ValueTask<T?> GetModel<T>(this HttpContext httpContext)
    {
        if (httpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            T model = Activator.CreateInstance<T>();

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

                        Type t when t == typeof(int?) => String.IsNullOrEmpty(parameter) ? null : int.Parse(parameter),
                        Type t when t == typeof(float?) => String.IsNullOrEmpty(parameter) ? null : parameter.ToFloat(),
                        Type t when t == typeof(double?) => String.IsNullOrEmpty(parameter) ? null : parameter.ToDouble(),
                        Type t when t == typeof(long?) => String.IsNullOrEmpty(parameter) ? null : long.Parse(parameter),
                        Type t when t == typeof(bool?) => String.IsNullOrEmpty(parameter) ? null : bool.Parse(parameter),

                        Type t when t.IsEnum => Enum.Parse(t, parameter, true),
                        _ => parameter.ParseToObject(propertyInfo.PropertyType)
                    };

                    propertyInfo.SetValue(model, val);
                }
            }

            return model;
        }

        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        string requestBody = await reader.ReadToEndAsync();
        //httpContext.Request.Body.Position = 0;

        try
        {
            return GeoJsonSerializer.Deserialize<T>(requestBody)!;
        }
        catch (Exception ex)
        {
            throw new MapServerException($"Parse Error: {ex.Message}");
        }
    }
}
