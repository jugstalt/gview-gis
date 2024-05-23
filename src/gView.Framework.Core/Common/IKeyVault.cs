using System.Threading.Tasks;

namespace gView.Framework.Core.Common
{
    public interface IKeyVault
    {
        Task<string> SecretAsync(string uri);

        string Secret(string uri);
    }
}
