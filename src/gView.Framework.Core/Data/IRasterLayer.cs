using gView.GraphicsEngine;
using gView.GraphicsEngine.Filters;

namespace gView.Framework.Core.Data
{
    public interface IRasterLayer : ILayer
    {
        InterpolationMethod InterpolationMethod { get; set; }
        float Opacity { get; set; }
        ArgbColor TransparentColor { get; set; }
        FilterImplementations FilterImplementation { get; set; }

        IRasterClass RasterClass { get; }
    }
}