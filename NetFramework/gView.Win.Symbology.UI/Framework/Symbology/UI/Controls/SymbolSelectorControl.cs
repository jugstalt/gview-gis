using gView.Framework.Carto.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace gView.Framework.Symbology.UI.Controls
{
    public partial class SymbolSelectorControl : UserControl
    {
        public SymbolSelectorControl()
        {
            InitializeComponent();
        }

        public event EventHandler OnSymbolSelected;

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
                    AddSymbols(SymbolDict[typeof(ILineSymbol)]);
                }
                if (symbolProtoType is IFillSymbol)
                {
                    AddSymbols(SymbolDict[typeof(IFillSymbol)]);
                }
                if (symbolProtoType is ITextSymbol)
                {
                    AddSymbols(SymbolDict[typeof(ITextSymbol)]);
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
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            using (Pen pen = new Pen(Color.Gray, 1))
            {
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(e.Bounds.X + 5, e.Bounds.Y + 5, e.Bounds.Width - 10, e.Bounds.Height - 10));
                e.Graphics.DrawRectangle(pen, new Rectangle(e.Bounds.X + 5, e.Bounds.Y + 5, e.Bounds.Width - 10, e.Bounds.Height - 10));
            }

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
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Yellow,
                         PenColor = Color.Red,
                         PenWidth = 2,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Cross,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Yellow,
                         PenColor = Color.Red,
                         PenWidth = 2,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Square,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Yellow,
                         PenColor = Color.Red,
                         PenWidth = 2,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Triangle,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Yellow,
                         PenColor = Color.Red,
                         PenWidth = 2,
                         Size = 20,
                         Angle = 90f,
                         Marker = SimplePointSymbol.MarkerType.Triangle,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Yellow,
                         PenColor = Color.Red,
                         PenWidth = 2,
                         Size = 20,
                         Angle = 180f,
                         Marker = SimplePointSymbol.MarkerType.Triangle,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Red,
                         PenColor = Color.Transparent,
                         PenWidth = 0,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Star,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Red,
                         PenColor = Color.Red,
                         PenWidth = 0,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Cross,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Red,
                         PenColor = Color.Transparent,
                         PenWidth = 0,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Square,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Red,
                         PenColor = Color.Transparent,
                         PenWidth = 0,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Triangle,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Red,
                         PenColor = Color.Transparent,
                         PenWidth = 0,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Triangle,
                         Angle = 90f,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimplePointSymbol()
                    {
                         FillColor = Color.Red,
                         PenColor = Color.Transparent,
                         PenWidth = 0,
                         Size = 20,
                         Marker = SimplePointSymbol.MarkerType.Triangle,
                         Angle = 180f,
                         SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    }
                }
            },
            {
                typeof(ILineSymbol),
                new List<ISymbol>()
                {
                    new SimpleLineSymbol()
                    {
                        Color = Color.Red,
                        Width = 3,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleLineSymbol()
                    {
                        Color = Color.Red,
                        Width = 3,
                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dot,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleLineSymbol()
                    {
                        Color = Color.Red,
                        Width = 3,
                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleLineSymbol()
                    {
                        Color = Color.Red,
                        Width = 3,
                        DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SymbolCollection(new List<ISymbol>(){
                        new SimpleLineSymbol() { Color = Color.Red, Width = 6 },
                        new SimpleLineSymbol() { Color = Color.Yellow, Width = 4 },
                    }) { SymbolSmothingMode = SymbolSmoothing.AntiAlias },
                    new SymbolCollection(new List<ISymbol>(){
                        new SimpleLineSymbol() { Color = Color.Red, Width = 6 },
                        new SimpleLineSymbol() { Color = Color.Yellow, Width = 2, DashStyle = System.Drawing.Drawing2D.DashStyle.Dash },
                    }) { SymbolSmothingMode = SymbolSmoothing.AntiAlias },
                    new SymbolDotedLineSymbol()
                    {
                        LineSymbol=new SimpleLineSymbol()
                        {
                            Color = Color.Gray,
                            Width = 2
                        },
                        PointSymbol = new SimplePointSymbol()
                        {
                            PenColor = Color.Transparent,
                            FillColor = Color.Gray,
                            Size = 10,
                            Marker = SimplePointSymbol.MarkerType.Triangle,
                            Angle = 90f,
                        },
                        DrawEndPoint = true,
                        DrawStartPoint=true,
                        DrawStepPoints=true,
                        StepWidth = 20,
                        StepWidthUnit = StepWidthUnit.Pixel,
                        UseSymbolRotation = true,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SymbolDotedLineSymbol()
                    {
                        PointSymbol = new SimplePointSymbol()
                        {
                            PenColor = Color.Transparent,
                            FillColor = Color.Gray,
                            Size = 10,
                            Marker = SimplePointSymbol.MarkerType.Circle,
                            Angle = 90f,
                        },
                        DrawEndPoint = true,
                        DrawStartPoint=true,
                        DrawStepPoints=true,
                        StepWidth = 20,
                        StepWidthUnit = StepWidthUnit.Pixel,
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
                            PenColor = Color.Gray,
                            PenWidth = 3f
                        },
                        FillColor = Color.Transparent,
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
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    }
                }
            },
            {
                typeof(ITextSymbol),
                new List<ISymbol>()
                {
                    new SimpleTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 10f),
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 15f),
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 20f),
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new SimpleTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 30f),
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new GlowingTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 10f),
                        GlowingColor = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new GlowingTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 15f),
                        GlowingColor = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new GlowingTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 20f),
                        GlowingColor = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new GlowingTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 30f),
                        GlowingColor = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new BlockoutTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 10f),
                        ColorOutline = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new BlockoutTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 15f),
                        ColorOutline = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new BlockoutTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 20f),
                        ColorOutline = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                    new BlockoutTextSymbol()
                    {
                        FontColor = Color.Black,
                        Font = new Font("Arial", 30f),
                        ColorOutline = Color.Yellow,
                        SymbolSmothingMode = SymbolSmoothing.AntiAlias
                    },
                }
            }
        };

        #endregion Predefined Symbols

        public ISymbol SelectedSymbol { get; private set; }

        private void listViewSymbols_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedSymbol =
                listViewSymbols.SelectedItems.Count == 1 ?
                ((SymbolListViewItem)listViewSymbols.SelectedItems[0]).Symbol.Clone(null) as ISymbol :
                null;

            if (this.SelectedSymbol != null)
            {
                OnSymbolSelected?.Invoke(this, new EventArgs());
            }
        }
    }
}