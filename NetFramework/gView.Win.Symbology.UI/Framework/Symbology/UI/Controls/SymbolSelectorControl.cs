using gView.Framework.Carto.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Framework.Symbology.UI.Controls
{
    public partial class SymbolSelectorControl : UserControl
    {
        public SymbolSelectorControl()
        {
            InitializeComponent();
        }

        public ISymbol SymbolProtoType
        {
            set
            {
                var symbolProtoType = value;

                this.listViewSymbols.Items.Clear();

                if (symbolProtoType is IPointSymbol)
                {
                    AddSymbols(SymbolDict[typeof(IPointSymbol)]);
                }
                if (symbolProtoType is ILineSymbol)
                {
                }
                if (symbolProtoType is IFillSymbol)
                {
                    AddSymbols(SymbolDict[typeof(IFillSymbol)]);
                }
                if (symbolProtoType is ITextSymbol)
                {
                }
            }
        }

        private void AddSymbols(IEnumerable<ISymbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                this.listViewSymbols.Items.Add(new SymbolListViewItem(symbol));
            }
        }

        private void listViewSymbols_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            Rectangle rect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 10, e.Bounds.Width - 20, e.Bounds.Height - 20);
            using (SolidBrush brush = new SolidBrush(Color.Gray))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            //using (Pen pen = new Pen(Color.Gray, 0))
            //{
            //    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            //    e.Graphics.DrawLine(pen, 0, rect.Height / 2, rect.Width, rect.Height / 2);
            //    e.Graphics.DrawLine(pen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
            //}

            new SymbolPreview(null).Draw(e.Graphics, rect,
                ((SymbolListViewItem)e.Item).Symbol,
                false);
        }

        private void SymbolSelectorControl_Load(object sender, EventArgs e)
        {
        }

        #region Item Classes

        private class SymbolListViewItem : ListViewItem
        {
            public SymbolListViewItem(ISymbol symbol)
            {
                this.Symbol = symbol;
            }

            public ISymbol Symbol { get; }
        }

        #endregion Item Classes

        #region Predefined Symbols

        private static Dictionary<Type, List<ISymbol>> SymbolDict = new Dictionary<Type, List<ISymbol>>()
        {
            {
                typeof(IPointSymbol),
                new List<ISymbol>()
                {
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Yellow,
                         PenColor = Color.Red,
                         PenWidth = 2,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Star,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    }
                }
            },
            {
                typeof(IFillSymbol),
                new List<ISymbol>()
                {
                    new SimpleFillSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Gray,
                            PenWidth = 1f
                        },
                        FillColor = Color.FromArgb(200,Color.Red),
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleFillSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Gray,
                            PenWidth = 1f
                        },
                        FillColor = Color.FromArgb(200,Color.Green),
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleFillSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Gray,
                            PenWidth = 1f
                        },
                        FillColor = Color.FromArgb(200,Color.Blue),
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleFillSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Red,
                            PenWidth = 3f
                        },
                        FillColor = Color.FromArgb(200,Color.Yellow),
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleFillSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Green,
                            PenWidth = 3f
                        },
                        FillColor = Color.FromArgb(100, Color.AliceBlue),
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new HatchSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Red,
                            PenWidth = 1f
                        },
                        HatchStyle = System.Drawing.Drawing2D.HatchStyle.Cross,
                        FillColor = Color.Red,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new HatchSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Red,
                            PenWidth = 1f
                        },
                        HatchStyle = System.Drawing.Drawing2D.HatchStyle.DashedHorizontal,
                        FillColor = Color.Red,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new HatchSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Red,
                            PenWidth = 1f
                        },
                        HatchStyle = System.Drawing.Drawing2D.HatchStyle.DiagonalBrick,
                        FillColor = Color.Red,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new HatchSymbol()
                    {
                        OutlineSymbol = new SimpleLineSymbol()
                        {
                            PenColor = Color.Red,
                            PenWidth = 1f
                        },
                        HatchStyle = System.Drawing.Drawing2D.HatchStyle.DarkVertical,
                        FillColor = Color.Red,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new GradientFillSymbol()
                    {
                    }
                }
            }
        };

        #endregion Predefined Symbols
    }
}