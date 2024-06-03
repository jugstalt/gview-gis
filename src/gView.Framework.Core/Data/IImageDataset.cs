using gView.Framework.Core.Carto;

namespace gView.Framework.Core.Data
{
    public interface IImageDataset : IFeatureDataset
    {
        bool RenderImage(IDisplay display);
        System.Drawing.Image Bitmap { get; }
    }
}