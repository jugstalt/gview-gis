using gView.Framework.Core.UI;

namespace gView.Carto.Core.Abstraction;

public interface ITocTreeNode
{
    ITocElement? Value { get; }
    string? Text { get; }
}

public interface ITocLayerNode : ITocTreeNode { }
public interface ITocLegendNode : ITocTreeNode { }
public interface ITocParentNode: ITocTreeNode { }
