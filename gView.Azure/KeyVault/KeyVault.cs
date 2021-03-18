using gView.Core.Framework.system;
using Microsoft.Azure.KeyVault;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Azure.KeyVault
{
    public class KeyVault : IKeyVault
    {
        public KeyVault(string clientId, string clientSecret)
        {
            var authentication = new AD.Authentication(clientId, clientSecret);
            this.Client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(authentication.GetToken));
        }

        private KeyVaultClient Client { get; set; }

        async public Task<string> SecretAsync(string uri)
        {
            var sec = await this.Client.GetSecretAsync(uri);
            return sec.Value;
        }

        public string Secret(string uri)
        {
            return SecretAsync(uri).Result;
        }
    }
}
