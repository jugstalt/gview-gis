using System.Runtime.CompilerServices;
using gView.Framework.Common;

namespace gView.Interoperability.GeoServices.Tests;

/// <summary>
/// Hatch fills, text symbols etc. need a registered <c>GraphicsEngine.Current.Engine</c>.
/// </summary>
internal static class TestSetup
{
    [ModuleInitializer]
    internal static void RegisterGraphicsEngine()
    {
        if (gView.GraphicsEngine.Current.Engine == null)
        {
            SystemInfo.RegisterDefaultGraphicEngines();
        }
    }
}
