using gView.Framework.Carto;
using System.Threading.Tasks;

namespace gView.Interoperability.Server.TileService.Extensions
{
    static public class MapExtensions
    {
        async static public Task<bool> TryRender(this IServiceMap map, int maxTries = 1)
        {
            for (int i = 0; i < maxTries; i++)
            {
                if (await map.Render() == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
