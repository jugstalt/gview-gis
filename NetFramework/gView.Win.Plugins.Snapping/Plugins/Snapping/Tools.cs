using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using System.Windows.Forms;
using gView.Framework.Snapping.Core;
using gView.Framework.IO;
using gView.Framework.Globalisation;
using System.Threading.Tasks;

namespace gView.Plugins.Snapping
{
    [gView.Framework.system.RegisterPlugIn("8C5AD6C8-8991-447c-9313-5E0FAC6EA2BB", Obsolete = true)]
    public class SchemaText : ITool, IToolItem
    {
        #region ITool Members

        public string Name
        {
            get { return "Snapping.Schema.Text"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
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

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Members

        public ToolStripItem ToolItem
        {
            get
            {
                return new ToolStripLabel(LocalizedResources.GetResString("String.SnappingSchema", "Snapping Schema:"));
            }

        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("9CDE6BD1-317E-478b-8828-B169A6688CC5", Obsolete = true)]
    public class SchemaCombo : ITool, IToolItem, IPersistable, IToolItemLabel
    {
        private IMapDocument _doc = null;
        private ToolStripComboBox _combo = null;
        private Module _module = null;
        private ISnapSchema _activeSnapSchema = null;

        public SchemaCombo()
        {
            _combo = new ToolStripComboBox();
            _combo.DropDownStyle = ComboBoxStyle.DropDownList;
            _combo.SelectedIndexChanged += new EventHandler(combo_SelectedIndexChanged);
        }

        #region ITool Members

        public string Name
        {
            get { return "Snapping.Schema.Combo"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
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
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                if (_doc.Application is IMapApplication)
                    _module = ((IMapApplication)_doc.Application).IMapApplicationModule(Globals.ModuleGuid) as Module;

                RefreshGUI();
            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Members

        public ToolStripItem ToolItem
        {
            get
            {
                return _combo;
            }
        }

        #endregion

        async void combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_combo.SelectedItem is SnapSchemaItem)
                _activeSnapSchema = ((SnapSchemaItem)_combo.SelectedItem).SnapSchema;
            else
                _activeSnapSchema = null;

            if (_module != null && _module.MapDocument != null &&
                _module.MapDocument.FocusMap != null && _module.MapDocument.FocusMap.Display != null)
                await _module.LoadGeometry(
                    _module.MapDocument.FocusMap,
                    _activeSnapSchema,
                    _module.MapDocument.FocusMap.Display.Envelope);
        }

        public ISnapSchema ActiveSnapSchema
        {
            get
            {
                return _activeSnapSchema;
            }
            set
            {
                foreach (SnapSchemaItem item in _combo.Items)
                {
                    if (item.SnapSchema == value)
                    {
                        _combo.SelectedItem = item;
                        _activeSnapSchema = value;
                        break;
                    }
                }
            }
        }

        private delegate void RefreshGUICallback();
        public void RefreshGUI()
        {
            if (_combo.Owner == null) return;
            if (_combo.Owner.InvokeRequired)
            {
                RefreshGUICallback d = new RefreshGUICallback(RefreshGUI);
                _combo.Owner.BeginInvoke(d);
                return;
            }
            string activeName = String.Empty;

            if (_combo.SelectedItem != null)
                activeName = _combo.SelectedItem.ToString();

            _combo.Items.Clear();
            _combo.Items.Add(new SnapSchemaItem(null));

            if (_module != null && _module.MapDocument != null &&
                _module.MapDocument.FocusMap != null && _module[_module.MapDocument.FocusMap] != null)
            {
                foreach (SnapSchema schema in _module[_module.MapDocument.FocusMap])
                {
                    if (schema == null) continue;
                    _combo.Items.Add(new SnapSchemaItem(schema));
                    if (!String.IsNullOrEmpty(activeName) &&
                        activeName == schema.Name)
                        _combo.SelectedIndex = _combo.Items.Count - 1;
                }
            }

            if (_persistedSelIndex >= -0 && _persistedSelIndex < _combo.Items.Count)
                _combo.SelectedIndex = _persistedSelIndex;
            if (_combo.SelectedIndex == -1)
                _combo.SelectedIndex = 0;
            _persistedSelIndex = -1;

            using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(1, 1))
            {
                using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                {
                    _combo.DropDownWidth = _combo.Size.Width;
                    foreach (object obj in _combo.Items)
                    {
                        System.Drawing.SizeF size = gr.MeasureString(obj.ToString(), _combo.Font);

                        if (size.Width + 20 > _combo.DropDownWidth)
                            _combo.DropDownWidth = (int)size.Width + 20;
                    }
                }
            }
        }

        #region ItemClasses
        public class SnapSchemaItem
        {
            private ISnapSchema _schema;

            public SnapSchemaItem(ISnapSchema schema)
            {
                _schema = schema;
            }

            public ISnapSchema SnapSchema
            {
                get { return _schema; }
            }

            public override string ToString()
            {
                if (_schema != null) 
                    return _schema.Name;

                return "None";
            }
        }
        #endregion

        #region IPersistable Member

        private int _persistedSelIndex = -1;
        public void Load(IPersistStream stream)
        {
            _persistedSelIndex = _combo.SelectedIndex = (int)stream.Load("selectedIndex", 0);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("selectedIndex", _combo.SelectedIndex);
        }

        #endregion

        #region IToolItemLabel Member

        public string Label
        {
            get { return LocalizedResources.GetResString("String.SnappingSchema", "Snapping Schema:"); }
        }

        public ToolItemLabelPosition LabelPosition
        {
            get { return ToolItemLabelPosition.top; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("35BCECD1-393F-443f-B15D-DC1DC6AD9564", Obsolete = true)]
    public class ToleranceText : ITool, IToolItem
    {
        #region ITool Members

        public string Name
        {
            get { return "Snapping.Schema.Text"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
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

        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Members

        public ToolStripItem ToolItem
        {
            get
            {
                return new ToolStripLabel("Tolerance:");
            }

        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("D678C377-79A0-49e5-88A3-6635FD7B522C", Obsolete = true)]
    public class ToleranceCombo : ITool, IToolItem, IPersistable
    {
        private IMapDocument _doc = null;
        private ToolStripComboBox _combo = null;
        private Module _module = null;

        public ToleranceCombo()
        {
            _combo = new ToolStripComboBox();
            for (int i = 1; i < 100; i++)
                _combo.Items.Add(new PixToleranceItem(i));
            _combo.SelectedIndexChanged += new EventHandler(_combo_SelectedIndexChanged);
            _combo.DropDownStyle = ComboBoxStyle.DropDownList;
            _combo.Size = new System.Drawing.Size(30, 25);
        }

        #region ITool Member

        public string Name
        {
            get { return "Snapping.Tolerance.Combo"; }
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
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                if (_doc.Application is IMapApplication)
                {
                    _module = ((IMapApplication)_doc.Application).IMapApplicationModule(Globals.ModuleGuid) as Module;
                    if (_module != null)
                    {
                        foreach (PixToleranceItem item in _combo.Items)
                        {
                            if (item.Value == _module.SnapTolerance)
                            {
                                _combo.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }

            }
        }

        public Task<bool> OnEvent(object MapEvent)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get { return _combo; }
        }

        #endregion

        void _combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_module != null)
            {
                _module.SnapTolerance = ((PixToleranceItem)_combo.SelectedItem).Value;
            }
        }

        #region ItemClasses
        private class PixToleranceItem
        {
            int _pixTolerance;
            public PixToleranceItem(int pixTolerance)
            {
                _pixTolerance = pixTolerance;
            }

            public int Value
            {
                get
                {
                    return _pixTolerance;
                }
            }

            public override string ToString()
            {
                return "Tol: " + _pixTolerance.ToString() + "px";
            }
        }
        #endregion

        #region IPersistable Member

        //int _persistedSnapTolerance = -1;
        public void Load(IPersistStream stream)
        {
            //_persistedSnapTolerance = (int)stream.Load("SnapTol", -1);
            if (_module != null)
            {
                _module.SnapTolerance = (int)stream.Load("SnapTol", _module.SnapTolerance);
                foreach (PixToleranceItem item in _combo.Items)
                {
                    if (item.Value == _module.SnapTolerance)
                    {
                        _combo.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_module == null) return;

            stream.Save("SnapTol", _module.SnapTolerance);
        }

        #endregion
    }
}
