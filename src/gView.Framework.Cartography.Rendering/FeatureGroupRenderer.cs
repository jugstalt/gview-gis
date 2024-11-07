using gView.Framework.Cartography.Rendering.Abstractions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.Rendering;

[RegisterPlugIn("2F814C8E-A8B7-442a-BB8B-410F2022F89A")]
public class FeatureGroupRenderer : Cloner, IGroupRenderer, IFeatureRenderer, IDefault, ILegendGroup, ISimplify
{
    private RendererGroup _rendererItems;
    private bool _useRefScale = true;

    public FeatureGroupRenderer()
    {
        _rendererItems = new RendererGroup();
    }

    #region IGroupRenderer
    public IRendererGroup RendererItems
    {
        get { return _rendererItems; }
    }
    #endregion

    #region IFeatureRenderer Member

    public void Draw(IDisplay disp, IFeature feature)
    {
        foreach (IFeatureRenderer renderer in _rendererItems
                                                    .Where(i => i?.Renderer is IFeatureRenderer)
                                                    .Select(i => i.Renderer))
        {
            renderer.Draw(disp, feature);
        }
    }

    public void StartDrawing(IDisplay display) { }

    public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
    {
        foreach (IFeatureRenderer renderer in _rendererItems
                                                    .Where(i => i?.Renderer is IFeatureRenderer)
                                                    .Select(i => i.Renderer))
        {
            if (renderer != null)
            {
                renderer.FinishDrawing(disp, cancelTracker);
            }
        }
    }

    public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
    {
        foreach (IFeatureRenderer renderer in _rendererItems
                                                    .Where(i => i?.Renderer is IFeatureRenderer)
                                                    .Select(i => i.Renderer))
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.PrepareQueryFilter(layer, filter);
        }
    }

    public bool CanRender(IFeatureLayer layer, IMap map)
    {
        return true;
    }

    public bool HasEffect(IFeatureLayer layer, IMap map)
        => _rendererItems?
                        .Where(i => i?.Renderer is IFeatureRenderer)
                        .Select(i => (IFeatureRenderer)i.Renderer)
                        .Any(r => r.HasEffect(layer, map)) == true;

    public bool UseReferenceScale
    {
        get
        {
            return _useRefScale;
        }
        set
        {
            _useRefScale = value;
            foreach (IFeatureRenderer feawtureRenderer in _rendererItems
                                                            .Where(i => i.Renderer is IFeatureRenderer)
                                                            .Select(i => (IFeatureRenderer)i.Renderer))
            {

                feawtureRenderer.UseReferenceScale = _useRefScale;
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

    public bool RequireClone()
    {
        return _rendererItems?.Any(rendererItem =>
                rendererItem?.Renderer is IFeatureRenderer featureRenderer
                && featureRenderer.RequireClone()) == true;
    }

    #endregion

    #region IPersistable Member

    public void Load(IPersistStream stream)
    {
        _useRefScale = (bool)stream.Load("useRefScale", true);

        IFeatureRenderer renderer;
        while ((renderer = stream.Load("FeatureRenderer", null) as IFeatureRenderer) != null)
        {
            _rendererItems.Add(new RendererItem(renderer));
        }
    }

    public void Save(IPersistStream stream)
    {
        stream.Save("useRefScale", _useRefScale);

        foreach (var item in _rendererItems)
        {
            if (item?.Renderer is null)
            {
                continue;
            }

            stream.Save("FeatureRenderer", item.Renderer);
        }
    }

    #endregion

    #region IClone2 Member

    public object Clone(CloneOptions options)
    {
        FeatureGroupRenderer grouprenderer = new FeatureGroupRenderer();
        foreach (var item in _rendererItems)
        {
            if (item?.Renderer == null)
            {
                continue;
            }

            grouprenderer._rendererItems.Add(
                new RendererItem(item.Renderer.Clone(options) as IFeatureRenderer));
        }

        grouprenderer.UseReferenceScale = _useRefScale;

        return grouprenderer;
    }

    public void Release()
    {
        foreach (IFeatureRenderer renderer in _rendererItems
                                                    .Where(i => i?.Renderer is IFeatureRenderer)
                                                    .Select(i => i.Renderer))
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.Release();
        }
        _rendererItems.Clear();
    }

    #endregion

    #region ICreateDefault Member

    public ValueTask DefaultIfEmpty(object initObject)
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region ILegendGroup Member

    public int LegendItemCount
    {
        get
        {
            int count = 0;
            foreach (IFeatureRenderer renderer in _rendererItems
                                                        .Where(i => i?.Renderer is IFeatureRenderer)
                                                        .Select(i => i.Renderer))
            {
                if (!(renderer is ILegendGroup))
                {
                    continue;
                }

                count += ((ILegendGroup)renderer).LegendItemCount;
            }
            return count;
        }
    }

    public ILegendItem LegendItem(int index)
    {
        int count = 0;
        foreach (IFeatureRenderer renderer in _rendererItems
                                                    .Where(i => i?.Renderer is IFeatureRenderer)
                                                    .Select(i => i.Renderer))
        {
            if (!(renderer is ILegendGroup))
            {
                continue;
            }

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
        foreach (IFeatureRenderer renderer in _rendererItems
                                                     .Where(i => i?.Renderer is IFeatureRenderer)
                                                     .Select(i => i.Renderer))
        {
            if (!(renderer is ILegendGroup))
            {
                continue;
            }

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

    private class RendererGroup : List<IRendererGroupItem>, IRendererGroup
    {
        public new void Add(IRendererGroupItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item is RendererItem rendererItem)
            {
                base.Add(rendererItem);
            }
            else if (item.Renderer is IFeatureRenderer featureRenderer)
            {
                base.Add(new RendererItem(featureRenderer));
            }
        }

        public new void Clear()
        {
            foreach (var item in this)
            {
                item.Renderer?.Release();
            }

            base.Clear();
        }

        public IRendererGroupItem Create(IRenderer renderer)
            => renderer is IFeatureRenderer featureRenderer
                ? new RendererItem(featureRenderer)
                : throw new ArgumentException("Renderer is not a feature renderer");
    }

    private class RendererItem : IRendererGroupItem
    {
        public RendererItem(IFeatureRenderer renderer)
            => (Renderer) = (renderer);

        public IRenderer Renderer { get; set; }
    }


    #region IRenderer Member

    public List<ISymbol> Symbols
    {
        get
        {
            List<ISymbol> symbols = new List<ISymbol>();

            if (_rendererItems != null)
            {
                foreach (IRenderer renderer in _rendererItems
                                                   .Where(i => i?.Renderer is IRenderer)
                                                   .Select(i => i.Renderer))
                {
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
        if (_rendererItems == null || _rendererItems.Count == 0)
        {
            return;
        }

        foreach (var item in _rendererItems)
        {
            if (item?.Renderer is ISimplify simplify)
            {
                simplify.Simplify();
            }
        }

        #region SimpleRenderer zusammenfassen

        bool allSimpleRenderers = true;
        foreach (var item in _rendererItems)
        {
            if (!(item?.Renderer is SimpleRenderer))
            {
                allSimpleRenderers = false;
                break;
            }
        }

        if (allSimpleRenderers)
        {
            IFeatureRenderer renderer = _rendererItems[0].Renderer as IFeatureRenderer;

            if (_rendererItems.Count > 1)
            {
                ISymbolCollection symCol = PlugInManager.Create(new Guid("062AD1EA-A93C-4c3c-8690-830E65DC6D91")) as ISymbolCollection;

                foreach (var item in _rendererItems)
                {
                    if (((SimpleRenderer)item.Renderer).Symbol != null)
                    {
                        symCol.AddSymbol(((SimpleRenderer)item.Renderer).Symbol);
                    }
                }

                ((SimpleRenderer)renderer).Symbol = (ISymbol)symCol;
                _rendererItems.Clear();
                _rendererItems.Add(new RendererItem(renderer));
            }
        }

        #endregion

        #region Combine Renderers

        for (int i = 0; i < _rendererItems.Count; i++)
        {
            for (int j = i + 1; j < _rendererItems.Count; j++)
            {
                if (_rendererItems[i].Renderer.Combine(_rendererItems[j].Renderer))
                {
                    _rendererItems.RemoveAt(j);
                    j--;
                }
            }
        }

        #endregion
    }

    #endregion
}
