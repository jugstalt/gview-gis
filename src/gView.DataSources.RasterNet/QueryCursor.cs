using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.RasterNet
{
    /// <summary>
    /// The result of an identify on one cell: a single row with the band values (fields as in the GDAL raster class:
    /// a "Bands" summary, the image position and one field per value).
    /// </summary>
    internal sealed class QueryCursor : IRowCursor
    {
        private readonly List<(string Name, object Value)> _fields;
        private bool _done;

        public QueryCursor(List<(string Name, object Value)> fields)
        {
            _fields = fields;
        }

        public Task<IRow> NextRow()
        {
            if (_done)
            {
                return Task.FromResult<IRow>(null);
            }
            _done = true;

            var row = new Row { OID = 1 };
            var values = _fields.Where(f => f.Name != "ImageX" && f.Name != "ImageY").Select(f => f.Value?.ToString());
            row.Fields.Add(new FieldValue("Bands", "(" + string.Join(",", values) + ")"));
            foreach (var (name, value) in _fields)
            {
                row.Fields.Add(new FieldValue(name, value));
            }
            return Task.FromResult<IRow>(row);
        }

        public void Dispose()
        {
        }
    }
}
