using gView.Framework.Common.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.Common.Extensions;

static public class JsonSerializerOptionsExtensions
{
    static public JsonSerializerOptions AddServerDefaults(this JsonSerializerOptions options)
    {
        if (options.Converters != null)
        {
            options.Converters.Add(new Array2DConverter());
            options.Converters.Add(new DBNullConverter());
        }

        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        return options;
    }
}
