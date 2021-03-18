using gView.Core.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Configuration
{
    public class KeyVaultConfigParser : IConfigParser
    {
        private readonly IKeyVault _keyVault;

        public KeyVaultConfigParser(IKeyVault keyVault)
        {
            _keyVault = keyVault;
        }

        public string Parse(string configValue)
        {
            if (!configValue.Contains("kv:"))
            {
                return configValue;
            }

            if (configValue.StartsWith("kv:"))
            {
                configValue = _keyVault.Secret(configValue.Substring("kv:".Length));
            }
            if (configValue.Contains("{{") && configValue.Contains("}}"))
            {
                foreach (Match match in Regex.Matches(configValue, @"\{{(.*?)\}}"))
                {
                    var matchKey = match.Value.Substring(2, match.Value.Length - 4);

                    if (matchKey.StartsWith("kv:"))
                    {
                        string kvValue = _keyVault.Secret(matchKey.Substring("kv:".Length));
                        configValue = configValue.Replace(match.Value, kvValue);
                    }
                }
            }

            return configValue;
        }
    }
}
