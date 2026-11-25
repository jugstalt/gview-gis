namespace gView.Pipe.Plugins.Gis.Extensions;

internal static class TimeSpanExtensions
{
    /// <summary>A short duration for the run log, e.g. <c>1 h 5 min</c>, <c>3 min 12 s</c> or <c>4.2 s</c>.</summary>
    public static string ToDurationText(this TimeSpan time)
        => time.TotalHours >= 1 ? $"{(int)time.TotalHours} h {time.Minutes} min"
         : time.TotalMinutes >= 1 ? $"{(int)time.TotalMinutes} min {time.Seconds} s"
         : $"{time.TotalSeconds:0.#} s";
}
