using gView.Framework.Reflection;
using gView.Framework.Symbology.UI.Abstractions;
using gView.Framework.Symbology.UI.Rules;
using gView.GraphicsEngine;
using System.ComponentModel;

namespace gView.Framework.Symbology.UI;

internal class QuickLineSymbolProperties : IQuickSymbolProperties
{
    private readonly ISymbol _symbol;

    public QuickLineSymbolProperties(ISymbol symbol)
       => (_symbol) = (symbol);

    [Browsable(false)]
    internal ISymbol Symbol => _symbol;

    [Browsable(true)]
    [DisplayName("Color")]
    [PropertyDescription(BrowsableRule = typeof(QuickLineSymbolPropertyRule))]
    public ArgbColor Color
    {
        get => _symbol switch
        {
            IPenColor penColor => penColor.PenColor,
            _ => ArgbColor.Transparent
        };
        set
        {
            if (_symbol is IPenColor penColor)
            {
                penColor.PenColor = value;
            }
        }
    }

    [Browsable(true)]
    [DisplayName("Width")]
    [PropertyDescription(BrowsableRule = typeof(QuickLineSymbolPropertyRule))]
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
    [PropertyDescription(BrowsableRule = typeof(QuickLineSymbolPropertyRule))]
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
    [DisplayName("Smoothing")]
    [PropertyDescription(BrowsableRule = typeof(QuickLineSymbolPropertyRule))]
    public SymbolSmoothing SymbolSmoothing
    {
        get => _symbol.SymbolSmoothingMode;
        set
        {
            _symbol.SymbolSmoothingMode = value;
        }
    }
}
