using gView.Framework.Core.Data;
using gView.Framework.Core.UI;

namespace gView.Interoperability.OgcApi.Extensions;

internal static class TocElementExtensions
{
    /// <summary>
    /// Layers inside a group layer with checkbox style are not exposed as individual layers
    /// by the map services (see gView.Server IsHidden)
    /// </summary>
    public static bool IsHiddenForServices(this ITocElement tocElement)
    {
        var parent = tocElement.ParentGroup;

        while (parent is not null)
        {
            if (parent.Layers?.FirstOrDefault() is IGroupLayer groupLayer
                && groupLayer.MapServerStyle == MapServerGrouplayerStyle.Checkbox)
            {
                return true;
            }
            parent = parent.ParentGroup;
        }

        return false;
    }
}
