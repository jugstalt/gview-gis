using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("062AD1EA-A93C-4c3c-8690-830E65DC6D91")]
    public sealed class SymbolCollection : LegendItem,
                                           ISymbolCollection,
                                           ISymbol,
                                           ILabel,
                                           ISymbolRotation,
                                           ITextSymbol,
                                           IPenColor,
                                           IBrushColor,
                                           IFontColor,
                                           ISymbolCurrentGraphicsEngineDependent
    {
        private List<SymbolCollectionItem> _symbols;

        public SymbolCollection()
        {
            _symbols = new List<SymbolCollectionItem>();
        }

        public SymbolCollection(ISymbol symbol)
            : this()
        {
            AddSymbol(symbol);
        }

        public SymbolCollection(IEnumerable<ISymbol> symbols)
            : this()
        {
            if (symbols != null)
            {
                foreach (var symbol in symbols)
                {
                    AddSymbol(symbol);
                }
            }
        }

        public void AddSymbol(ISymbol symbol)
        {
            AddSymbol(symbol, true);
        }
        public void AddSymbol(ISymbol symbol, bool visible)
        {
            if (!PlugInManager.IsPlugin(symbol))
            {
                return;
            }

            if (symbol is SymbolCollection)
            {
                foreach (SymbolCollectionItem item in ((SymbolCollection)symbol).Symbols)
                {
                    _symbols.Add(item);
                }
            }
            else
            {
                _symbols.Add(new SymbolCollectionItem(symbol, visible));
            }
        }

        public void RemoveSymbol(ISymbol symbol)
        {
            SymbolCollectionItem symbolItem = null;
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == symbol)
                {
                    symbolItem = item;
                    break;
                }
            }
            if (symbolItem != null)
            {
                _symbols.Remove(symbolItem);
            }
        }

        public void InsertBefore(ISymbol symbol, ISymbol before, bool visible)
        {
            SymbolCollectionItem beforeItem = null;
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == before)
                {
                    beforeItem = item;
                    break;
                }
            }
            if (beforeItem != null)
            {
                int index = _symbols.IndexOf(beforeItem);
                if (index > -1)
                {
                    _symbols.Insert(index, new SymbolCollectionItem(symbol, visible));
                }
            }
        }

        public int IndexOf(ISymbol symbol)
        {
            int index = 0;
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == symbol)
                {
                    return index;
                }

                index++;
            }
            return -1;
        }
        public void ReplaceSymbol(ISymbol oldSymbol, ISymbol newSymbol)
        {
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == oldSymbol)
                {
                    item.Symbol = newSymbol;
                }
            }
        }
        public bool IsVisible(ISymbol symbol)
        {
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol == symbol)
                {
                    return item.Visible;
                }
            }
            return false;
        }

        public List<ISymbolCollectionItem> Symbols
        {
            get
            {
                List<ISymbolCollectionItem> list = new List<ISymbolCollectionItem>();
                foreach (SymbolCollectionItem item in _symbols)
                {
                    list.Add(item);
                }
                return list;
            }
        }

        #region ISymbol Member

        public void Release()
        {
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                if (sSym.Symbol == null)
                {
                    continue;
                }

                sSym.Symbol.Release();
            }
            _symbols.Clear();
        }

        public string Name
        {
            get
            {
                return "SymbolCollection";
            }
        }

        public bool SupportsGeometryType(GeometryType geomType)
        {
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                if (sSym?.Symbol != null)
                {
                    if (sSym.Symbol.SupportsGeometryType(geomType) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Draw(IDisplay display, IGeometry geometry)
        {
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                if (sSym.Symbol == null || !sSym.Visible)
                {
                    continue;
                }

                sSym.Symbol.Draw(display, geometry);
            }
        }

        public void Draw(IDisplay display, IGeometry geometry, TextSymbolAlignment symbolAlignment)
        {
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                if (sSym.Symbol == null || !sSym.Visible)
                {
                    continue;
                }

                if (sSym.Symbol is ITextSymbol)
                {
                    ((ITextSymbol)sSym.Symbol).Draw(display, geometry, symbolAlignment);
                }
                else
                {
                    sSym.Symbol.Draw(display, geometry);
                }
            }
        }

        #endregion

        #region ILabel
        public string Text
        {
            get
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                    {
                        return ((ITextSymbol)item.Symbol).Text;
                    }
                }
                return "";
            }
            set
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                    {
                        ((ITextSymbol)item.Symbol).Text = value;
                    }
                }
            }
        }
        public TextSymbolAlignment TextSymbolAlignment
        {
            get
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                    {
                        return ((ITextSymbol)item.Symbol).TextSymbolAlignment;
                    }
                }
                return TextSymbolAlignment.Center;
            }
            set
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                    {
                        ((ITextSymbol)item.Symbol).TextSymbolAlignment = value;
                    }
                }
            }
        }

        [Browsable(false)]
        public TextSymbolAlignment[] SecondaryTextSymbolAlignments
        {
            get
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                    {
                        return ((ITextSymbol)item.Symbol).SecondaryTextSymbolAlignments;
                    }
                }
                return new TextSymbolAlignment[0];
            }
            set
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ITextSymbol)
                    {
                        ((ITextSymbol)item.Symbol).SecondaryTextSymbolAlignments = value;
                    }
                }
            }
        }

        public IDisplayCharacterRanges MeasureCharacterWidth(IDisplay display)
        {
            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol is ILabel)
                {
                    return ((ILabel)item.Symbol).MeasureCharacterWidth(display);
                }
            }
            return null;
        }

        public List<IAnnotationPolygonCollision> AnnotationPolygon(IDisplay display, IGeometry geometry, TextSymbolAlignment symbolAlignment)
        {
            List<IAnnotationPolygonCollision> aPolygons = new List<IAnnotationPolygonCollision>();

            foreach (SymbolCollectionItem item in _symbols)
            {
                if (item.Symbol is ILabel)
                {
                    List<IAnnotationPolygonCollision> pList = ((ILabel)item.Symbol).AnnotationPolygon(display, geometry, symbolAlignment);
                    if (pList == null)
                    {
                        continue;
                    }

                    foreach (AnnotationPolygon aPolygon in pList)
                    {
                        if (aPolygons != null)
                        {
                            aPolygons.Add(aPolygon);
                        }
                    }
                }
            }

            return aPolygons;
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
            base.Load(stream);
            _symbols.Clear();

            SymbolCollectionItem item;
            while ((item = (SymbolCollectionItem)stream.Load("Item", null, new SymbolCollectionItem(null, false))) != null)
            {
                _symbols.Add(item);
            }
        }

        public void Save(IPersistStream stream)
        {
            base.Save(stream);
            foreach (SymbolCollectionItem item in _symbols)
            {
                stream.Save("Item", item);
            }
        }

        #endregion

        #region IClone2
        public object Clone(CloneOptions options)
        {
            SymbolCollection collection = new SymbolCollection();
            foreach (SymbolCollectionItem sSym in _symbols)
            {
                collection._symbols.Add(sSym.Clone(options));
            }
            collection.LegendLabel = _legendLabel;

            return collection;
        }
        #endregion

        #region ISymbolRotation Members

        public float Rotation
        {
            get
            {
                return 0;
            }
            set
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ISymbolRotation)
                    {
                        ((ISymbolRotation)item.Symbol).Rotation = value;
                    }
                }
            }
        }

        #endregion

        #region ITextSymbol Member

        public GraphicsEngine.Abstraction.IFont Font
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MaxFontSize { get; set; }

        [Browsable(true)]
        [Category("Reference Scaling")]
        public float MinFontSize { get; set; }

        #endregion

        #region ISymbolTransformation Member

        public float HorizontalOffset
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public float VerticalOffset
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        public float Angle
        {
            get
            {
                return 0;
            }
            set
            {

            }
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set
            {
                if (_symbols != null)
                {
                    foreach (SymbolCollectionItem item in _symbols)
                    {
                        if (item.Symbol != null)
                        {
                            item.Symbol.SymbolSmothingMode = value;
                        }
                    }
                }
            }
        }

        public bool RequireClone()
        {
            return _symbols.Where(s => s?.Symbol != null && s.Symbol.RequireClone()).FirstOrDefault() != null;
        }



        #endregion

        #region IPenColor Member

        public ArgbColor PenColor
        {
            get
            {
                return ArgbColor.Black;
            }
            set
            {
                if (_symbols == null)
                {
                    return;
                }

                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is IPenColor)
                    {
                        ((IPenColor)item.Symbol).PenColor = value;
                    }
                }
            }
        }

        #endregion

        #region IBrushColor Member

        public ArgbColor FillColor
        {
            get
            {
                return ArgbColor.Black;
            }
            set
            {
                if (_symbols == null)
                {
                    return;
                }

                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is IBrushColor)
                    {
                        ((IBrushColor)item.Symbol).FillColor = value;
                    }
                }
            }
        }

        #endregion

        #region IFontColor Member

        public ArgbColor FontColor
        {
            get
            {
                return ArgbColor.Black;
            }
            set
            {
                if (_symbols == null)
                {
                    return;
                }

                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is IFontColor)
                    {
                        ((IFontColor)item.Symbol).FontColor = value;
                    }
                }
            }
        }

        #endregion


        #region ISymbolCurrentGraphicsEngineDependent

        public void CurrentGraphicsEngineChanged()
        {
            if (_symbols != null)
            {
                foreach (SymbolCollectionItem item in _symbols)
                {
                    if (item.Symbol is ISymbolCurrentGraphicsEngineDependent)
                    {
                        ((ISymbolCurrentGraphicsEngineDependent)item.Symbol).CurrentGraphicsEngineChanged();
                    }
                }
            }
        }

        #endregion
    }
}
