using System;
using System.Collections.Generic;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.IO;
using System.Threading.Tasks;

namespace gView.Framework.UI
{
    public interface ITOC : IClone, IClone3, IPersistableLoadAsync
    {
        event EventHandler TocChanged;

        void Reset();

        ITOCElement NextVisibleElement { get; }

        List<ITOCElement> GroupedElements(ITOCElement group);

        /*
		void AddGroup(string GroupName,ITOCElement parent);
         * */
        void Add2Group(ITOCElement element, ITOCElement Group);

        ITOCElement GetTOCElement(string name, ITOCElement parent);
        ITOCElement GetTOCElement(ILayer element);
        ITOCElement GetTOCElementByLayerId(int layerId);
        //ITOCElement GetTOCElement(IClass element);

        List<ITOCElement> GroupElements { get; }

        void RenameElement(ITOCElement element, string newName);
        void MoveElement(ITOCElement element, ITOCElement refElement, bool insertAfter);

        void SplitMultiLayer(ITOCElement element);
        int CountGroupLayers(ITOCElement Group, bool subGroups);
        int CountVisibleGroupLayers(ITOCElement Group, bool subGroups);

        List<ILayer> Layers { get; }
        List<ILayer> VisibleLayers { get; }
        List<ITOCElement> Elements { get; }
        List<IWebServiceLayer> VisibleWebServiceLayers { get; }

        Task<GraphicsEngine.Abstraction.IBitmap> Legend();
        Task<GraphicsEngine.Abstraction.IBitmap> Legend(ITOCElement element);
        Task<GraphicsEngine.Abstraction.IBitmap> Legend(List<ITOCElement> elements);
        Task<TocLegendItems> LegendSymbol(ITOCElement element);
    }

    
}