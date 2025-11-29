using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.UI
{
    public interface IToc : IClone, IClone3, IPersistableLoadAsync
    {
        event EventHandler TocChanged;

        void Reset();

        ITocElement NextVisibleElement { get; }

        List<ITocElement> GetChildElements(ITocElement group, bool recursive = true);

        /*
		void AddGroup(string GroupName,ITOCElement parent);
         * */
        void Add2Group(ITocElement element, ITocElement Group);

        ITocElement GetTOCElement(string name, ITocElement parent);
        ITocElement GetTOCElement(ILayer element);
        ITocElement GetTocElementByLayerId(int layerId);
        //ITOCElement GetTOCElement(IClass element);

        int GetTOCElementOrder(ITocElement element);

        List<ITocElement> GroupElements { get; }

        void RenameElement(ITocElement element, string newName);
        void MoveElement(ITocElement element, ITocElement refElement, bool insertAfter);

        void SplitMultiLayer(ITocElement element);
        int CountGroupLayers(ITocElement Group, bool subGroups);
        int CountVisibleGroupLayers(ITocElement Group, bool subGroups);

        List<ILayer> Layers { get; }
        List<ILayer> VisibleLayers { get; }
        List<ITocElement> Elements { get; }
        List<IWebServiceLayer> VisibleWebServiceLayers { get; }

        Task<GraphicsEngine.Abstraction.IBitmap> Legend();
        Task<GraphicsEngine.Abstraction.IBitmap> Legend(ITocElement element);
        Task<GraphicsEngine.Abstraction.IBitmap> Legend(List<ITocElement> elements);
        Task<TocLegendItems> LegendSymbol(ITocElement element, int width = 20, int height = 20, HashSet<string> symbolKeys = null);
    }
}