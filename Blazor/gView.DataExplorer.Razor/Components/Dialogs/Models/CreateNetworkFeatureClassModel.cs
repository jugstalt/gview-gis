using gView.Blazor.Models.Dialogs;
using gView.Framework.Data;
using gView.Framework.Network;

namespace gView.DataExplorer.Razor.Components.Dialogs.Models;

public class CreateNetworkFeatureClassModel : IDialogResultItem
{
    public CreateNetworkFeatureClassModel() { }

    public CreateNetworkFeatureClassModel(IFeatureDataset featureDataset)
    {
        FeatureDataset = featureDataset;
    }

    public ResultClass Result { get; } = new ResultClass();

    public IFeatureDataset? FeatureDataset { get; set; }

    #region Classes

    public class ResultClass
    {
        public string Name { get; set; } = "NET_NETWORK";

        public ICollection<IFeatureClass> NodeFeatureClasses { get; } = new List<IFeatureClass>();
        public ICollection<IFeatureClass> EdgeFeatureClasses { get; } = new List<IFeatureClass>();

        public bool UseSnapTolerance { get; set; }
        public double SnapTolerance { get; set; }

        public bool UseComplexEdges { get; set; }
        public ICollection<IFeatureClass> ComplexEdges { get; } = new List<IFeatureClass>();

        public ICollection<Node> Nodes { get; } = new List<Node>();

        public GraphWeights Weights { get; } = new GraphWeights();

        public class Node
        {
            public Node(IFeatureClass fc)
            {
                this.FeatureClass = fc;
            }

            public bool IsSwitch { get; set; }
            public IFeatureClass FeatureClass { get; set; }
            public string Fieldname { get; set; } = String.Empty;
            public NetworkNodeType NodeType { get; set; } = NetworkNodeType.Unknown;
        }
    }

    #endregion
}
