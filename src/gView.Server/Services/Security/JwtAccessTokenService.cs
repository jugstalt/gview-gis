using gView.Framework.Core.Network;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace gView.Server.Services.Security;

public class JwtAccessTokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly TokenValidationParameters _validationParameters;
    private readonly IMemoryCache _memoryCache;
    private readonly string _issuer;
    
    public JwtAccessTokenService(
                IConfiguration config, 
                EncryptionCertificateService ecs)
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = 1024
        });

        _key = new SymmetricSecurityKey(ecs.GetCertificate().AESPassword);
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer = config["Jwt:Issuer"] ?? "gview-server",

            ValidateAudience = false,
            //ValidAudience = audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
        };
    }

    public string GenerateToken(AuthToken authToken)
    {
        var claimsPrincipal = authToken.ToClaimsPrincipal();
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claimsPrincipal.Identity),
            Expires = new DateTime(authToken.Expire, DateTimeKind.Utc),
            SigningCredentials = credentials,
            Issuer = _issuer
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        if (_memoryCache.TryGetValue(token, out ClaimsPrincipal cachedPrincipal))
        {
            return cachedPrincipal;
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(
                                token, 
                                _validationParameters, 
                                out SecurityToken validatedToken
                            );

            if (validatedToken is JwtSecurityToken jwtToken)
            {
                if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token algorithm");
                }
            }

            var expiresIn = principal.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (expiresIn != null && long.TryParse(expiresIn, out var exp))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = expirationTime
                }.SetSize(1);

                _memoryCache.Set(token, principal, cacheEntryOptions);
            }

            return principal;
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Token validation failed", ex);
        }
    }

}
