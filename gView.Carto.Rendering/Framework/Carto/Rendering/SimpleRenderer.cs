using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using gView.Framework;
using gView.Framework.Carto.Rendering;
using gView.Framework.Symbology;
using gView.Framework.Geometry;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.system;
using System.Reflection;
using gView.Framework.Carto.Rendering.UI;

namespace gView.Framework.Carto.Rendering
{
    public class RendererFunctions
    {
        static internal Random r = new Random(DateTime.Now.Millisecond);
        static internal Color RandomColor
        {
            get
            {
                return Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
            }
        }
        static public ISymbol CreateStandardSymbol(geometryType type)
        {
            ISymbol symbol = null;
            switch (type)
            {
                case geometryType.Envelope:
                case geometryType.Polygon:
                    symbol = new SimpleFillSymbol();
                    ((SimpleFillSymbol)symbol).OutlineSymbol = new SimpleLineSymbol();
                    ((SimpleFillSymbol)symbol).Color = RandomColor;
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Color = RandomColor;
                    break;
                case geometryType.Polyline:
                    symbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)symbol).Color = RandomColor;
                    break;
                case geometryType.Multipoint:
                case geometryType.Point:
                    symbol = new SimplePointSymbol();
                    ((SimplePointSymbol)symbol).Color = RandomColor;
                    break;
            }
            return symbol;
        }

        static public ISymbol CreateStandardSelectionSymbol(geometryType type)
        {
            ISymbol symbol = null;
            switch (type)
            {
                case geometryType.Envelope:
                case geometryType.Polygon:
                    symbol = new SimpleFillSymbol();
                    ((SimpleFillSymbol)symbol).Color = Color.Transparent;
                    ((SimpleFillSymbol)symbol).OutlineSymbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Color = Color.Cyan;
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Width = 3;
                    break;
                case geometryType.Polyline:
                    symbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)symbol).Color = Color.Cyan;
                    ((SimpleLineSymbol)symbol).Width = 3;
                    break;
                case geometryType.Point:
                    symbol = new SimplePointSymbol();
                    ((SimplePointSymbol)symbol).Color = Color.Cyan;
                    ((SimplePointSymbol)symbol).Size = 5;
                    break;
            }
            return symbol;
        }

        static public ISymbol CreateStandardHighlightSymbol(geometryType type)
        {
            ISymbol symbol = null;
            switch (type)
            {
                case geometryType.Envelope:
                case geometryType.Polygon:
                    symbol = new SimpleFillSymbol();
                    ((SimpleFillSymbol)symbol).Color = Color.FromArgb(100, 255, 255, 0);
                    ((SimpleFillSymbol)symbol).OutlineSymbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Color = Color.Yellow;
                    ((SimpleLineSymbol)((SimpleFillSymbol)symbol).OutlineSymbol).Width = 5;
                    break;
                case geometryType.Polyline:
                    symbol = new SimpleLineSymbol();
                    ((SimpleLineSymbol)symbol).Color = Color.Yellow;
                    ((SimpleLineSymbol)symbol).Width = 5;
                    break;
                case geometryType.Point:
                    symbol = new SimplePointSymbol();
                    ((SimplePointSymbol)symbol).Color = Color.Yellow;
                    ((SimplePointSymbol)symbol).Size = 10;
                    break;
            }
            return symbol;
        }
    }

    [gView.Framework.system.RegisterPlugIn("646386E4-D010-4c7d-98AA-8C1903A3D5E4")]
    public class SimpleRenderer : Cloner, IFeatureRenderer2, IPropertyPage, ILegendGroup, ISymbolCreator
    {
        public enum CartographicMethod { Simple = 0, SymbolOrder = 1 }

        private ISymbol _symbol;
        private SymbolRotation _symbolRotation;
        private bool _useRefScale = true, _rotate = false;
        private CartographicMethod _cartoMethod = CartographicMethod.Simple, _actualCartoMethod = CartographicMethod.Simple;
        private List<IFeature> _features = null;

        public SimpleRenderer()
        {
            _symbolRotation = new SymbolRotation();
        }

        public void Dispose()
        {
            if (_symbol != null)
                _symbol.Release();
            _symbol = null;
        }

        public ISymbol Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                _rotate = (_symbol is ISymbolRotation && _symbolRotation != null && _symbolRotation.RotationFieldName != "");
            }
        }

        public ISymbol CreateStandardSymbol(geometryType type)
        {
            return RendererFunctions.CreateStandardSymbol(type);
        }

        public ISymbol CreateStandardSelectionSymbol(geometryType type)
        {
            return RendererFunctions.CreateStandardSelectionSymbol(type);
        }

        public ISymbol CreateStandardHighlightSymbol(geometryType type)
        {
            return RendererFunctions.CreateStandardHighlightSymbol(type);
        }

        public SymbolRotation SymbolRotation
        {
            get { return _symbolRotation; }
            set
            {
                if (value == null)
                    _symbolRotation.RotationFieldName = "";
                else
                    _symbolRotation = value;

                _rotate = (_symbol is ISymbolRotation && _symbolRotation != null && _symbolRotation.RotationFieldName != "");
            }
        }

        public CartographicMethod CartoMethod
        {
            get { return _cartoMethod; }
            set { _cartoMethod = value; }
        }

        #region IFeatureRenderer

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            if (layer == null) return false;
            if (layer.FeatureClass == null) return false;
            /*
            if (layer.FeatureClass.GeometryType == geometryType.Unknown ||
                layer.FeatureClass.GeometryType == geometryType.Network) return false;
             * */
            if (layer.LayerGeometryType == geometryType.Unknown ||
                layer.LayerGeometryType == geometryType.Network) return false;

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
            if (!(_symbol is ISymbolCollection) ||
                ((ISymbolCollection)_symbol).Symbols.Count < 2)
            {
                _actualCartoMethod = CartographicMethod.Simple;
            }
            else
            {
                _actualCartoMethod = _cartoMethod;
            }

            if (_rotate && layer.FeatureClass.FindField(_symbolRotation.RotationFieldName) != null)
                filter.AddField(_symbolRotation.RotationFieldName);
        }

        /*
		public void Draw(IDisplay disp,IFeatureCursor fCursor,DrawPhase drawPhase,ICancelTracker cancelTracker) 
		{
			if(_symbol==null) return;
			IFeature feature;
			
			try 
			{
				while((feature=fCursor.NextFeature)!=null) 
				{
					//_symbol.Draw(disp,feature.Shape);
					if(cancelTracker!=null) 
						if(!cancelTracker.Continue) 
							return;
					disp.Draw(_symbol,feature.Shape);
				}
			} 
			catch(Exception ex)
			{
				string msg=ex.Message;
			}
		}
         * */
        public void Draw(IDisplay disp, IFeature feature)
        {
            /*
            if (feature.Shape is IPolyline)
            {
                ISymbol symbol = RendererFunctions.CreateStandardSymbol(geometryType.Polygon);
                disp.Draw(symbol, ((ITopologicalOperation)feature.Shape).Buffer(30));
            }
            if (feature.Shape is IPoint)
            {
                ISymbol symbol = RendererFunctions.CreateStandardSymbol(geometryType.Polygon);
                disp.Draw(symbol, ((ITopologicalOperation)feature.Shape).Buffer(30));
            }
            if (feature.Shape is IPolygon)
            {
                IPolygon buffer = ((ITopologicalOperation)feature.Shape).Buffer(4.0);
                if (buffer != null) disp.Draw(_symbol, buffer);
            }*/

            if (_actualCartoMethod == CartographicMethod.Simple)
            {
                if (_rotate)
                {
                    object rot = feature[_symbolRotation.RotationFieldName];
                    if (rot != null && rot != DBNull.Value)
                    {
                        ((ISymbolRotation)_symbol).Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(rot));
                    }
                    else
                    {
                        ((ISymbolRotation)_symbol).Rotation = 0;
                    }
                }
                disp.Draw(_symbol, feature.Shape);
            }
            else if (_actualCartoMethod == CartographicMethod.SymbolOrder)
            {
                if (_features == null)
                    _features = new List<IFeature>();
                _features.Add(feature);
            }
        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
            if (cancelTracker == null) cancelTracker = new CancelTracker();

            if (_actualCartoMethod == CartographicMethod.SymbolOrder && cancelTracker.Continue)
            {
                ISymbolCollection sColl = (ISymbolCollection)_symbol;
                foreach (ISymbolCollectionItem symbolItem in sColl.Symbols)
                {
                    if (symbolItem.Visible == false || symbolItem.Symbol == null)
                        continue;

                    ISymbol symbol = symbolItem.Symbol;
                    bool isRotatable = symbol is ISymbolRotation;

                    int counter = 0;
                    if (!cancelTracker.Continue)
                        break;

                    foreach (IFeature feature in _features)
                    {
                        if (_rotate && isRotatable)
                        {
                            object rot = feature[_symbolRotation.RotationFieldName];
                            if (rot != null && rot != DBNull.Value)
                            {
                                ((ISymbolRotation)symbol).Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(rot));
                            }
                            else
                            {
                                ((ISymbolRotation)symbol).Rotation = 0;
                            }
                        }
                        disp.Draw(symbol, feature.Shape);

                        counter++;
                        if (counter % 100 == 0 && !cancelTracker.Continue)
                            break;
                    }
                }
            }

            if (_features != null)
                _features.Clear();
            _features = null;
        }

        public string Name
        {
            get { return "Single Symbol"; }
        }
        public string Category
        {
            get { return "Features"; }
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

                if (_symbol == null)
                {
                    _symbol = RendererFunctions.CreateStandardSymbol(((IFeatureLayer)initObject).LayerGeometryType/*((IFeatureLayer)initObject).FeatureClass.GeometryType*/);
                }
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Carto.Rendering.UI.dll");

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
            _symbolRotation = (SymbolRotation)stream.Load("SymbolRotation", _symbolRotation, _symbolRotation);

            _rotate = ((_symbol is ISymbolRotation) && _symbolRotation != null && _symbolRotation.RotationFieldName != "");
            _cartoMethod = (CartographicMethod)stream.Load("CartographicMethod", (int)CartographicMethod.Simple);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("Symbol", _symbol);
            if (_symbolRotation.RotationFieldName != "")
            {
                stream.Save("SymbolRotation", _symbolRotation);
            }
            stream.Save("CartographicMethod", (int)_cartoMethod);
        }

        #endregion

        #region ILegendGroup Members

        public int LegendItemCount
        {
            get { return (_symbol == null) ? 0 : 1; }
        }

        public ILegendItem LegendItem(int index)
        {
            if (index == 0) return _symbol;
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (_symbol == null) return;
            if (item == symbol) return;

            if (item == _symbol)
            {
                if (symbol is ILegendItem && _symbol is ILegendItem)
                {
                    ((ILegendItem)symbol).LegendLabel = ((ILegendItem)_symbol).LegendLabel;
                }
                _symbol.Release();
                _symbol = symbol;
            }
        }
        #endregion

        #region IClone2
        public object Clone(IDisplay display)
        {
            SimpleRenderer renderer = new SimpleRenderer();
            if (_symbol != null)
                renderer._symbol = (ISymbol)_symbol.Clone(_useRefScale ? display : null);

            renderer._symbolRotation = (SymbolRotation)_symbolRotation.Clone();
            renderer._rotate = _rotate;
            renderer._cartoMethod = _cartoMethod;
            renderer._actualCartoMethod = _actualCartoMethod;

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
