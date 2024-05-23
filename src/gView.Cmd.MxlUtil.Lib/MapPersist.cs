using gView.Framework.Cartography;
using gView.Framework.Core.Data;

namespace gView.Cmd.MxlUtil.Lib
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
