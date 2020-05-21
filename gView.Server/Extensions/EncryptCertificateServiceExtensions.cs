using gView.Framework.Security;
using gView.Server.AppCode;
using gView.Server.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static gView.Server.AppCode.AuthToken;

namespace gView.Server.Extensions
{
    static public class EncryptCertificateServiceExtensions
    {
        static public string ToToken(this EncryptionCertificateService ecs, AuthToken authToken)
        {
            return SecureCrypto.EncryptToken(
                ecs.GetCertificate(),
                Guid.NewGuid().ToString("N") + "|" + authToken.Username + "|" + (int)authToken.AuthType + "|" + authToken.Expire.ToString(), resultType: SecureCrypto.ResultType.Base62);
        }

        static public AuthToken FromToken(this EncryptionCertificateService ecs, string token)
        {
            string authToken = SecureCrypto.DecryptToken(ecs.GetCertificate(), token); //Crypto.Decrypt(token, Globals.MasterPassword);

            var at = authToken.Split('|');
            return new AuthToken()
            {
                Username = at[1],
                AuthType = (AuthTypes)int.Parse(at[2]),
                Expire = long.Parse(at[3])
            };
        }
    }
}
