using gView.Framework.Carto.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using gView.Framework.Sys.UI.Extensions;

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

                if (SymbolDict != null)
                {
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

            if (GraphicsEngine.Current.Engine != null)
            {
                using (var bitmap = GraphicsEngine.Current.Engine.CreateBitmap(rect.Width, rect.Height))
                using (var canvas = bitmap.CreateCanvas())
                {
                    new SymbolPreview(null).Draw(canvas, new GraphicsEngine.CanvasRectangle(0, 0, rect.Width, rect.Height), ((SymbolListViewItem)e.Item).Symbol.Clone(null) as ISymbol, false);
                    e.Graphics.DrawImage(bitmap.ToGdiBitmap(), rect, new Rectangle(0, 0, rect.Width, rect.Height), GraphicsUnit.Pixel);
                }
            }
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

        static SymbolSelectorControl()
        {
            if(GraphicsEngine.Current.Engine!=null)
            {
                SymbolDict = new Dictionary<Type, List<ISymbol>>()
                {
                    {
                        typeof(IPointSymbol),
                        new List<ISymbol>()
                        {
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Yellow,
                                 PenColor = GraphicsEngine.ArgbColor.Red,
                                 PenWidth = 2,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Star,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Yellow,
                                 PenColor = GraphicsEngine.ArgbColor.Red,
                                 PenWidth = 2,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Cross,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Yellow,
                                 PenColor = GraphicsEngine.ArgbColor.Red,
                                 PenWidth = 2,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Square,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Yellow,
                                 PenColor = GraphicsEngine.ArgbColor.Red,
                                 PenWidth = 2,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Triangle,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Yellow,
                                 PenColor = GraphicsEngine.ArgbColor.Red,
                                 PenWidth = 2,
                                 Size = 20,
                                 Angle = 90f,
                                 Marker = SimplePointSymbol.MarkerType.Triangle,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Yellow,
                                 PenColor = GraphicsEngine.ArgbColor.Red,
                                 PenWidth = 2,
                                 Size = 20,
                                 Angle = 180f,
                                 Marker = SimplePointSymbol.MarkerType.Triangle,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Red,
                                 PenColor = GraphicsEngine.ArgbColor.Transparent,
                                 PenWidth = 0,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Star,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Red,
                                 PenColor = GraphicsEngine.ArgbColor.Red,
                                 PenWidth = 0,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Cross,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Red,
                                 PenColor = GraphicsEngine.ArgbColor.Transparent,
                                 PenWidth = 0,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Square,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Red,
                                 PenColor = GraphicsEngine.ArgbColor.Transparent,
                                 PenWidth = 0,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Triangle,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Red,
                                 PenColor = GraphicsEngine.ArgbColor.Transparent,
                                 PenWidth = 0,
                                 Size = 20,
                                 Marker = SimplePointSymbol.MarkerType.Triangle,
                                 Angle = 90f,
                                 SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimplePointSymbol()
                            {
                                 FillColor = GraphicsEngine.ArgbColor.Red,
                                 PenColor = GraphicsEngine.ArgbColor.Transparent,
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
                                Color = GraphicsEngine.ArgbColor.Red,
                                Width = 3,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleLineSymbol()
                            {
                                Color = GraphicsEngine.ArgbColor.Red,
                                Width = 3,
                                DashStyle = GraphicsEngine.LineDashStyle.Dot,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleLineSymbol()
                            {
                                Color = GraphicsEngine.ArgbColor.Red,
                                Width = 3,
                                DashStyle = GraphicsEngine.LineDashStyle.Dash,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleLineSymbol()
                            {
                                Color = GraphicsEngine.ArgbColor.Red,
                                Width = 3,
                                DashStyle = GraphicsEngine.LineDashStyle.DashDot,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SymbolCollection(new List<ISymbol>(){
                                new SimpleLineSymbol() { Color = GraphicsEngine.ArgbColor.Red, Width = 6 },
                                new SimpleLineSymbol() { Color = GraphicsEngine.ArgbColor.Yellow, Width = 4 },
                            }) { SymbolSmothingMode = SymbolSmoothing.AntiAlias },
                            new SymbolCollection(new List<ISymbol>(){
                                new SimpleLineSymbol() { Color = GraphicsEngine.ArgbColor.Red, Width = 6 },
                                new SimpleLineSymbol() { Color = GraphicsEngine.ArgbColor.Yellow, Width = 2, DashStyle = GraphicsEngine.LineDashStyle.Dash },
                            }) { SymbolSmothingMode = SymbolSmoothing.AntiAlias },
                            new SymbolDotedLineSymbol()
                            {
                                LineSymbol=new SimpleLineSymbol()
                                {
                                    Color = GraphicsEngine.ArgbColor.Gray,
                                    Width = 2
                                },
                                PointSymbol = new SimplePointSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Transparent,
                                    FillColor = GraphicsEngine.ArgbColor.Gray,
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
                                    PenColor = GraphicsEngine.ArgbColor.Transparent,
                                    FillColor = GraphicsEngine.ArgbColor.Gray,
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
                                    PenColor = GraphicsEngine.ArgbColor.Gray,
                                    PenWidth = 1f
                                },
                                FillColor = GraphicsEngine.ArgbColor.FromArgb(200, GraphicsEngine.ArgbColor.Red),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleFillSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Gray,
                                    PenWidth = 1f
                                },
                                FillColor = GraphicsEngine.ArgbColor.FromArgb(200, GraphicsEngine.ArgbColor.Green),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleFillSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Gray,
                                    PenWidth = 1f
                                },
                                FillColor = GraphicsEngine.ArgbColor.FromArgb(200, GraphicsEngine.ArgbColor.Blue),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleFillSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Gray,
                                    PenWidth = 3f
                                },
                                FillColor = GraphicsEngine.ArgbColor.Transparent,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleFillSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Red,
                                    PenWidth = 3f
                                },
                                FillColor = GraphicsEngine.ArgbColor.FromArgb(200,GraphicsEngine.ArgbColor.Yellow),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleFillSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Green,
                                    PenWidth = 3f
                                },
                                FillColor = GraphicsEngine.ArgbColor.FromArgb(100, GraphicsEngine.ArgbColor.AliceBlue),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new HatchSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Red,
                                    PenWidth = 1f
                                },
                                HatchStyle = GraphicsEngine.HatchStyle.Cross,
                                FillColor = GraphicsEngine.ArgbColor.Red,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new HatchSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Red,
                                    PenWidth = 1f
                                },
                                HatchStyle = GraphicsEngine.HatchStyle.DashedHorizontal,
                                FillColor = GraphicsEngine.ArgbColor.Red,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new HatchSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Red,
                                    PenWidth = 1f
                                },
                                HatchStyle = GraphicsEngine.HatchStyle.DiagonalBrick,
                                FillColor = GraphicsEngine.ArgbColor.Red,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new HatchSymbol()
                            {
                                OutlineSymbol = new SimpleLineSymbol()
                                {
                                    PenColor = GraphicsEngine.ArgbColor.Red,
                                    PenWidth = 1f
                                },
                                HatchStyle = GraphicsEngine.HatchStyle.DarkVertical,
                                FillColor = GraphicsEngine.ArgbColor.Red,
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
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 10f),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 15f),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 20f),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new SimpleTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 30f),
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new GlowingTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 10f),
                                GlowingColor = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new GlowingTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 15f),
                                GlowingColor = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new GlowingTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 20f),
                                GlowingColor = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new GlowingTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 30f),
                                GlowingColor = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new BlockoutTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 10f),
                                ColorOutline = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new BlockoutTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 15f),
                                ColorOutline = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new BlockoutTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 20f),
                                ColorOutline = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                            new BlockoutTextSymbol()
                            {
                                FontColor = GraphicsEngine.ArgbColor.Black,
                                Font = GraphicsEngine.Current.Engine?.CreateFont("Arial", 30f),
                                ColorOutline = GraphicsEngine.ArgbColor.Yellow,
                                SymbolSmothingMode = SymbolSmoothing.AntiAlias
                            },
                        }
                    }
                };
            }
        }

        private static Dictionary<Type, List<ISymbol>> SymbolDict = null;

        #endregion Predefined Symbols

        public ISymbol SelectedSymbol { get; private set; }

        private void listViewSymbols_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedSymbol =
                listViewSymbols.SelectedItems.Count == 1 ?
                ((SymbolListViewItem)listViewSymbols.SelectedItems[0]).Symbol.Clone(null) as ISymbol :
                null;

            OnSymbolSelected?.Invoke(this, new EventArgs());
        }
    }
}