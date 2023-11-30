using gView.Framework.Core.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.Network
{
    public interface INetworkCreator
    {
        IFeatureDataset FeatureDataset { get; set; }
        string NetworkName { get; set; }
        List<IFeatureClass> EdgeFeatureClasses { get; set; }
        List<IFeatureClass> NodeFeatureClasses { get; set; }

        double SnapTolerance { get; set; }
        List<int> ComplexEdgeFcIds { get; set; }

        Dictionary<int, string> SwitchNodeFcIdAndFieldnames { get; set; }

        Dictionary<int, NetworkNodeType> NodeTypeFcIds { get; set; }

        GraphWeights GraphWeights { get; set; }

        Task Run();
    }
}
