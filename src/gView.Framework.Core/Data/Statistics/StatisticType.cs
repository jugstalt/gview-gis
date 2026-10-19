namespace gView.Framework.Core.Data.Statistics
{
    public enum StatisticType
    {
        Count,
        Sum,
        Min,
        Max,
        Avg,
        /// <summary>Sample standard deviation (n-1).</summary>
        StdDev,
        /// <summary>Sample variance (n-1).</summary>
        Var
    }
}
