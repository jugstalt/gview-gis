using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using System.Collections.Generic;

namespace gView.Framework.Data
{
    public class GroupLayer : Layer, IGroupLayer
    {
        private List<ILayer> _childLayers = new List<ILayer>();

        public GroupLayer() { }
        public GroupLayer(string name)
        {
            this.Title = name;
        }

        internal override void CopyFrom(IDatasetElement element)
        {
            base.CopyFrom(element);
        }

        #region IGroupLayer Member

        public List<ILayer> ChildLayer
        {
            get { return ListOperations<ILayer>.Clone(_childLayers); }
        }

        public MapServerGrouplayerStyle MapServerStyle { get; set; }

        #endregion

        public void Add(Layer layer)
        {
            if (layer == null || _childLayers.Contains(layer))
            {
                return;
            }

            _childLayers.Add(layer);
            layer.GroupLayer = this;
        }
        public void Remove(Layer layer)
        {
            if (layer == null || !_childLayers.Contains(layer))
            {
                return;
            }

            _childLayers.Remove(layer);
            layer.GroupLayer = null;
        }

        #region IPersitable

        public override void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.MapServerStyle = (MapServerGrouplayerStyle)(int)stream.Load("MapServerStyle", (int)MapServerGrouplayerStyle.Dropdownable);
        }

        public override void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("MapServerStyle", (int)this.MapServerStyle);
        }

        #endregion
    }
}
