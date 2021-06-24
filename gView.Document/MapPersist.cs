using gView.Framework.Carto;
using gView.Framework.Data;
using System.Linq;

namespace gView.Framework.Document
{
    public class MapPersist : Map
    {
        public void SetDataset(int datasetId, IDataset dataset)
        {
            if (datasetId >= 0 && datasetId < _datasets.Count())
            {
                _datasets[datasetId] = dataset;
            }
        }
    }
}
