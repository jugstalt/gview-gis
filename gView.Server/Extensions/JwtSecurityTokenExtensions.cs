using gView.Server.Exceptions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Server.Extensions
{
    static public class JwtSecurityTokenExtensions
    {
        static public JwtSecurityToken ToJwtSecurityToken(this string jwtEncodedString)
        {
            return new JwtSecurityToken(jwtEncodedString);
        }

        async static public Task<JwtSecurityToken> ToValidatedJwtSecurityToken(this string jwtEncodedString, string issuerUrl, string audience = null)
        {
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
                   new ConfigurationManager<OpenIdConnectConfiguration>($"{issuerUrl.ToValidIssuerUrl()}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfiguration = await configurationManager.GetConfigurationAsync(CancellationToken.None);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters()
            {
                ValidIssuer = issuerUrl.ToValidIssuerUrl(),
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                ValidateAudience = !String.IsNullOrEmpty(audience),
                IssuerSigningKeys = openIdConfiguration.SigningKeys
            };

            try
            {
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(jwtEncodedString, validationParameters, out validatedToken);
            }
            catch (Exception ex)
            {
                throw new TokenValidationException($"Invalid token: {ex.Message}");
            }
            return jwtEncodedString.ToJwtSecurityToken();
        }

        private static string ToValidIssuerUrl(this string issuerUrl)
        {
            issuerUrl = issuerUrl.Trim();
            while (issuerUrl.EndsWith("/"))
            {
                issuerUrl = issuerUrl.Substring(0, issuerUrl.Length - 1);
            }

            return issuerUrl;
        }
    }
}
