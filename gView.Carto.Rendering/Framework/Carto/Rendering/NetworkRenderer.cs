using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.Carto.Rendering;
using gView.Framework.IO;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.UI;
using gView.Framework.Data;
using System.Reflection;
using gView.Framework.Network;
using gView.Framework.Carto.Rendering.UI;

namespace gView.Framework.Carto.Rendering
{
    [gView.Framework.system.RegisterPlugIn("9E51A28A-5735-4b84-9DC1-D6EB77D9FD26")]
    public class NetworkRenderer : Cloner, IFeatureRenderer, IPropertyPage, ILegendGroup
    {
        private RendererGroup _renderers;
        private bool _useRefScale = true;

        public enum RendererType
        {
            Edges = 0,
            SimpleNodes = 1,
            SwitchesOn = 2,
            SwitchesOff = 3
        }

        public NetworkRenderer()
            : this(true)
        {
        }
        private NetworkRenderer(bool init)
        {
            _renderers = new RendererGroup();

            if (init == true)
            {
                _renderers.Add(PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer);  // Edges
                _renderers.Add(PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer);  // Simple Nodes
                _renderers.Add(PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer);  // Switches On
                _renderers.Add(PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer);  // Switches Off

                foreach (IFeatureRenderer renderer in _renderers)
                {
                    geometryType geomType = geometryType.Polyline;
                    if (_renderers.IndexOf(renderer) > 0)
                        geomType = geometryType.Point;

                    if (renderer is ISymbolCreator && renderer is IFeatureRenderer2)
                    {
                        ((IFeatureRenderer2)renderer).Symbol = ((ISymbolCreator)renderer).CreateStandardSymbol(geomType);
                    }
                }
            }
        }

        public ISymbol this[RendererType rendererType]
        {
            get
            {
                try
                {
                    IFeatureRenderer2 r = _renderers[(int)rendererType] as IFeatureRenderer2;
                    if (r != null)
                        return r.Symbol;
                }
                catch
                {
                }
                return null;
            }
            set
            {
                try
                {
                    IFeatureRenderer2 r = _renderers[(int)rendererType] as IFeatureRenderer2;
                    if (r != null && r.Symbol != value)
                        r.Symbol = value;
                }
                catch
                {
                }
            }
        }

        #region IFeatureRenderer Member

        public void Draw(IDisplay disp, gView.Framework.Data.IFeature feature)
        {
            if (feature != null)
            {
                if (feature.Shape is IPolyline)
                {
                    if (_renderers[0] != null)
                        _renderers[0].Draw(disp, feature);
                }
                else if (feature.Shape is IPoint)
                {
                    object isSwitch = feature["SWITCH"];
                    object switchState = feature["STATE"];

                    if (isSwitch is bool &&
                        switchState is bool &&
                        (bool)isSwitch == true)
                    {
                        if ((bool)switchState == true)
                        {
                            if (_renderers[2] != null)
                                _renderers[2].Draw(disp, feature);
                        }
                        else
                        {
                            if (_renderers[3] != null)
                                _renderers[3].Draw(disp, feature);
                        }
                    }
                    else
                    {
                        if (_renderers[1] != null)
                            _renderers[1].Draw(disp, feature);
                    }
                }
            }
        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
        }

        public void PrepareQueryFilter(gView.Framework.Data.IFeatureLayer layer, gView.Framework.Data.IQueryFilter filter)
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                IFeatureRenderer renderer = _renderers[i];
                if (renderer == null) continue;

                filter.AddField("*");
                renderer.PrepareQueryFilter(layer, filter);
            }
        }

        public bool CanRender(gView.Framework.Data.IFeatureLayer layer, IMap map)
        {
            if (layer != null &&
                layer.FeatureClass is INetworkFeatureClass)
                return true;

            return false;
        }

        public bool HasEffect(gView.Framework.Data.IFeatureLayer layer, IMap map)
        {
            if (_renderers == null) return false;
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer == null) continue;
                if (renderer.HasEffect(layer, map)) return true;
            }
            return false;
        }

        public bool UseReferenceScale
        {
            get
            {
                return _useRefScale;
            }
            set
            {
                _useRefScale = value;
                foreach (IFeatureRenderer renderer in _renderers)
                {
                    if (renderer == null) continue;

                    renderer.UseReferenceScale = _useRefScale;
                }
            }
        }

        public string Name
        {
            get { return "Network Renderer"; }
        }

        public string Category
        {
            get { return "Network"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _useRefScale = (bool)stream.Load("useRefScale", true);

            _renderers.Clear();
            IFeatureRenderer renderer;
            while ((renderer = stream.Load("FeatureRenderer", null) as IFeatureRenderer) != null)
            {
                _renderers.Add(renderer);
            }
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("useRefScale", _useRefScale);

            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer == null) continue;
                stream.Save("FeatureRenderer", renderer);
            }
        }

        #endregion

        #region IClone2 Member

        public object Clone(IDisplay display)
        {
            NetworkRenderer networkrenderer = new NetworkRenderer(false);
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer == null) continue;
                networkrenderer._renderers.Add(renderer.Clone(display) as IFeatureRenderer);
            }

            networkrenderer.UseReferenceScale = _useRefScale;

            return networkrenderer;
        }

        public void Release()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPageObject()
        {
            return this;
        }

        public object PropertyPage(object initObject)
        {
            if (initObject is IFeatureLayer)
            {
                if (((IFeatureLayer)initObject).FeatureClass == null) return null;

                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Carto.Rendering.UI.dll");

                IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Carto.Rendering.UI.NetworkRendererControl") as IPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IFeatureLayer)initObject);
                }
            }

            return null;
        }

        #endregion

        private class RendererGroup : List<IFeatureRenderer>
        {

        }

        #region ILegendGroup Member

        public int LegendItemCount
        {
            get { return _renderers.Count; }
        }

        public ILegendItem LegendItem(int index)
        {
            try
            {
                ILegendItem item = ((ILegendGroup)_renderers[index]).LegendItem(0);
                RendererType type = (RendererType)index;
                item.LegendLabel = type.ToString();
                return item;
            }
            catch
            {
                return null;
            }
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer is ILegendGroup)
                {
                    if (((ILegendGroup)renderer).LegendItem(0) == item)
                    {
                        ((ILegendGroup)renderer).SetSymbol(item, symbol);
                        break;
                    }
                }
            }
        }

        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                List<ISymbol> symbols = new List<ISymbol>();

                if (_renderers != null)
                {
                    foreach (IRenderer renderer in _renderers)
                    {
                        if (renderer == null) continue;
                        symbols.AddRange(renderer.Symbols);
                    }
                }

                return symbols;
            }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }
        #endregion
    }

}
