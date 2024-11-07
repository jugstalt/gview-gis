using gView.Framework.IO;
using System.Text;
using gView.Framework.Common.Extensions;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models.Extensions;

static public class VectorTileCacheConnectionModelExtensions
{
    static public string ToConnectionString(this VectorTileCacheConnectionModel connectionModel)
    {
        StringBuilder sb = new StringBuilder();

        sb.AddConnectionStringParameter("name", connectionModel.Name);
        sb.AddConnectionStringParameter("source", connectionModel.Source);

        return sb.ToString();
    }

    static public VectorTileCacheConnectionModel ToVectorTileCacheConnectionModel(this string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return new VectorTileCacheConnectionModel();
        }

        return new VectorTileCacheConnectionModel()
        {
            Name = ConfigTextStream.ExtractValue(connectionString, "name"),
            Source = ConfigTextStream.ExtractValue(connectionString, "source")
        };
    }
}
