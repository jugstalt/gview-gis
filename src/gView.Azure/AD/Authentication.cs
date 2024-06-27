using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Azure.AD;

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
        
        //var authContext = new AuthenticationContext(authority);
        //ClientCredential clientCred = new ClientCredential(this.ClientId, this.ClientSecret);
        //AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

        if (result == null)
        {
            throw new InvalidOperationException("Failed to obtain the Azure AD JWT token");
        }

        return result.AccessToken;
    }
}
