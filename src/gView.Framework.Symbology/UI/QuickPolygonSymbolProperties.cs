using gView.Framework.Core.Reflection;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.UI.Abstractions;
using gView.Framework.Symbology.UI.Rules;
using gView.GraphicsEngine;
using System.ComponentModel;

namespace gView.Framework.Symbology.UI;

internal class QuickPolygonSymbolProperties : IQuickSymbolProperties
{
    private readonly ISymbol _symbol;

    public QuickPolygonSymbolProperties(ISymbol symbol)
       => (_symbol) = (symbol);

    [Browsable(false)]
    internal ISymbol Symbol => _symbol;

    [Browsable(true)]
    [DisplayName("Color")]
    [PropertyDescription(BrowsableRule = typeof(QuickPolygonSymbolPropertyRule))]
    public ArgbColor Color
    {
        get => _symbol switch
        {
            IBrushColor brushColor when brushColor.FillColor.A > 0 => ArgbColor.FromArgb(255, brushColor.FillColor),
            IPenColor penColor => penColor.PenColor,
            _ => ArgbColor.Transparent
        };
        set
        {
            if (_symbol is IBrushColor brushColor)
            {
                brushColor.FillColor = ArgbColor.FromArgb(brushColor.FillColor.A, value);
            }
            if (_symbol is IPenColor penColor)
            {
                penColor.PenColor = value;
            }
        }
    }

    [Browsable(true)]
    [DisplayName("Width")]
    [PropertyDescription(BrowsableRule = typeof(QuickPolygonSymbolPropertyRule))]
    public float Width
    {
        get => _symbol switch
        {
            IPenWidth penWidth => penWidth.PenWidth,
            _ => 0f
        };
        set
        {
            if (_symbol is IPenWidth penWidth)
            {
                penWidth.PenWidth = value;
            }
        }
    }

    [Browsable(true)]
    [DisplayName("DashStyle")]
    [PropertyDescription(BrowsableRule = typeof(QuickPolygonSymbolPropertyRule))]
    public LineDashStyle DashStyle
    {
        get => _symbol switch
        {
            IPenDashStyle penDashStyle => penDashStyle.PenDashStyle,
            _ => LineDashStyle.Solid
        };
        set
        {
            if (_symbol is IPenDashStyle penDashStyle)
            {
                penDashStyle.PenDashStyle = value;
            }
        }
    }

    [Browsable(true)]
    [DisplayName("Opacity")]
    [PropertyDescription(
        MinValue = 0f,
        MaxValue = 100f,
        RangeStep = 1f,
        LabelFormat = "{0:0}%",
        BrowsableRule = typeof(QuickPolygonSymbolPropertyRule))]
    public float Opacity
    {
        get => _symbol switch
        {
            IBrushColor brushColor => brushColor.FillColor.A / 2.55f,
            _ => 0
        };
        set
        {
            if (_symbol is IBrushColor brushColor)
            {
                brushColor.FillColor = ArgbColor.FromArgb((int)(value * 2.55f), brushColor.FillColor);
            }
        }
    }

    [Browsable(true)]
    [DisplayName("Smoothing")]
    [PropertyDescription(BrowsableRule = typeof(QuickPolygonSymbolPropertyRule))]
    public SymbolSmoothing SymbolSmoothing
    {
        get => _symbol.SymbolSmoothingMode;
        set
        {
            _symbol.SymbolSmoothingMode = value;
        }
    }
}
