using gView.Framework.Core.IO;
using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Blazor.Core.Extensions;

static public class PersistableExtensions
{
    static public T? PersistedClone<T>(this T? obj)
        where T : IPersistable, new()
    {
        try
        {
            if (obj is null)
            {
                return default;
            }

            T? clone = default;

            if (PlugInManager.IsPlugin(obj))
            {
                PlugInManager compMan = new PlugInManager();
                clone = (T?)compMan.CreateInstance(PlugInManager.PlugInID(obj));
            }
            else
            {
                var assembly = System.Reflection.Assembly.GetAssembly(obj.GetType());
                clone = (T?)assembly?.CreateInstance(obj.GetType().ToString());
            }
            if (clone == null)
            {
                return default;
            }

            XmlStream stream = new XmlStream("root");

            ((IPersistable)obj).Save(stream);
            ((IPersistable)clone).Load(stream);

            return clone;
        }
        catch
        {
        }
        return default;
    }
}
