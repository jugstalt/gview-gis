using System.Windows.Forms;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class FormLineLabellingOrientation : Form
    {
        public FormLineLabellingOrientation(SimpleLabelRenderer renderer)
        {
            InitializeComponent();

            switch (renderer.CartoLineLabelling)
            {
                case SimpleLabelRenderer.CartographicLineLabeling.Parallel:
                    btnParallel.Checked = true;
                    break;
                case SimpleLabelRenderer.CartographicLineLabeling.Horizontal:
                    btnHorizontal.Checked = true;
                    break;
                case SimpleLabelRenderer.CartographicLineLabeling.Perpendicular:
                    btnPerpenticular.Checked = true;
                    break;
                case SimpleLabelRenderer.CartographicLineLabeling.CurvedText:
                    btnCurvedText.Checked = true;
                    break;
            }
        }

        public SimpleLabelRenderer.CartographicLineLabeling CartoLineLabelling
        {
            get
            {
                if (btnParallel.Checked == true)
                {
                    return SimpleLabelRenderer.CartographicLineLabeling.Parallel;
                }
                else if (btnHorizontal.Checked == true)
                {
                    return SimpleLabelRenderer.CartographicLineLabeling.Horizontal;
                }
                else if (btnPerpenticular.Checked == true)
                {
                    return SimpleLabelRenderer.CartographicLineLabeling.Perpendicular;
                }

                return SimpleLabelRenderer.CartographicLineLabeling.CurvedText;
            }
        }
    }
}
