//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;

namespace gView.Framework.Azure.AD
{
    public class Authentication
    {
        public Authentication(string clientId, string clientSecret)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
        }

        private string ClientId { get; set; }
        private string ClientSecret { get; set; }

        public async Task<string> GetToken(string authority, string resourceId, string scope)
        {
            // https://learn.microsoft.com/de-de/entra/msal/dotnet/how-to/migrate-confidential-client?tabs=daemon

            var app = ConfidentialClientApplicationBuilder
                                    .Create(this.ClientId)
                                    .WithClientSecret(this.ClientSecret)
                                    .WithAdfsAuthority(authority)
                                    .Build();

            

            AuthenticationResult result = await app
                                    .AcquireTokenForClient([$"{resourceId}/.default"])
                                    .ExecuteAsync();

            return result.AccessToken;

            //var authContext = new AuthenticationContext(authority);
            //ClientCredential clientCred = new ClientCredential(this.ClientId, this.ClientSecret);
            //AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            //if (result == null)
            //{
            //    throw new InvalidOperationException("Failed to obtain the JWT token");
            //}

            //return result.AccessToken;
        }
    }
}
