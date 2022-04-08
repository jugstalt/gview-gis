using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;

namespace gView.Server.AppCode.Configuration
{
    static public class ConfigurationExtensions
    {
        private static ConcurrentBag<IConfigParser> _parsers = new ConcurrentBag<IConfigParser>();

        public static void AddParser(this IConfigParser configParser)
        {
            _parsers.Add(configParser);
        }

        public static string GetParsedValue(this IConfiguration config, string key)
        {
            string val = config[key];

            if (!String.IsNullOrEmpty(val))
            {
                foreach (var parser in _parsers)
                {
                    val = parser.Parse(val);
                }
            }

            return val;
        }

        public static IConfiguration BuildConfigParsers(this IConfiguration configuration)
        {
            return new ConfigurationParserBuilder().Build(configuration);
        }
    }
}
