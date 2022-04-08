//using WeifenLuo.WinFormsUI.Docking;

//namespace gView.Framework.system.UI
//{
//    public partial class ToolWindow : DockContent
//    {
//        private IDockableWindow _window;

//        public ToolWindow(IDockableWindow window)
//        {
//            InitializeComponent();

//            _window = window;
//            if (window is Form)
//            {
//                List<Control> controls = new List<Control>();
//                foreach (Control control in ((Form)window).Controls)
//                {
//                    controls.Add(control);
//                }
//                ((Form)window).Controls.Clear();
//                foreach (Control control in controls)
//                {
//                    this.Controls.Add(control);
//                }
//            }
//        }
//    }
//}