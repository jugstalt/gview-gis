using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace gView.Server.Services.Security;

public class JwtAccessTokenService
{
    private const string Issuer = "gview";
    private readonly EncryptionCertificateService _ecs;
    private readonly SymmetricSecurityKey _key;

    public JwtAccessTokenService(EncryptionCertificateService ecs)
    {
        _ecs = ecs;
        _key = new SymmetricSecurityKey(_ecs.GetCertificate().AESPassword);
    }

    public string GenerateToken(string userName, bool isTokenUser, int expireMinutes)
    {
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, userName),
                new Claim("isTokenUser", isTokenUser.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
            SigningCredentials = credentials,
            Issuer = Issuer
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, 
            ValidIssuer = Issuer,

            ValidateAudience = false, 
            //ValidAudience = audience,

            ValidateLifetime = true, 
            ClockSkew = TimeSpan.Zero, 

            ValidateIssuerSigningKey = true, 
            IssuerSigningKey = _key
        };

        try
        {
            var principal = tokenHandler.ValidateToken(
                                token, 
                                validationParameters, 
                                out SecurityToken validatedToken
                            );

            if (validatedToken is JwtSecurityToken jwtToken)
            {
                if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token algorithm");
                }
            }

            return principal;
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Token validation failed", ex);
        }
    }

}
