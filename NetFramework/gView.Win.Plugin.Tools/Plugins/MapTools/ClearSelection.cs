using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Globalisation;
using gView.Framework.system;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using System.Threading.Tasks;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("16C05C00-7F21-4216-95A6-0B4B020D3B7D")]
    public class ClearSelection : gView.Framework.UI.ITool
    {
        IMapDocument _doc = null;

        #region ITool Member

        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[9];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ClearSelection", "Clear Selection");
            }
        }

        public bool Enabled
        {
            get
            {
                if (_doc == null)
                {
                    return false;
                }

                if (_doc.FocusMap == null)
                {
                    return false;
                }

                foreach (IDatasetElement layer in _doc.FocusMap.MapElements)
                {
                    if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null && ((IWebServiceLayer)layer).WebServiceClass.Themes != null)
                    {
                        foreach (IWebServiceTheme theme in ((IWebServiceLayer)layer).WebServiceClass.Themes)
                        {
                            if (!(theme is IFeatureSelection))
                            {
                                continue;
                            }

                            ISelectionSet themeSelSet = ((IFeatureSelection)theme).SelectionSet;
                            if (themeSelSet == null)
                            {
                                continue;
                            }

                            if (themeSelSet.Count > 0)
                            {
                                return true;
                            }
                        }
                    }

                    if (!(layer is IFeatureSelection))
                    {
                        continue;
                    }

                    ISelectionSet selSet = ((IFeatureSelection)layer).SelectionSet;
                    if (selSet == null)
                    {
                        continue;
                    }

                    if (selSet.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent))
            {
                return Task.FromResult(true);
            }

            IMap map = ((MapEvent)MapEvent).Map;
            if (map == null)
            {
                return Task.FromResult(true);
            }

            map.ClearSelection();

            ((MapEvent)MapEvent).refreshMap = true;
            ((MapEvent)MapEvent).drawPhase = DrawPhase.Selection | DrawPhase.Graphics;

            return Task.FromResult(true);
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion
    }
}
