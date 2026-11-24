#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace gView.Framework.Cartography.Rendering.Classification;

public enum ClassificationMethod
{
    /// <summary>Equal width (fixed number of classes or fixed step width).</summary>
    EqualInterval,
    /// <summary>The same number of features per class.</summary>
    Quantile,
    /// <summary>Jenks natural breaks: class limits at the natural gaps of the values.</summary>
    NaturalBreaks
}

/// <summary>
/// Class limits for the quantity renderer. All methods return the limits as an ascending array
/// <c>[min, b1, ..., max]</c>; class <c>i</c> is <c>[limits[i], limits[i + 1]]</c> (the quantity renderer
/// assigns a value on a limit to the first class).
/// </summary>
public static class ClassBreaks
{
    /// <summary>Above this number of values natural breaks are computed on an even sample of the sorted values.</summary>
    public const int NaturalBreaksMaxValues = 5000;

    public static double[] EqualInterval(double min, double max, int classCount)
    {
        classCount = Math.Max(classCount, 1);
        if (max <= min)
        {
            return [min, max];
        }

        double width = (max - min) / classCount;
        return Enumerable.Range(0, classCount + 1)
                         .Select(i => i == classCount ? max : min + i * width)
                         .ToArray();
    }

    public static double[] EqualIntervalByWidth(double min, double max, double width)
    {
        if (max <= min || width <= 0)
        {
            return [min, max];
        }

        var limits = new List<double>() { min };
        for (double x = min + width; x < max - width * 1e-9; x += width)
        {
            limits.Add(x);
        }
        limits.Add(max);

        return limits.ToArray();
    }

    /// <param name="sortedValues">All values, sorted ascending</param>
    public static double[] Quantile(IReadOnlyList<double> sortedValues, int classCount)
    {
        if (sortedValues.Count == 0)
        {
            return [];
        }

        classCount = Math.Max(classCount, 1);
        int n = sortedValues.Count;

        var limits = new List<double>() { sortedValues[0] };
        for (int i = 1; i < classCount; i++)
        {
            int index = Math.Min((int)Math.Floor((double)i * n / classCount), n - 1);
            limits.Add(sortedValues[index]);
        }
        limits.Add(sortedValues[n - 1]);

        return Distinct(limits);
    }

    /// <summary>Jenks natural breaks (Fisher's exact optimisation on the (sampled) sorted values).</summary>
    /// <param name="sortedValues">All values, sorted ascending</param>
    public static double[] NaturalBreaks(IReadOnlyList<double> sortedValues, int classCount)
    {
        if (sortedValues.Count == 0)
        {
            return [];
        }

        var data = Sample(sortedValues, NaturalBreaksMaxValues);
        int n = data.Length;
        int k = Math.Min(Math.Max(classCount, 1), n);

        if (k <= 1 || data[0] == data[n - 1])
        {
            return Distinct([data[0], data[n - 1]]);
        }

        // mat1: lower class limit (1-based index), mat2: variance
        var mat1 = new int[n + 1, k + 1];
        var mat2 = new double[n + 1, k + 1];

        for (int j = 1; j <= k; j++)
        {
            mat1[1, j] = 1;
            mat2[1, j] = 0;
            for (int i = 2; i <= n; i++)
            {
                mat2[i, j] = double.PositiveInfinity;
            }
        }

        for (int l = 2; l <= n; l++)
        {
            double s1 = 0, s2 = 0, w = 0, variance = 0;

            for (int m = 1; m <= l; m++)
            {
                int i3 = l - m + 1;
                double value = data[i3 - 1];

                s2 += value * value;
                s1 += value;
                w++;
                variance = s2 - (s1 * s1) / w;

                int i4 = i3 - 1;
                if (i4 != 0)
                {
                    for (int j = 2; j <= k; j++)
                    {
                        if (mat2[l, j] >= variance + mat2[i4, j - 1])
                        {
                            mat1[l, j] = i3;
                            mat2[l, j] = variance + mat2[i4, j - 1];
                        }
                    }
                }
            }

            mat1[l, 1] = 1;
            mat2[l, 1] = variance;
        }

        var limits = new double[k + 1];
        limits[0] = data[0];
        limits[k] = data[n - 1];

        int kk = n;
        for (int j = k; j >= 2; j--)
        {
            int id = mat1[kk, j] - 2;
            limits[j - 1] = data[Math.Max(id, 0)];
            kk = mat1[kk, j] - 1;
        }

        return Distinct(limits);
    }

    /// <summary>
    /// Number of decimals for limits and labels: about three significant digits of the value range
    /// (0 for 0..5000, 2 for 0..1, 4 for 0..0.05).
    /// </summary>
    public static int AutoDecimals(double min, double max)
    {
        double range = Math.Abs(max - min);
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
        {
            return 2;
        }

        int decimals = 2 - (int)Math.Floor(Math.Log10(range));
        return Math.Clamp(decimals, 0, 6);
    }

    /// <summary>
    /// Rounds the limits to <paramref name="decimals"/>: the first limit down, the last up (so all values
    /// stay inside), the others to the nearest. Limits that collapse are removed.
    /// </summary>
    public static double[] Round(IReadOnlyList<double> limits, int decimals)
    {
        if (limits.Count == 0)
        {
            return [];
        }

        double factor = Math.Pow(10, Math.Clamp(decimals, 0, 10));
        var rounded = limits.Select((limit, i) =>
            i == 0 ? Math.Floor(limit * factor) / factor
            : i == limits.Count - 1 ? Math.Ceiling(limit * factor) / factor
            : Math.Round(limit * factor, MidpointRounding.AwayFromZero) / factor);

        return Distinct(rounded);
    }

    /// <summary>Label of a class, e.g. <c>10 – 20</c> (always with '.' as decimal separator).</summary>
    public static string Label(double min, double max, int decimals)
    {
        var format = "F" + Math.Clamp(decimals, 0, 10);
        return $"{min.ToString(format, CultureInfo.InvariantCulture)} – {max.ToString(format, CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Values per class, like the quantity renderer assigns them: the first class with min &lt;= value &lt;= max.
    /// </summary>
    public static long[] CountByRanges(IEnumerable<double> values, IReadOnlyList<(double Min, double Max)> ranges)
    {
        var counts = new long[ranges.Count];

        foreach (var value in values)
        {
            for (int i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].Min <= value && ranges[i].Max >= value)
                {
                    counts[i]++;
                    break;
                }
            }
        }

        return counts;
    }

    #region Helper

    private static double[] Distinct(IEnumerable<double> limits)
    {
        var result = new List<double>();
        foreach (var limit in limits)
        {
            if (result.Count == 0 || limit > result[^1])
            {
                result.Add(limit);
            }
        }

        return result.ToArray();
    }

    private static double[] Sample(IReadOnlyList<double> sortedValues, int maxCount)
    {
        int n = sortedValues.Count;
        if (n <= maxCount)
        {
            return sortedValues.ToArray();
        }

        var sample = new double[maxCount];
        for (int i = 0; i < maxCount; i++)
        {
            long index = (long)i * (n - 1) / (maxCount - 1);   // first and last value are always included
            sample[i] = sortedValues[(int)index];
        }

        return sample;
    }

    #endregion
}
