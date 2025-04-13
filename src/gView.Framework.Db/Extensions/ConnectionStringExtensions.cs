#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.Db.Extensions;

static public class ConnectionStringExtensions
{
    static public Dictionary<string, string> SqlServerParametersToAppend = new Dictionary<string, string>()
    {
        { "TrustServerCertificate", "false" },
        { "Encrypt", "true" },
        //{ "Integrated Security", "false" }
    };

    static public void SetSqlServerParametersToAppend(this string[]? parameters)
    {
        if(parameters == null) // use the defaults
        {
            return;
        }

        SqlServerParametersToAppend.Clear();

        foreach (var parameter in parameters)
        {
            var keyValue = parameter.Split('=');
            if (keyValue.Length == 2)
            {
                SqlServerParametersToAppend.Add(keyValue[0].Trim(), keyValue[1].Trim());
            }
        }
    }

    static public string AppendSqlServerParametersIfNotExists(this string connectionString)
    {
        var sb = new StringBuilder(connectionString);

        var connectionStringWithoutSpaces = (connectionString ?? "").Replace(" ", "").Trim();
        if (!connectionStringWithoutSpaces.EndsWith(";"))
        {
            sb.Append(";");
        }

        foreach (var parameter in SqlServerParametersToAppend)
        {
            if (!connectionStringWithoutSpaces.Contains($"{parameter.Key}=", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($"{parameter.Key}={parameter.Value};");
            }
        }

        return sb.ToString();
    }

    static public string ParseNpgsqlConnectionString(this string connectionString)
    {
        string[] knownKeywords = new string[]
        {
                "host",
                "server",
                "port",
                "database",
                "userid",
                "username",
                "password",

                "pooling",
                "minpoolsize",
                "maxpoolsize",

                "timeout",
                "sslmode"
        };

        Dictionary<string, string> keywordTranslation = new Dictionary<string, string>()
            {
                { "server", "host" },
                { "userid","username" }
            };

        StringBuilder sb = new StringBuilder();

        foreach (var keywordParameter in connectionString.Split(';'))
        {
            if (keywordParameter.Contains("="))
            {
                var keyword = keywordParameter.Substring(0, keywordParameter.IndexOf("=")).Trim().ToLower();
                if (knownKeywords.Contains(keyword))
                {
                    string kp = keywordParameter;
                    if (keywordTranslation.ContainsKey(keyword))
                    {
                        kp = keywordTranslation[keyword] + keywordParameter.Substring(keywordParameter.IndexOf("="));
                    }

                    sb.Append(kp);
                    sb.Append(";");
                }
            }
        }

        return sb.ToString();
    }
}
