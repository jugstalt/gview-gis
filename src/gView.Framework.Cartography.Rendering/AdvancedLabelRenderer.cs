using gView.Framework.Cartography.Rendering.Exntensions;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.IO;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.UI;
using gView.GraphicsEngine;
using gView.GraphicsEngine.Abstraction;

namespace gView.Framework.Cartography.Rendering
{
    /// <summary>
    /// A <see cref="SimpleLabelRenderer"/> with "advanced" settings that most layers never need -
    /// currently just <see cref="ColorExpression"/> - kept in a separate class/plug-in rather than
    /// added directly to <see cref="SimpleLabelRenderer"/> itself. <see cref="SimpleLabelRenderer"/>
    /// is the most widely used, performance-critical renderer in gView (Desktop and Server alike,
    /// potentially very many features per request); every property/field/branch added there
    /// permanently raises its complexity and cost for the common case - including, concretely, an
    /// otherwise avoidable color-struct construction/property write on every single
    /// <c>Draw()</c> call under a graphics engine (e.g. Skia) where that isn't free. This class is
    /// where any further "advanced" per-feature behaviour belongs instead, hooking in via
    /// <see cref="SimpleLabelRenderer.HandleAdditionalLabelRendererBehavior"/> (mirroring the
    /// existing <see cref="SimpleLabelRenderer.ModifyEvaluatedLabel"/> extension point) - all of
    /// this class's own state (<c>_colorExpression</c>, the cached base color) lives entirely here,
    /// never on the base class.
    /// </summary>
    [RegisterPlugIn("F30B5A7A-5C8C-4AD8-B61F-6CB14F239349")]
    public class AdvancedLabelRenderer : SimpleLabelRenderer
    {
        private string _colorExpression = string.Empty;

        // The symbol's own, non-conditional color - lazily captured (see
        // HandleAdditionalLabelRendererBehavior) the first time it's needed for the *current*
        // TextSymbol instance, so a feature that doesn't match any color branch can fall back to
        // it instead of keeping whatever override the previous feature left behind. Re-captured
        // whenever TextSymbol is reassigned (detected by reference, via _baseFontColorSymbol) -
        // this avoids needing SimpleLabelRenderer to expose a hook at every place it could assign
        // _symbol (constructors, the TextSymbol setter, Load, SetSymbol).
        private ArgbColor? _baseFontColor;
        private ITextSymbol _baseFontColorSymbol;

        public AdvancedLabelRenderer()
        {
        }

        public override string Name => "Advanced Text Renderer";

        protected AdvancedLabelRenderer(ITextSymbol symbol, string fieldname)
            : base(symbol, fieldname)
        {
        }

        /// <summary>
        /// A gView expression - a plain <c>[Field]</c> substitution, or a full
        /// <c>@@start/@@if.../@@end</c> conditional script, resolved exactly like
        /// <see cref="SimpleLabelRenderer.LabelExpression"/> - that resolves to a color string
        /// (see <see cref="ArgbColor.TryFromString"/>, e.g. <c>"rgb(255,0,0)"</c>, <c>"#ff0000"</c>,
        /// or a plain field reference like <c>[COLOR]</c> whose own value is one of those). Empty
        /// (the default) or a value that doesn't resolve to a recognized color for a given feature
        /// (e.g. no branch of a conditional script matched) means "use the renderer's own,
        /// non-conditional <see cref="SimpleLabelRenderer.TextSymbol"/> color for this feature".
        /// </summary>
        public string ColorExpression
        {
            get => _colorExpression;
            set => _colorExpression = value ?? string.Empty;
        }

        protected override void HandleAdditionalLabelRendererBehavior(IDisplay display, IFeature feature)
        {
            // Nothing to do at all - not even an IFontColor check or a property touch - unless
            // this specific renderer actually has a color expression configured.
            if (string.IsNullOrEmpty(_colorExpression) || TextSymbol is not IFontColor fontColorSymbol)
            {
                return;
            }

            if (!ReferenceEquals(_baseFontColorSymbol, TextSymbol))
            {
                _baseFontColor = fontColorSymbol.FontColor;
                _baseFontColorSymbol = TextSymbol;
            }

            fontColorSymbol.FontColor = ResolveColor(feature) ?? _baseFontColor!.Value;
        }

        private ArgbColor? ResolveColor(IFeature feature)
        {
            var expr = _colorExpression;
            foreach (FieldValue fv in feature.Fields)
            {
                expr = expr.EvaluateExpression(fv);
            }

            if (SimpleScriptInterpreter.IsSimpleScript(expr))
            {
                expr = new SimpleScriptInterpreter(expr).Interpret();
            }

            return ArgbColor.TryFromString(expr, out var color) ? color : (ArgbColor?)null;
        }

        public override void PrepareQueryFilter(IDisplay display, IFeatureLayer layer, IQueryFilter filter)
        {
            base.PrepareQueryFilter(display, layer, filter);

            if (layer.FeatureClass == null || string.IsNullOrEmpty(_colorExpression))
            {
                return;
            }

            foreach (string fieldname in _colorExpression.ExtractFieldNames())
            {
                if (layer.FeatureClass.FindField(fieldname) != null)
                {
                    filter.AddField(fieldname);
                }
            }
        }

        #region IPersistable Members

        public override void Load(IPersistStream stream)
        {
            base.Load(stream);
            _colorExpression = (string)stream.Load("ColorExpression", string.Empty);
        }

        public override void Save(IPersistStream stream)
        {
            base.Save(stream);
            stream.Save("ColorExpression", _colorExpression);
        }

        #endregion

        #region IClone2 Members

        protected override SimpleLabelRenderer CreateCloneInstance(CloneOptions options)
            => new AdvancedLabelRenderer(
                (ITextSymbol)(TextSymbol is IClone2 ? TextSymbol.Clone(options) : null),
                FieldName);

        public override object Clone(CloneOptions options)
        {
            var renderer = (AdvancedLabelRenderer)base.Clone(options);
            renderer._colorExpression = _colorExpression;
            return renderer;
        }

        #endregion
    }
}
