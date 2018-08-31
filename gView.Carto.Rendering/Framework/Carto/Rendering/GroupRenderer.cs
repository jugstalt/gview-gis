using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using System.Reflection;
using System.IO;
using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.Framework.Carto.Rendering.UI;

namespace gView.Framework.Carto.Rendering
{
    [gView.Framework.system.RegisterPlugIn("2F814C8E-A8B7-442a-BB8B-410F2022F89A")]
    public class FeatureGroupRenderer : Cloner, IGroupRenderer, IFeatureRenderer, IPropertyPage, ILegendGroup, ISimplify
    {
        private RendererGroup _renderers;
        private bool _useRefScale = true;

        public FeatureGroupRenderer()
        {
            _renderers = new RendererGroup();
        }

        #region IGroupRenderer
        public IRendererGroup Renderers
        {
            get { return _renderers; }
        }
        #endregion

        #region IFeatureRenderer Member

        public void Draw(IDisplay disp, gView.Framework.Data.IFeature feature)
        {
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer == null) continue;
                renderer.Draw(disp, feature);
            }
        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer != null)
                    renderer.FinishDrawing(disp, cancelTracker);
            }
        }

        public void PrepareQueryFilter(gView.Framework.Data.IFeatureLayer layer, gView.Framework.Data.IQueryFilter filter)
        {
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer == null) continue;

                renderer.PrepareQueryFilter(layer, filter);
            }
        }

        public bool CanRender(gView.Framework.Data.IFeatureLayer layer, IMap map)
        {
            return true;
        }

        public bool HasEffect(IFeatureLayer layer, IMap map)
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
            get { return "Renderer Group"; }
        }

        public string Category
        {
            get { return "Group"; }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _useRefScale = (bool)stream.Load("useRefScale", true);

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
            FeatureGroupRenderer grouprenderer = new FeatureGroupRenderer();
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer == null) continue;
                grouprenderer._renderers.Add(renderer.Clone(display) as IFeatureRenderer);
            }

            grouprenderer.UseReferenceScale = _useRefScale;

            return grouprenderer;
        }

        public void Release()
        {
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer == null) continue;
                renderer.Release();
            }
            _renderers.Clear();
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPage(object initObject)
        {
            if (!(initObject is IFeatureLayer)) return null;

            try
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Carto.Rendering.UI.dll");

                IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Carto.Rendering.UI.PropertyForm_FeatureGroupRenderer") as IPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IFeatureLayer)initObject);
                }
            }
            catch (Exception ex)
            {
                
            }
            return null;
        }

        public object PropertyPageObject()
        {
            return this;
        }

        #endregion

        #region ILegendGroup Member

        public int LegendItemCount
        {
            get
            {
                int count = 0;
                foreach (IFeatureRenderer renderer in _renderers)
                {
                    if (!(renderer is ILegendGroup)) continue;
                    count += ((ILegendGroup)renderer).LegendItemCount;
                }
                return count;
            }
        }

        public ILegendItem LegendItem(int index)
        {
            int count = 0;
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (!(renderer is ILegendGroup)) continue;
                if (count + ((ILegendGroup)renderer).LegendItemCount > index)
                {
                    return ((ILegendGroup)renderer).LegendItem(index - count);
                }
                count += ((ILegendGroup)renderer).LegendItemCount;
            }
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (!(renderer is ILegendGroup)) continue;

                int count = ((ILegendGroup)renderer).LegendItemCount;
                for (int i = 0; i < count; i++)
                {
                    if (((ILegendGroup)renderer).LegendItem(i) == item)
                    {
                        ((ILegendGroup)renderer).SetSymbol(item, symbol);
                        return;
                    }
                }
            }
        }

        #endregion

        private class RendererGroup : List<IFeatureRenderer>, IRendererGroup
        {

        }

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

        #region ISimplify Member

        public void Simplify()
        {
            if (_renderers == null || _renderers.Count == 0)
                return;

            foreach (IFeatureRenderer renderer in _renderers)
            {
                if (renderer is ISimplify)
                    ((ISimplify)renderer).Simplify();
            }

            #region SimpleRenderer zusammenfassen
            bool allSimpleRenderers = true;
            foreach (IRenderer renderer in _renderers)
            {
                if (!(renderer is SimpleRenderer))
                {
                    allSimpleRenderers = false;
                    break;
                }
            }

            if (allSimpleRenderers)
            {
                IFeatureRenderer renderer = _renderers[0];
                if (_renderers.Count > 1)
                {
                    ISymbolCollection symCol = PlugInManager.Create(new Guid("062AD1EA-A93C-4c3c-8690-830E65DC6D91")) as ISymbolCollection;
                    foreach (IRenderer sRenderer in _renderers)
                    {
                        if (((SimpleRenderer)renderer).Symbol != null)
                            symCol.AddSymbol(((SimpleRenderer)renderer).Symbol);
                    }
                    ((SimpleRenderer)renderer).Symbol = (ISymbol)symCol;
                    _renderers.Clear();
                    _renderers.Add(renderer);
                }
            }
            #endregion

            #region Combine Renderers
            for (int i = 0; i < _renderers.Count; i++)
            {
                for (int j = i + 1; j < _renderers.Count; j++)
                {
                    if (_renderers[i].Combine(_renderers[j]))
                    {
                        _renderers.RemoveAt(j);
                        j--;
                    }
                }
            }
            #endregion
        }

        #endregion
    }
}
