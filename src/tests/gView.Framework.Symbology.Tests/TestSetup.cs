using System.Runtime.CompilerServices;
using gView.Framework.Common;

namespace gView.Framework.Symbology.Tests;

/// <summary>
/// One-time test-assembly setup. Symbols create pens and brushes in their constructors and need a
/// registered <c>GraphicsEngine.Current.Engine</c>. Mirrors gView.Framework.Tests.TestSetup.
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
