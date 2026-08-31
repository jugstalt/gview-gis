using System.Runtime.CompilerServices;
using gView.Framework.Common;

namespace gView.Framework.Cartography.Rendering.Tests;

/// <summary>
/// One-time test-assembly setup. Anything that constructs a text symbol (e.g.
/// <c>SimpleTextSymbol</c>, used to drive <c>SimpleLabelRenderer.Draw</c>) needs a registered
/// <c>GraphicsEngine.Current.Engine</c> - without one it throws a
/// <see cref="System.NullReferenceException"/>. Mirrors gView.Framework.Tests.TestSetup.
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
