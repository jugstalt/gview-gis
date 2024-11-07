using gView.Framework.Core.Reflection;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.UI.Abstractions;
using gView.Framework.Symbology.UI.Rules;
using gView.GraphicsEngine;
using System.ComponentModel;

namespace gView.Framework.Symbology.UI;
internal class QuickPointSymbolProperties : IQuickSymbolProperties
{
    private readonly ISymbol _symbol;

    public QuickPointSymbolProperties(ISymbol symbol)
       => (_symbol) = (symbol);

    [Browsable(false)]
    internal ISymbol Symbol => _symbol;

    [Browsable(true)]
    [DisplayName("Color")]
    [PropertyDescription(BrowsableRule = typeof(QuickPointSymbolPropertyRule))]
    public ArgbColor Color
    {
        get => _symbol switch
        {
            IBrushColor brushColor => brushColor.FillColor,
            IFontColor fontColor => fontColor.FontColor,
            _ => ArgbColor.Transparent
        };
        set
        {
            if (_symbol is IBrushColor brushColor)
            {
                brushColor.FillColor = value;
            }
            if (_symbol is IFontColor fontColor)
            {
                fontColor.FontColor = value;
            }
        }
    }

    [Browsable(true)]
    [DisplayName("Size")]
    [PropertyDescription(BrowsableRule = typeof(QuickPointSymbolPropertyRule))]
    public float Size
    {
        get => _symbol switch
        {
            ISymbolSize symbolSize => symbolSize.SymbolSize,
            _ => 0f
        };
        set
        {
            if(_symbol is ISymbolSize symbolSize)
            {
                symbolSize.SymbolSize = value;
            }
        }
    }

    [Browsable(true)]
    [DisplayName("Smoothing")]
    [PropertyDescription(BrowsableRule = typeof(QuickPointSymbolPropertyRule))]
    public SymbolSmoothing SymbolSmoothing
    {
        get => _symbol.SymbolSmoothingMode;
        set
        {
            _symbol.SymbolSmoothingMode = value;
        }
    }
}
