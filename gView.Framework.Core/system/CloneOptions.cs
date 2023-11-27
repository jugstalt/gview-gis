using gView.Framework.Core.Carto;
using System;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.system
{
    public class CloneOptions
    {
        public CloneOptions(IDisplay display,
                            bool applyRefScale,
                            float maxRefScaleFactor = 0f,
                            float maxLabelRefscaleFactor = 0f)
        {
            Display = display;
            ApplyRefScale = applyRefScale;

            DpiFactor = display == null || display.Dpi == 96D ?
                1f :
                (float)Math.Pow(display.Dpi / 96.0, 1.0);

            MaxRefScaleFactor = maxRefScaleFactor <= float.Epsilon ? float.MaxValue : maxRefScaleFactor;
            MaxLabelRefScaleFactor = maxLabelRefscaleFactor <= float.Epsilon ? float.MaxValue : maxLabelRefscaleFactor;
        }
        public IDisplay Display { get; private set; }

        public float MaxRefScaleFactor { get; private set; }
        public float MaxLabelRefScaleFactor { get; private set; }

        public float DpiFactor { get; private set; }

        public bool ApplyRefScale { get; private set; }

        public float RefScaleFactor(float factor)
        {
            return Math.Min(factor, MaxRefScaleFactor);
        }
        public float LabelRefScaleFactor(float factor)
        {
            return Math.Min(factor, MaxLabelRefScaleFactor);
        }
    }
}
