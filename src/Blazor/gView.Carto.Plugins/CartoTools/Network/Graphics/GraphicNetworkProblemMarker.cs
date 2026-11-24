using gView.Framework.Core.Geometry;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

/// <summary>The pin at the build problem clicked last in the list (its own type, so it can be removed alone).</summary>
public class GraphicNetworkProblemMarker(IPoint point, string text) : GraphicHotspotPoint(point, text)
{
}
