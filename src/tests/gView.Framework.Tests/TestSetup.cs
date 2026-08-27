using System.Runtime.CompilerServices;
using gView.Framework.Common;

namespace gView.Framework.Tests;

/// <summary>
/// One-time test-assembly setup. Anything that draws/measures text (e.g.
/// <c>SimpleTextSymbol.AnnotationPolygon</c>) needs a registered
/// <c>GraphicsEngine.Current.Engine</c> - without one it throws a
/// <see cref="System.NullReferenceException"/>. Mirrors
/// gView.Cmd.MxlUtil.Lib.Tests.TestSetup.
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
