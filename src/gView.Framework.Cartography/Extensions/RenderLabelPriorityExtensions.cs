using gView.Framework.Core.Carto;

namespace gView.Framework.Cartography.Extensions;

static public class RenderLabelPriorityExtensions
{
    extension(RenderLabelPriority labelPriority)
    {
        public int ToIntPriority()
            => labelPriority switch
            {
                RenderLabelPriority.Always => -1,
                RenderLabelPriority.High => 100,
                RenderLabelPriority.Normal => 0,
                RenderLabelPriority.Low => -100,
                _ => -1
            };

        static public RenderLabelPriority FromIntPriority(int priority)
            => priority switch
            {
                100 => RenderLabelPriority.High,
                0 => RenderLabelPriority.Normal,
                -100 => RenderLabelPriority.Low,
                _ => RenderLabelPriority.Always,
            };
    }
}
