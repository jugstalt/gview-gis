using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IGroupLayer : ILayer
    {
        List<ILayer> ChildLayers { get; }

        MapServerGrouplayerStyle MapServerStyle { get; set; }
    }
}