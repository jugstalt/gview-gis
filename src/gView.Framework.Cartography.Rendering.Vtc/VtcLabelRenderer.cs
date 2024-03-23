using gView.DataSources.VectorTileCache.Json;
using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Vtc;
using gView.Framework.Symbology.Vtc.Extensions;
using gView.GraphicsEngine;
using System.Security.Cryptography;

namespace gView.Framework.Cartography.Rendering.Vtc;

public class VtcLabelRenderer : SimpleLabelRenderer
{
    private VtcStyleFilter? _filter = null;
    private PaintSymbol _paintSymbol;

    public VtcLabelRenderer(PaintSymbol paintSymbol) 
        : base() 
    { 
        _paintSymbol = paintSymbol;
        base.TextSymbol = _paintSymbol.TextSymbol;
    }

    protected VtcLabelRenderer(PaintSymbol paintSymbol, ITextSymbol? textSymbol, string fieldname)
        : base(textSymbol, fieldname) 
    {
        _paintSymbol = paintSymbol;
    }

    public override void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter)
    {
        base.PrepareQueryFilter(display, layer, filter);

        _filter = layer.FilterQuery as VtcStyleFilter;
    }

    protected override bool BeforeRenderFeature(IDisplay display, IFeature feature)
    {
        if (_filter != null && !_filter.Filter(feature))
        {
            return false;
        }

        CartoLineLabelling =
           _paintSymbol.GetValueOrDeafult(GLStyleProperties.SymbolPlacement, "", display, feature) == "line"
                            ? CartographicLineLabeling.CurvedText
                            : CartographicLineLabeling.Horizontal;

        //if(CartoLineLabelling == CartographicLineLabeling.CurvedText)
        //{
        //    HowManyLabels =
        //        _paintSymbol.GetValueOrDeafult(GLStyleProperties.SymbolSpacing, 0f, display, feature) > 0
        //                            ? RenderHowManyLabels.OnPerName
        //                           : RenderHowManyLabels.OnPerFeature;
        //}

        var textSymbol = base.TextSymbol;

        if (textSymbol != null)
        {
            var offset = _paintSymbol.GetValueOrDeafult<float[]?>(GLStyleProperties.TextOffset, null, display, feature);
            if (offset != null && offset.Length == 2)
            {
                textSymbol.VerticalOffset = offset[0];
                textSymbol.HorizontalOffset = offset[1];
            }

            textSymbol.TextSymbolAlignment =
                _paintSymbol.GetValueOrDeafult(GLStyleProperties.TextAnchor, "", display, feature) switch
                {
                    "top" => TextSymbolAlignment.Under,
                    "bottom" => TextSymbolAlignment.Over,
                    _ => TextSymbolAlignment.Center
                };
        }

        if (textSymbol is ISymbolSpacing spacing)
        {
            var spacingSize = _paintSymbol.GetValueOrDeafult(
                    GLStyleProperties.SymbolSpacing,
                    0f,  // todo: 0f if text-allow-overlap = true
                    display,
                    feature);

            if(spacingSize>0)
            {
                spacing.SymbolSpacingType = SymbolSpacingType.BoundingBox;
                spacing.SymbolSpacingX = spacing.SymbolSpacingY = spacingSize;
            } 
            else
            {
                spacing.SymbolSpacingType = SymbolSpacingType.None;
            }
        }

        if(textSymbol is IFontColor fontColor)
        {
            fontColor.FontColor =
                _paintSymbol.GetValueOrDeafult(GLStyleProperties.TextColor, fontColor.FontColor, display, feature);
        }
        if (textSymbol is IFontSymbol fontSymbol)
        {
            var fontSize =
                _paintSymbol.GetValueOrDeafult(GLStyleProperties.TextSize, fontSymbol.Font.Size, display, feature);
            
            if (fontSize != fontSymbol.Font.Size)
            {
                // only if differs
                fontSymbol.Font = Current.Engine.CreateFont(
                    fontSymbol.Font.Name,
                    fontSize,
                    fontSymbol.Font.Style,
                    fontSymbol.Font.Unit);
            }
        }

        return true;
    }

    protected override string ModifyEvaluatedLabel(IFeature feature, string label)
    {
        // remove placeholder 
        // if attribute not exits in feature, there are still the placeholders in the exrpression

        if (label.Contains("[") && label.Contains("]"))
        {
            return RemovePlaceholders(label);
        }

        return label;
    }

    protected override SimpleLabelRenderer CreateCloneInstance(CloneOptions options)
        => new VtcLabelRenderer(
                (PaintSymbol)_paintSymbol.Clone(),
                (ITextSymbol?)(base.TextSymbol is IClone2 ? base.TextSymbol.Clone(options) : null),
                base.FieldName);

    #region Helpers

    static string RemovePlaceholders(string text)
    {
        int startIndex, endIndex;

        while ((startIndex = text.IndexOf('[')) != -1 && (endIndex = text.IndexOf(']', startIndex)) != -1)
        {
            text = text.Remove(startIndex, endIndex - startIndex + 1);
        }

        return text;
    }

    #endregion
}
