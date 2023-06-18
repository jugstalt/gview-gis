using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace gView.Cmd.Core.Extensions;
static public class DictionaryExtensions
{
    static public bool HasKey(this IDictionary<string, object> parameters, string key)
        => parameters != null && parameters.ContainsKey(key);

    static public T? GetValue<T>(this IDictionary<string, object> parameters, string key, bool isRequired = false)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        if (!parameters.ContainsKey(key))
        {
            if (isRequired == true)
            {
                throw new ArgumentException($"parameter {key} is required");
            }

            return default(T);
        }

        return (T)Convert.ChangeType(parameters[key], typeof(T));
    }

    static public T GetRequiredValue<T>(this IDictionary<string, object> parameters, string key)
        => parameters.GetValue<T>(key, true) ??
                throw new Exception("Required parameter {key} can't be null");

    static public IEnumerable<T> GetArray<T>(this IDictionary<string, object> parameters, string key, bool isRequired =false)
    {
        var arrayString = parameters.GetValue<string>(key, isRequired);

        if (String.IsNullOrEmpty(arrayString)) 
        {
            yield break;
        }

        foreach (var val in arrayString!.Split(';'))
        {
            yield return (T)Convert.ChangeType(val, typeof(T));
        }
    }
}