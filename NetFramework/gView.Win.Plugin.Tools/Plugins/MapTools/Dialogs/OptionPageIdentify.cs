using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Globalisation;
using gView.Framework.system;

namespace gView.Plugins.MapTools.Dialogs
{
    internal partial class OptionPageIdentify : Form
    {
        IMapDocument _doc;
        QueryThemes _queries = null;

        public OptionPageIdentify(IMapDocument doc, Identify identify)
        {
            InitializeComponent();

            if (identify == null) return;

            if (identify.toolType == ToolType.click)
                btnMethodClick.Checked = true;
            else if (identify.toolType == ToolType.rubberband)
                btnMethodRubberband.Checked = true;

            if (identify.UserDefinedQueries != null)
                _queries = identify.UserDefinedQueries.Clone() as QueryThemes;

            numTolerance.Value = (decimal)identify.Tolerance;

            if (identify.ThemeMode == QueryThemeMode.Default)
                btnDefault.Checked = true;
            else if (identify.ThemeMode == QueryThemeMode.Custom)
                btnCustom.Checked = true;

            _doc = doc;
            btnQueryThemeEditor.Enabled = _doc != null;
        }

        public double Tolerance
        {
            get
            {
                return (double)numTolerance.Value;
            }
        }

        public ToolType IdentifyToolType
        {
            get
            {
                if (btnMethodClick.Checked) return ToolType.click;
                return ToolType.rubberband;
            }
        }

        public QueryThemeMode QueryThemeMode
        {
            get
            {
                if (btnDefault.Checked) return QueryThemeMode.Default;
                return QueryThemeMode.Custom;
            }
        }
        public QueryThemes Queries
        {
            get { return _queries; }
        }

        private void btnQueryThemeEditor_Click(object sender, EventArgs e)
        {
            FormQueryThemeEditor editor = new FormQueryThemeEditor(_doc);

            editor.QueryThemes = _queries;
            if (editor.ShowDialog() == DialogResult.OK)
            {
                _queries = editor.QueryThemes;
            }
        }
    }

    [gView.Framework.system.RegisterPlugIn("EBF278B7-53FB-45f7-9DCA-B2902B2882A9")]
    public class MapOptionPageIdentify : IMapOptionPage
    {
        OptionPageIdentify _dlg;
        IMapDocument _doc;

        public MapOptionPageIdentify()
        {
        }

        public MapOptionPageIdentify(IMapDocument document)
        {
            _doc = document;
        }

        private Identify ToolIdentify
        {
            get
            {
                if (_doc == null || !(_doc.Application is IGUIApplication)) return null;

                IGUIApplication app = _doc.Application as IGUIApplication;
                return app.Tool(new Guid("F13D5923-70C8-4c6b-9372-0760D3A8C08C")) as Identify;
            }
        }

        public void ShowDialog()
        {
            Panel panel = OptionPage(_doc);
            if (panel == null || _dlg == null)
                return;

            if (_dlg.ShowDialog() == DialogResult.OK)
                this.Commit();
        }

        #region IMapOptionPage Member

        public Panel OptionPage(IMapDocument document)
        {
            _doc=document;

            if (!IsAvailable(document))
                return null;

            IGUIApplication app = document.Application as IGUIApplication;
            Identify identify = ToolIdentify;
            if(identify==null) return null;

            _dlg = new OptionPageIdentify(_doc, identify);
            return _dlg.OptionPanel;
        }

        public string Title
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Identify", "Identify") + "/" +
                       LocalizedResources.GetResString("Tools.Find", "Query");
            }
        }

        public Image Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.info; }
        }

        public void Commit()
        {
            Identify identify = ToolIdentify;
            if (identify == null || _dlg == null) return;

            identify.Tolerance = _dlg.Tolerance;
            identify.toolType = _dlg.IdentifyToolType;

            identify.UserDefinedQueries = _dlg.Queries;

            identify.ThemeMode = _dlg.QueryThemeMode;
        }

        public bool IsAvailable(IMapDocument document)
        {
            if (document == null || document.Application == null) return false;

            if (document.Application is IMapApplication &&
                ((IMapApplication)document.Application).ReadOnly == true) return false;

            return true;
        }

        #endregion
    }
}