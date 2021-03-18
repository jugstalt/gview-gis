using gView.Core.Framework.system;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Configuration
{
    public class ConfigurationParserBuilder
    {
        public IConfiguration Build(IConfiguration configuration)
        {
            if (!String.IsNullOrWhiteSpace(configuration["AZURE_AD_ClientId"]) &&
                !String.IsNullOrWhiteSpace(configuration["AZURE_AD_ClientSecret"]))
            {
                var keyVault = new gView.Framework.Azure.KeyVault.KeyVault(
                    configuration["AZURE_AD_ClientId"],
                    configuration["AZURE_AD_ClientSecret"]);

                Console.WriteLine("Add AzureKeyVaultConfigValueParser");
                new KeyVaultConfigParser(keyVault).AddParser();
            }

            return configuration;
        }
    }
}
