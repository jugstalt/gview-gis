using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.Framework.IO
{
    public class ConfigTextStream
    {
        #region Static Members

        public static string ExtractValue(string Params, string Param)
        {
            Param = Param.Trim();

            foreach (string a in Params.Split(';'))
            {
                string aa = a.Trim();
                if (aa.ToLower().IndexOf(Param.ToLower() + "=") == 0)
                {
                    if (aa.Length == Param.Length + 1)
                    {
                        return "";
                    }

                    return aa.Substring(Param.Length + 1, aa.Length - Param.Length - 1);
                }
            }
            return String.Empty;
        }

        public static string ExtractOracleValue(string @params, string param)
        {
            @params = @params.Trim();
            int pos = @params.ToLower().IndexOf("(" + param.ToLower() + "=");
            if (pos >= 0)
            {
                int pos2 = @params.IndexOf(")", pos + param.Length + 1);
                if (pos2 > pos)
                {
                    return @params.Substring(pos + param.Length + 2, pos2 - pos - param.Length - 2);
                }
            }
            return String.Empty;
        }

        public static string RemoveValue(string Params, string Param)
        {
            Param = Param.ToLower().Trim();

            StringBuilder sb = new StringBuilder();
            foreach (string a in Params.Split(';'))
            {
                if (String.IsNullOrEmpty(a.Trim()) ||
                    a.ToLower().StartsWith(Param + "="))
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(";");
                }

                sb.Append(a);
            }
            return sb.ToString();
        }

        public static string BuildLine(string id, string config)
        {
            return id + ":" + config;
        }

        public static Dictionary<string, string> Extract(string Params)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (string keyValue in Params.Split(';')
                                              .Select(p => p.Trim())
                                              .Where(p => !String.IsNullOrEmpty(p)))
            {
                int pos = keyValue.IndexOf("=");
                if (pos == -1)
                {
                    parameters.Add(keyValue, null);
                }
                else
                {
                    parameters.Add(
                        keyValue.Substring(0, pos),
                        keyValue.Substring(pos + 1, keyValue.Length - pos - 1));
                }
            }
            return parameters;
        }

        public static string Build(IDictionary<string, string> keyValues)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in keyValues.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append(";");
                }

                sb.Append($"{key}={keyValues[key]}");
            }

            return sb.ToString();
        }

        public static string SecureConfig(string Params)
        {
            if (!Params.Contains("="))
            {
                return Params;
            }

            List<string> secure1 = new List<string>()/* { "user", "username", "userid", "usr", "usrid", "uid" }*/;
            List<string> secure2 = new List<string>() { "password", "pwd", "passwd", "passwrd", "pw" };
            Dictionary<string, string> parameters = Extract(Params);

            StringBuilder sb = new StringBuilder();
            foreach (string key in parameters.Keys)
            {
                if (key == String.Empty)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append(";");
                }

                if (secure2.Contains(key.Trim().ToLower()) ||
                    secure2.Contains(key.Trim().ToLower().Replace(" ", "")))
                {
                    sb.Append(key + "=********");
                }
                else if (secure1.Contains(key.Trim().ToLower()) ||
                    secure1.Contains(key.Trim().ToLower().Replace(" ", "")))
                {
                    string v = parameters[key], val = v;
                    if (v.Length > 2)
                    {
                        val = v[0].ToString();
                        for (int i = 1; i < v.Length - 2; i++)
                        {
                            val += "*";
                        }

                        val += v[v.Length - 1];
                    }
                    sb.Append(key + "=" + val);
                }
                else
                {
                    sb.Append(key + "=" + parameters[key]);
                }
            }

            return sb.ToString();
        }

        public static string SecureConfigValue(string paraneterName, string parameterValue)
        {
            List<string> secure = new List<string>() { "password", "pwd", "passwd", "passwrd", "pw" };

            if (secure.Contains(paraneterName?.ToLower()))
            {
                return "*************";
            }

            return parameterValue;
        }

        #endregion
    }
}
