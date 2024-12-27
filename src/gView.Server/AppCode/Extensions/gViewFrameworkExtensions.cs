using gView.Framework.Core.Data;
using gView.Framework.Core.UI;
using System.Linq;

namespace gView.Server.AppCode.Extensions
{
    static public class gViewFrameworkExtensions
    {
        static public bool IsHidden(this ITocElement tocElement)
        {
            var parent = tocElement.ParentGroup;

            while (parent != null)
            {
                IGroupLayer groupLayer = parent.Layers.FirstOrDefault() as IGroupLayer;
                if (groupLayer != null && groupLayer.MapServerStyle == MapServerGrouplayerStyle.Checkbox)
                {
                    return true;
                }
                parent = parent.ParentGroup;
            }

            return false;
        }
    }
}
