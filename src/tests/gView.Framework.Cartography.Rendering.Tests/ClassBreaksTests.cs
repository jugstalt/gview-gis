using gView.Framework.Cartography.Rendering.Classification;

namespace gView.Framework.Cartography.Rendering.Tests;

public class ClassBreaksTests
{
    [Fact]
    public void EqualInterval_SplitsRangeEvenly()
    {
        var limits = ClassBreaks.EqualInterval(0, 100, 4);

        Assert.Equal([0, 25, 50, 75, 100], limits);
    }

    [Fact]
    public void EqualIntervalByWidth_LastClassEndsAtMax()
    {
        var limits = ClassBreaks.EqualIntervalByWidth(0, 95, 20);

        Assert.Equal([0, 20, 40, 60, 80, 95], limits);
    }

    [Fact]
    public void EqualIntervalByWidth_ExactMultiple_NoEmptyLastClass()
    {
        var limits = ClassBreaks.EqualIntervalByWidth(0, 100, 25);

        Assert.Equal([0, 25, 50, 75, 100], limits);
    }

    [Fact]
    public void Quantile_SameNumberOfValuesPerClass()
    {
        var values = Enumerable.Range(1, 100).Select(i => (double)i).ToArray();

        var limits = ClassBreaks.Quantile(values, 4);
        var counts = ClassBreaks.CountByRanges(values, Ranges(limits));

        Assert.Equal(5, limits.Length);
        Assert.Equal(1, limits[0]);
        Assert.Equal(100, limits[^1]);
        Assert.All(counts, c => Assert.InRange(c, 24, 26));
    }

    [Fact]
    public void Quantile_ManyEqualValues_DropsDuplicateLimits()
    {
        double[] values = [1, 1, 1, 1, 1, 1, 1, 1, 2, 3];

        var limits = ClassBreaks.Quantile(values, 5);

        Assert.Equal(limits.Distinct().Count(), limits.Length);
        Assert.Equal(1, limits[0]);
        Assert.Equal(3, limits[^1]);
    }

    [Fact]
    public void NaturalBreaks_FindsTheGapsBetweenClusters()
    {
        double[] values = [1, 2, 3, 4, 50, 51, 52, 53, 100, 101, 102];

        var limits = ClassBreaks.NaturalBreaks(values, 3);
        var counts = ClassBreaks.CountByRanges(values, Ranges(limits));

        Assert.Equal([1d, 4d, 53d, 102d], limits);
        Assert.Equal([4L, 4L, 3L], counts);
    }

    [Fact]
    public void NaturalBreaks_LargeInput_IsSampledAndKeepsMinMax()
    {
        var values = Enumerable.Range(0, 20000).Select(i => (double)(i % 3 == 0 ? i : i + 100000)).Order().ToArray();

        var limits = ClassBreaks.NaturalBreaks(values, 5);

        Assert.Equal(values[0], limits[0]);
        Assert.Equal(values[^1], limits[^1]);
        Assert.True(limits.Length <= 6);
    }

    [Fact]
    public void NaturalBreaks_AllValuesEqual_OneClass()
    {
        var limits = ClassBreaks.NaturalBreaks([5, 5, 5], 3);

        Assert.Equal([5d], limits);
    }

    [Theory]
    [InlineData(0, 5000, 0)]
    [InlineData(0, 1, 2)]
    [InlineData(0, 0.05, 4)]
    [InlineData(10, 10, 2)]
    public void AutoDecimals(double min, double max, int expected)
    {
        Assert.Equal(expected, ClassBreaks.AutoDecimals(min, max));
    }

    [Fact]
    public void Round_FirstDownLastUp()
    {
        var limits = ClassBreaks.Round([0.123, 0.456, 0.987], 1);

        Assert.Equal([0.1, 0.5, 1.0], limits);
    }

    [Fact]
    public void Round_CollapsingLimitsAreRemoved()
    {
        var limits = ClassBreaks.Round([1.01, 1.02, 1.03, 5.0], 0);

        Assert.Equal([1d, 5d], limits);
    }

    [Fact]
    public void Label_UsesPointAsDecimalSeparator()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

            Assert.Equal("0.30 – 0.40", ClassBreaks.Label(0.30000000000000004, 0.4, 2));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    [Fact]
    public void CountByRanges_ValueOnLimitGoesToFirstClass()
    {
        var counts = ClassBreaks.CountByRanges([10, 20, 30], [(10, 20), (20, 30)]);

        Assert.Equal([2L, 1L], counts);
    }

    private static (double Min, double Max)[] Ranges(double[] limits)
        => limits.Zip(limits.Skip(1)).Select(p => (p.First, p.Second)).ToArray();
}
