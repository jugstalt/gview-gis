using Microsoft.Extensions.Configuration;
using System;

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
                    configuration["AZURE_KeyVaultUri"],
                    configuration["AZURE_AD_ClientId"],
                    configuration["AZURE_AD_ClientSecret"],
                    configuration["AZURE_AD_TenantId"]);

                Console.WriteLine("Add AzureKeyVaultConfigValueParser");
                new KeyVaultConfigParser(keyVault).AddParser();
            }

            return configuration;
        }
    }
}
