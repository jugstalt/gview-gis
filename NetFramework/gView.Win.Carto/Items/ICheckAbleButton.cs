using gView.Framework.UI;

namespace gView.Win.Carto.Items
{
    public interface ICheckAbleButton
    {
        bool Checked { get; set; }
        ITool Tool { get; }
    }
}