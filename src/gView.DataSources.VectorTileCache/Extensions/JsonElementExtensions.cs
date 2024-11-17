using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.DataSources.VectorTileCache.Extensions;

static internal class JsonElementExtensions
{
    // "fieldname"
    // [ "get", "fieldname" ]
    static public string GetFieldName(this JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.String)
            return jsonElement.GetString();

        if(jsonElement.ValueKind == JsonValueKind.Array)
        {
            string[] array = jsonElement.EnumerateArray().Select(e => e.GetString()).ToArray();

            return array switch
            {
                { Length: 1 } => array[0],
                { Length: 2 } when (array[0] == "get") => array[1],
                _ => throw new NotSupportedException($"Unsupported fieldname: {jsonElement.ToString()}")
            };
        }

        throw new NotSupportedException($"Unsupported fieldname: {jsonElement.ToString()}");
    }
}
