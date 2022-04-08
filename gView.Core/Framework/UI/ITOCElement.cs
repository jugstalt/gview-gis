using System.Collections.Generic;
using gView.Framework.Data;
using gView.Framework.IO;

namespace gView.Framework.UI
{
    public interface ITOCElement : IPersistableLoadAsync
    {
        string Name { get; set; }
        TOCElementType ElementType { get; }
        List<ILayer> Layers { get; }
        void AddLayer(ILayer layer);
        void RemoveLayer(ILayer layer);
        ITOCElement ParentGroup { get; }
        bool LayerVisible { get; set; }
        bool LayerLocked { get; set; }
        bool LegendVisible { get; set; }
        void OpenCloseGroup(bool open);

        ITOC TOC { get; }
    }

    
}