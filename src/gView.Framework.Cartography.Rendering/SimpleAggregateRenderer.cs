using gView.Framework.Cartography.Rendering.UI;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.system;
using gView.Framework.Core.UI;
using gView.Framework.Geometry;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace gView.Framework.Cartography.Rendering
{
    [RegisterPlugIn("7B82A53D-63DA-43CA-BE94-23B4B1F1D9DA")]
    public class SimpleAggregateRenderer : Cloner, IFeatureRenderer2, IPropertyPage, ILegendGroup, ISymbolCreator
    {
        private ISymbol _symbol;
        private bool _useRefScale = true;
        private IAggregateGeometry _aggregateGeometry = null;

        public SimpleAggregateRenderer()
        {
        }

        public void Dispose()
        {
            if (_symbol != null)
            {
                _symbol.Release();
            }

            _symbol = null;
        }

        public ISymbol Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
            }
        }

        public ISymbol CreateStandardSymbol(GeometryType type)
        {
            return RendererFunctions.CreateStandardSymbol(type);
        }

        public ISymbol CreateStandardSelectionSymbol(GeometryType type)
        {
            return RendererFunctions.CreateStandardSelectionSymbol(type);
        }

        public ISymbol CreateStandardHighlightSymbol(GeometryType type)
        {
            return RendererFunctions.CreateStandardHighlightSymbol(type);
        }

        #region IFeatureRenderer

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            if (layer == null)
            {
                return false;
            }

            if (layer.FeatureClass == null)
            {
                return false;
            }

            if (layer.LayerGeometryType == GeometryType.Unknown ||
                layer.LayerGeometryType == GeometryType.Network)
            {
                return false;
            }

            return true;
        }

        public bool HasEffect(IFeatureLayer layer, IMap map)
        {
            return true;
        }

        public bool UseReferenceScale
        {
            get { return _useRefScale; }
            set { _useRefScale = value; }
        }

        public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        {

        }

        public void Draw(IDisplay disp, IFeature feature)
        {
            if (feature?.Shape != null)
            {
                if (_aggregateGeometry == null)
                {
                    _aggregateGeometry = new AggregateGeometry();
                }

                _aggregateGeometry.AddGeometry(feature.Shape);
            }
        }

        public void StartDrawing(IDisplay display)
        {

        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
            if (cancelTracker == null)
            {
                cancelTracker = new CancelTracker();
            }

            try
            {
                if (cancelTracker.Continue)
                {
                    // also draw empty aggregates (a MUST for PoygonMask!!)
                    if (_aggregateGeometry == null)
                    {
                        _aggregateGeometry = new AggregateGeometry();
                    }

                    if (_symbol is ISymbolCollection)
                    {
                        ISymbolCollection sColl = (ISymbolCollection)_symbol;
                        foreach (ISymbolCollectionItem symbolItem in sColl.Symbols)
                        {
                            if (symbolItem.Visible == false || symbolItem.Symbol == null)
                            {
                                continue;
                            }

                            ISymbol symbol = symbolItem.Symbol;

                            if (symbol.SupportsGeometryType(GeometryType.Aggregate))
                            {
                                disp.Draw(symbol, _aggregateGeometry);
                            }
                            else
                            {
                                for (int g = 0; g < _aggregateGeometry.GeometryCount; g++)
                                {
                                    disp.Draw(symbol, _aggregateGeometry[g]);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_symbol.SupportsGeometryType(GeometryType.Aggregate))
                        {
                            disp.Draw(_symbol, _aggregateGeometry);
                        }
                        else
                        {
                            for (int g = 0; g < _aggregateGeometry.GeometryCount; g++)
                            {
                                disp.Draw(_symbol, _aggregateGeometry[g]);
                            }
                        }
                    }
                }
            }
            finally
            {
                _aggregateGeometry = null;
            }
        }

        public string Name
        {
            get { return "Single Symbol (Aggregated)"; }
        }
        public string Category
        {
            get { return "Aggregated"; }
        }

        public bool RequireClone()
        {
            return _symbol != null && _symbol.RequireClone();
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
                if (((IFeatureLayer)initObject).FeatureClass == null)
                {
                    return null;
                }

                if (_symbol == null)
                {
                    _symbol = RendererFunctions.CreateStandardSymbol(((IFeatureLayer)initObject).LayerGeometryType/*((IFeatureLayer)initObject).FeatureClass.GeometryType*/);
                }
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Carto.Rendering.UI.dll");

                IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Carto.Rendering.UI.PropertyForm_SimpleRenderer") as IPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IFeatureLayer)initObject);
                }
            }

            return null;
        }

        #endregion

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        public void Load(IPersistStream stream)
        {
            _symbol = (ISymbol)stream.Load("Symbol");
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Symbol", _symbol);
        }

        #endregion

        #region ILegendGroup Members

        public int LegendItemCount
        {
            get { return _symbol == null ? 0 : 1; }
        }

        public ILegendItem LegendItem(int index)
        {
            if (index == 0)
            {
                return _symbol;
            }

            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (_symbol == null)
            {
                return;
            }

            if (item == symbol)
            {
                return;
            }

            if (item == _symbol)
            {
                if (symbol is ILegendItem && _symbol is ILegendItem)
                {
                    symbol.LegendLabel = _symbol.LegendLabel;
                }
                _symbol.Release();
                _symbol = symbol;
            }
        }
        #endregion

        #region IClone2

        public object Clone(CloneOptions options)
        {
            SimpleAggregateRenderer renderer = new SimpleAggregateRenderer();
            if (_symbol != null)
            {
                renderer._symbol = (ISymbol)_symbol.Clone(_useRefScale ? options : null);
            }

            return renderer;
        }

        public void Release()
        {
            Dispose();
        }

        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                return new List<ISymbol>(new ISymbol[] { _symbol });
            }
        }

        public bool Combine(IRenderer renderer)
        {
            if (renderer is SimpleRenderer && renderer != this)
            {
                if (_symbol is ISymbolCollection)
                {
                    ((ISymbolCollection)_symbol).AddSymbol(
                        ((SimpleRenderer)renderer).Symbol);
                }
                else
                {
                    ISymbolCollection symCol = PlugInManager.Create(new Guid("062AD1EA-A93C-4c3c-8690-830E65DC6D91")) as ISymbolCollection;
                    symCol.AddSymbol(_symbol);
                    symCol.AddSymbol(((SimpleRenderer)renderer).Symbol);
                    _symbol = (ISymbol)symCol;
                }

                return true;
            }

            return false;
        }
        #endregion
    }
}
