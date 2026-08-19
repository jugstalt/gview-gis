using System.Runtime.CompilerServices;
using gView.Framework.Common;

namespace gView.Cmd.MxlUtil.Lib.Tests;

/// <summary>
/// One-time test-assembly setup. <c>AprxMapConverter.Convert</c> constructs a gView
/// <c>Map</c>/<c>Display</c>, whose constructor reads <c>GraphicsEngine.Current.Engine.ScreenDpi</c>
/// - without a registered graphics engine that throws a <see cref="System.NullReferenceException"/>.
/// The real CLI (<c>ConvertAprx.cs</c>) registers one on startup; tests need the same.
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
