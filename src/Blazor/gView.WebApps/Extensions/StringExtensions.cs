namespace gView.WebApps.Extensions;

static public class StringExtensions
{
    static public string ToAppTitle(this string str)
    {
        if(str.Contains("/carto", StringComparison.OrdinalIgnoreCase))
        {
            return "gView.Carto";
        }

        if (str.Contains("/explorer", StringComparison.OrdinalIgnoreCase))
        {
            return "gView.DataExplorer";
        }

        return "gView.WebApps";
    }
}
