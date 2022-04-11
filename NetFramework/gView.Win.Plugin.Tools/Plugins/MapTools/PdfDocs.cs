using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Plugins.MapTools
{
    [RegisterPlugInAttribute("6782B011-83C8-420B-9158-9267EB9C70D0")]
    public class PdfDocs : ITool, IExTool, IToolMenu
    {
        private List<ITool> _docTools = null;

        #region ITool Member

        public string Name
        {
            get { return "PDF Dokumentations"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (_docTools != null)
            {
                return;
            }

            _docTools = new List<ITool>();
            try
            {
                foreach (FileInfo fi in (new DirectoryInfo(SystemVariables.ApplicationDirectory + @"/doc").GetFiles("*.pdf")))
                {
                    _docTools.Add(new PdfDocTool(fi));
                }
            }
            catch { }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolMenu Member

        public List<ITool> DropDownTools
        {
            get { return _docTools; }
        }

        public ITool SelectedTool
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        #endregion

        #region HelperClasses
        private class PdfDocTool : ITool, IExTool
        {
            private FileInfo _fi;
            private string _name;

            public PdfDocTool(FileInfo fi)
            {
                _fi = fi;
                _name = _fi.Name.Substring(0, _fi.Name.Length - _fi.Extension.Length);
            }

            #region ITool Member

            public string Name
            {
                get { return _name; }
            }

            public bool Enabled
            {
                get { return true; }
            }

            public string ToolTip
            {
                get { return String.Empty; }
            }

            public ToolType toolType
            {
                get { return ToolType.command; }
            }

            public object Image
            {
                get { return gView.Win.Plugin.Tools.Properties.Resources.pdf; }
            }

            public void OnCreate(object hook)
            {

            }

            public Task<bool> OnEvent(object MapEvent)
            {
                try
                {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = _fi.FullName;
                    p.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                return Task.FromResult(true);
            }

            #endregion
        }
        #endregion
    }
}
