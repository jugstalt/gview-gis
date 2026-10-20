using gView.Framework.Common;
using gView.Framework.Core.Common;

namespace gView.Framework.Tests.Common;

public class PlugInManagerTests
{
    // Namespace gView.Framework.* => these count as plugin interfaces
    public interface IFakeRasterDataset { }
    public interface IFakeOtherInterface { }

    public class Original : IFakeRasterDataset, IPlugInDependencies
    {
        public bool HasUnsolvedDependencies() => true;
    }

    public class OtherOriginal : IFakeRasterDataset, IPlugInDependencies
    {
        public bool HasUnsolvedDependencies() => true;
    }

    public class NoDependencies : IFakeRasterDataset { }

    [PlugInFallbackFor(OriginalGuid)]
    [PlugInFallbackFor(OtherOriginalGuid)]
    public class Fallback : IFakeRasterDataset { }

    [PlugInFallbackFor(OriginalGuid)]
    public class SecondFallback : IFakeRasterDataset { }

    [PlugInFallbackFor(OriginalGuid)]
    public class FallbackWithUnsolvedDependencies : IFakeRasterDataset, IPlugInDependencies
    {
        public bool HasUnsolvedDependencies() => true;
    }

    [PlugInFallbackFor(OriginalGuid)]
    public class FallbackMissingInterface : IFakeOtherInterface { }

    private const string OriginalGuid = "11111111-0000-0000-0000-000000000001";
    private const string OtherOriginalGuid = "11111111-0000-0000-0000-000000000002";

    private static readonly Guid OriginalId = new Guid(OriginalGuid);
    private static readonly Guid OtherOriginalId = new Guid(OtherOriginalGuid);

    private static KeyValuePair<Guid, Type> Plugin(Guid guid, Type type) => new(guid, type);
    private static KeyValuePair<Guid, Type> Plugin(Type type) => new(Guid.NewGuid(), type);

    private static (HashSet<Guid> unsolved, Dictionary<Guid, Type> fallbackTypes, List<string> log) Resolve(
            IEnumerable<KeyValuePair<Guid, Type>> plugins,
            params Type[] unsolvedTypes)
    {
        var log = new List<string>();
        var (unsolved, fallbackTypes) = PlugInManager.ResolveFallbacks(plugins, type => unsolvedTypes.Contains(type), log.Add);
        return (unsolved, fallbackTypes, log);
    }

    [Fact]
    public void ResolveFallbacks_OriginalWithUnsolvedDependencies_UsesFallback()
    {
        var (unsolved, fallbackTypes, log) = Resolve(
            [Plugin(OriginalId, typeof(Original)), Plugin(typeof(Fallback))],
            typeof(Original));

        Assert.Contains(OriginalId, unsolved);
        Assert.Equal(typeof(Fallback), fallbackTypes[OriginalId]);
        Assert.Contains(log, l => l.Contains("using fallback") && l.Contains(nameof(Fallback)));
    }

    [Fact]
    public void ResolveFallbacks_AllDependenciesSolved_NoFallbacksAndInfo()
    {
        var (unsolved, fallbackTypes, log) = Resolve(
            [Plugin(OriginalId, typeof(Original)), Plugin(typeof(Fallback))]);

        Assert.Empty(unsolved);
        Assert.Empty(fallbackTypes);
        Assert.Contains("PluginManager: all plugin dependencies solved", log);
    }

    [Fact]
    public void ResolveFallbacks_PluginWithoutIPlugInDependencies_IsNeverUnsolved()
    {
        var plugin = Plugin(typeof(NoDependencies));

        var (unsolved, _, _) = Resolve([plugin], typeof(NoDependencies));

        Assert.Empty(unsolved);
    }

    [Fact]
    public void ResolveFallbacks_NoFallbackDefined_UnsolvedWithoutFallback()
    {
        var (unsolved, fallbackTypes, log) = Resolve(
            [Plugin(OriginalId, typeof(Original))],
            typeof(Original));

        Assert.Contains(OriginalId, unsolved);
        Assert.Empty(fallbackTypes);
        Assert.Contains(log, l => l.Contains("no fallback defined"));
    }

    [Fact]
    public void ResolveFallbacks_FallbackWithUnsolvedDependencies_IsIgnored()
    {
        var (unsolved, fallbackTypes, log) = Resolve(
            [Plugin(OriginalId, typeof(Original)), Plugin(typeof(FallbackWithUnsolvedDependencies))],
            typeof(Original), typeof(FallbackWithUnsolvedDependencies));

        Assert.False(fallbackTypes.ContainsKey(OriginalId));
        Assert.Contains(log, l => l.Contains("has unsolved dependencies itself"));
    }

    [Fact]
    public void ResolveFallbacks_FallbackMissingInterfaceOfOriginal_IsIgnored()
    {
        var (_, fallbackTypes, log) = Resolve(
            [Plugin(OriginalId, typeof(Original)), Plugin(typeof(FallbackMissingInterface))],
            typeof(Original));

        Assert.False(fallbackTypes.ContainsKey(OriginalId));
        Assert.Contains(log, l => l.Contains("does not implement") && l.Contains(nameof(IFakeRasterDataset)));
    }

    [Fact]
    public void ResolveFallbacks_MoreThanOneFallback_FirstWinsWithWarning()
    {
        var (_, fallbackTypes, log) = Resolve(
            [Plugin(OriginalId, typeof(Original)), Plugin(typeof(Fallback)), Plugin(typeof(SecondFallback))],
            typeof(Original));

        Assert.Equal(typeof(Fallback), fallbackTypes[OriginalId]);
        Assert.Contains(log, l => l.Contains("more than one fallback") && l.Contains(nameof(SecondFallback)));
    }

    [Fact]
    public void ResolveFallbacks_OneFallbackForSeveralOriginals_UsedForEach()
    {
        var (_, fallbackTypes, _) = Resolve(
            [Plugin(OriginalId, typeof(Original)), Plugin(OtherOriginalId, typeof(OtherOriginal)), Plugin(typeof(Fallback))],
            typeof(Original), typeof(OtherOriginal));

        Assert.Equal(typeof(Fallback), fallbackTypes[OriginalId]);
        Assert.Equal(typeof(Fallback), fallbackTypes[OtherOriginalId]);
    }
}
