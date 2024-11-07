using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IGroupLayer : ILayer
    {
        List<ILayer> ChildLayer { get; }

        MapServerGrouplayerStyle MapServerStyle { get; set; }
    }
}