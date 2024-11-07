using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using gView.Framework.Core.Common;
//using Microsoft.Azure.KeyVault;
using System;
using System.Threading.Tasks;

//namespace gView.Framework.Azure.KeyVault
//{
//    public class KeyVault : IKeyVault
//    {
//        public KeyVault(string clientId, string clientSecret)
//        {
//            var authentication = new AD.Authentication(clientId, clientSecret);
//            this.Client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(authentication.GetToken));
//        }

//        private KeyVaultClient Client { get; set; }

//        async public Task<string> SecretAsync(string uri)
//        {
//            var sec = await this.Client.GetSecretAsync(uri);
//            return sec.Value;
//        }

//        public string Secret(string uri)
//        {
//            return SecretAsync(uri).Result;
//        }
//    }
//}

namespace gView.Framework.Azure.KeyVault
{
    public class KeyVault : IKeyVault
    {
        private readonly SecretClient _client;

        public KeyVault(string keyVaultUri, string clientId, string clientSecret, string tenantId)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _client = new SecretClient(new Uri(keyVaultUri), credential);
        }

        public async Task<string> SecretAsync(string secretName)
        {
            KeyVaultSecret secret = await _client.GetSecretAsync(secretName);
            return secret.Value;
        }

        public string Secret(string secretName)
        {
            return SecretAsync(secretName).GetAwaiter().GetResult();
        }
    }
}
