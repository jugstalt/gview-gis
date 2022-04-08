using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Plugins.Editor.Dialogs
{
    internal partial class FormAttributeEditor : UserControl, IDockableToolWindow, IErrorMessage
    {
        private Module _module;
        private IFeatureClass _fc = null;
        private IFeature _feature = null;
        private string _errMsg = String.Empty;

        public FormAttributeEditor(Module module)
        {
            InitializeComponent();

            this.Name = "Attribute Editor";
            attributeControl.Module = module;
            attributeControl.Dock = DockStyle.Fill;

            _module = module;

            if (_module == null)
            {
                return;
            }

            _module.OnChangeSelectedFeature += new Module.OnChangeSelectedFeatureEventHandler(Module_OnChangeSelectedFeature);
            _module.OnCreateStandardFeature += new Module.OnCreateStandardFeatureEventHandler(Module_OnChangeSelectedFeature);
        }

        #region IDockableWindow Member

        DockWindowState _state = DockWindowState.none;
        public DockWindowState DockableWindowState
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public Image Image
        {
            get { return global::gView.Win.Plugins.Editor.Properties.Resources.application_form_edit; }
        }

        #endregion

        private void Module_OnChangeSelectedFeature(Module sender, IFeature feature)
        {
            if (sender == null || sender.SelectedEditLayer == null ||
                sender.SelectedEditLayer.FeatureLayer == null)
            {
                _fc = null;
            }
            else
            {
                _fc = sender.SelectedEditLayer.FeatureLayer.FeatureClass;
            }
            _feature = feature;

            MakeGUI();
        }

        private void MakeGUI()
        {
            if (_module == null)
            {
                return;
            }

            attributeControl.RefreshGUI();
        }

        internal bool CommitValues()
        {
            if (!attributeControl.CommitValues())
            {
                _errMsg = attributeControl.LastErrorMessage;
                return false;
            }

            return true;
        }

        #region IErrorMessage Member

        public string LastErrorMessage
        {
            get { return _errMsg; }
            set { _errMsg = value; }
        }

        #endregion
    }
}