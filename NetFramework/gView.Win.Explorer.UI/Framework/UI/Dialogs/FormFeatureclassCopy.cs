using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.Geometry;
using gView.Framework.FDB;
using gView.Framework.system;
using System.Threading.Tasks;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormFeatureclassCopy : Form
    {
        private List<IFeatureClass> _fcs = null;
        private IDataset _destDataset = null;
        private List<IFileFeatureDatabase> _destDatabases = null;
        private int _maxFieldLength = 255;
        private string _directory=String.Empty;
        private bool _schemaOnly = false;

        private FormFeatureclassCopy()
        {
            InitializeComponent();
        }

        public FormFeatureclassCopy(List<IFeatureClass> featureClasses,IDataset destDataset)
            : this()
        {
            _fcs = featureClasses;
            _destDataset = destDataset;
        }

        async static public Task<FormFeatureclassCopy> Create(List<IExplorerObject> exObjects, IDataset destDataset)
        {
            var dlg = new FormFeatureclassCopy();

            if (exObjects == null)
                return dlg;
            dlg._fcs = new List<IFeatureClass>();

            foreach (IExplorerObject exObject in exObjects)
            {
                var instanace = await exObject?.GetInstanceAsync();
                if (instanace is IFeatureClass)
                {
                    dlg._fcs.Add((IFeatureClass)instanace);
                }
                else if (instanace is IFeatureDataset)
                {
                    List<IDatasetElement> elements = await (((IFeatureDataset)instanace).Elements());
                    if (elements == null) continue;

                    foreach (IDatasetElement element in elements)
                    {
                        if (element != null && element.Class is IFeatureClass)
                        {
                            dlg._fcs.Add(element.Class as IFeatureClass);
                        }
                    }
                }
            }
            dlg._destDataset = destDataset;

            return dlg;
        }

        public FormFeatureclassCopy(List<IFeatureClass> featureClasses, List<IFileFeatureDatabase> destDatabases, string directory)
            : this()
        {
            _fcs = featureClasses;
            _destDatabases = destDatabases;
            _directory = directory;
        }

        async static public Task<FormFeatureclassCopy> Create(List<IExplorerObject> exObjects, List<IFileFeatureDatabase> destDatabases, string directory)
        {
            var dlg = new FormFeatureclassCopy();

            dlg._destDatabases = destDatabases;
            dlg._directory = directory;

            if (exObjects == null)
                return dlg;
            dlg._fcs = new List<IFeatureClass>();

            foreach (IExplorerObject exObject in exObjects)
            {
                var instance = await exObject?.GetInstanceAsync();
                if (instance is IFeatureClass)
                {
                    dlg._fcs.Add((IFeatureClass)instance);
                }
                else if (instance is IFeatureDataset)
                {
                    List<IDatasetElement> elements = await (((IFeatureDataset)instance).Elements());
                    if (elements == null) continue;

                    foreach (IDatasetElement element in elements)
                    {
                        if (element != null && element.Class is IFeatureClass)
                        {
                            dlg._fcs.Add(element.Class as IFeatureClass);
                        }
                    }
                }
            }

            return dlg;
        }

        public bool SchemaOnly
        {
            get { return _schemaOnly; }
            set { _schemaOnly = value; }
        }

        public int MaxFieldLength
        {
            get { return _maxFieldLength; }
            set { _maxFieldLength = value; }
        }

        private void FormFeatureclassCopy_Load(object sender, EventArgs e)
        {
            if (_fcs == null) return;

            if (SchemaOnly)
            {
                btnScript.Visible = false;
                this.Text = "Copy Featureclass Schema(s)";
            }

            if (_destDatabases == null)
            {
                foreach (IFeatureClass fc in _fcs)
                {
                    FeatureClassListViewItem item = new FeatureClassListViewItem(fc, _maxFieldLength);
                    switch (fc.GeometryType)
                    {
                        case geometryType.Point:
                        case geometryType.Multipoint:
                            item.ImageIndex = 0;
                            break;
                        case geometryType.Polyline:
                            item.ImageIndex = 1;
                            break;
                        default:
                            item.ImageIndex = 2;
                            break;
                    }

                    item.Checked = true;
                    lstFeatureclasses.Items.Add(item);
                }
                if (lstFeatureclasses.Items.Count > 0)
                    lstFeatureclasses.Items[0].Selected = true;
            }
            else
            {
                panelFormat.Visible = true;
                foreach (IFileFeatureDatabase database in _destDatabases)
                {
                    if (database == null) continue;

                    List<FeatureClassListViewItem> items = new List<FeatureClassListViewItem>();
                    foreach (IFeatureClass fc in _fcs)
                    {
                        FeatureClassListViewItem item = new FeatureClassListViewItem(fc, database.MaxFieldNameLength);
                        switch (fc.GeometryType)
                        {
                            case geometryType.Point:
                            case geometryType.Multipoint:
                                item.ImageIndex = 0;
                                break;
                            case geometryType.Polyline:
                                item.ImageIndex = 1;
                                break;
                            default:
                                item.ImageIndex = 2;
                                break;
                        }

                        item.Checked = true;
                        items.Add(item);
                    }

                    cmbDestFormat.Items.Add(new FileDatabaseItem(database, items));
                }
                if (cmbDestFormat.Items.Count > 0)
                    cmbDestFormat.SelectedIndex = 0;

                //btnScript.Visible = false;
            }
        }

        private void lstFeatureclasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvFields.DataSource = null;
            if (_fcs == null) return;
            if (lstFeatureclasses.SelectedItems.Count == 0 || lstFeatureclasses.SelectedIndices[0] < 0 || lstFeatureclasses.SelectedIndices[0] >= _fcs.Count) return;

            FeatureClassListViewItem item = lstFeatureclasses.SelectedItems[0] as FeatureClassListViewItem;
            gvFields.DataSource = item.Fields;
            txtTargetFeatureclass.Text = item.TargetName;
        }

        private void cmbDestFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstFeatureclasses.Items.Clear();
            if (!(cmbDestFormat.SelectedItem is FileDatabaseItem)) return;

            FileDatabaseItem dsItem = (FileDatabaseItem)cmbDestFormat.SelectedItem;
            foreach (FeatureClassListViewItem item in dsItem.ListViewItems)
            {
                lstFeatureclasses.Items.Add(item);
            }
            if (lstFeatureclasses.Items.Count > 0)
                lstFeatureclasses.Items[0].Selected = true;
        }

        async private void btnScript_Click(object sender, EventArgs e)
        {
            if (_fcs.Count == 0) return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Explort Script...";
            dlg.Filter = "BATCH File(*.bat)|*.bat";

            string destConnStr = String.Empty;
            Guid destGUID = new Guid();

            if (cmbDestFormat.Visible == true && cmbDestFormat.SelectedItem is FileDatabaseItem)
            {
                destConnStr = _directory;
                destGUID = PlugInManager.PlugInID(((FileDatabaseItem)cmbDestFormat.SelectedItem).FileFeatureDatabase);
            }
            else if (_destDataset != null)
            {
                if (_destDataset is IDataset2)
                {
                    IDataset2 ds2 = await ((IDataset2)_destDataset).EmptyCopy();
                    if (ds2 != null)
                    {
                        destConnStr = ds2.ConnectionString;
                        ds2.Dispose();
                    }
                }
                else
                {
                    destConnStr = _destDataset.ConnectionString;
                }
                destGUID = PlugInManager.PlugInID(_destDataset);
            }
            else
            {
                return;
            }
            
            if (dlg.ShowDialog() == DialogResult.OK)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("echo off\r\n");

                //foreach (IFeatureClass fc in _fcs)
                foreach (FeatureClassListViewItem item in FeatureClassItems)
                {
                    if (!item.Checked || item.FeatureClass == null) continue;
                    IDataset ds = item.FeatureClass.Dataset;
                    string connStr = ds.ConnectionString;
                    if (item.FeatureClass.Dataset is IDataset2)
                    {
                        IDataset2 ds2 = await ((IDataset2)item.FeatureClass.Dataset).EmptyCopy();
                        await ds2.AppendElement(item.FeatureClass.Name);
                        connStr = ds2.ConnectionString;
                        ds2.Dispose();
                    }
                    if (ds == null)
                    {
                        sb.Append("rem FeatureClass " + item.FeatureClass.Name + " has no dataset...\r\n");
                        continue;
                    }

                    FieldTranslation ftrans = item.ImportFieldTranslation;
                    StringBuilder sb1 = new StringBuilder();
                    StringBuilder sb2 = new StringBuilder();
                    foreach (IField field in ftrans.SourceFields.ToEnumerable())
                    {
                        if (field.type == FieldType.ID || field.type == FieldType.Shape) continue;

                        if (sb1.Length != 0) sb1.Append(";");
                        sb1.Append(field.name);
                    }
                    foreach (IField field in ftrans.DestinationFields.ToEnumerable())
                    {
                        if (field.type == FieldType.ID || field.type == FieldType.Shape) continue;

                        if (sb2.Length != 0) sb2.Append(";");
                        sb2.Append(field.name);
                    }
                    sb.Append("\"%GVIEW4_HOME%\\gView.Cmd.CopyFeatureclass\" -source_connstr \"" + connStr + "\" -source_guid \"" + PlugInManager.PlugInID(ds) + "\" -source_fc \"" + item.FeatureClass.Name + "\" ");
                    sb.Append("-dest_connstr \"" + destConnStr + "\" -dest_guid \"" + destGUID + "\" -dest_fc \"" + item.FeatureClass.Name + "\" ");
                    sb.Append("-sourcefields \"" + sb1.ToString() + "\" -destfields \"" + sb2.ToString() + "\"");
                    sb.Append("\r\n");
                }

                StreamWriter sw = new StreamWriter(dlg.FileName, false);
                if (!String2DOS(sw.BaseStream, sb.ToString()))
                {
                    MessageBox.Show("Warning: Can't find encoding codepage (ibm850)...");
                    sw.WriteLine(sb.ToString());
                }
                sw.Close();
            }
        }

        private bool String2DOS(Stream stream, string str)
        {
            Encoding encoding = null;
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage == 850)   // ibm850...Westeuropäisch(DOS)
                {
                    encoding = ei.GetEncoding();
                }
            }

            if (encoding == null) return false;

            byte[] bytes = encoding.GetBytes(str);
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(bytes);

            return true;
        }

        public List<FeatureClassListViewItem> FeatureClassItems
        {
            get
            {
                List<FeatureClassListViewItem> items = new List<FeatureClassListViewItem>();
                foreach (FeatureClassListViewItem item in lstFeatureclasses.CheckedItems)
                {
                    items.Add(item);
                }
                return items;
            }
        }

        private void txtTargetFeatureclass_TextChanged(object sender, EventArgs e)
        {
            if (lstFeatureclasses.SelectedItems.Count == 0) return;

            FeatureClassListViewItem item = lstFeatureclasses.SelectedItems[0] as FeatureClassListViewItem;
            if (item == null) return;
            item.TargetName = txtTargetFeatureclass.Text;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private class FileDatabaseItem
        {
            private IFileFeatureDatabase _db;
            List<FeatureClassListViewItem> _items;

            public FileDatabaseItem(IFileFeatureDatabase db, List<FeatureClassListViewItem> items)
            {
                _db = db;
                _items = items;
            }

            public IFileFeatureDatabase FileFeatureDatabase
            {
                get { return _db; }
            }

            public List<FeatureClassListViewItem> ListViewItems
            {
                get { return _items; }
            }
            public override string ToString()
            {
                if (_db != null)
                    return _db.DatabaseName;

                return "";
            }
        }

        public IFileFeatureDatabase SelectedFeatureDatabase
        {
            get
            {
                if (cmbDestFormat.SelectedItem is FileDatabaseItem)
                {
                    return ((FileDatabaseItem)cmbDestFormat.SelectedItem).FileFeatureDatabase;
                }
                return null;
            }
        }
    }

    public class FeatureClassListViewItem : ListViewItem
    {
        private IFeatureClass _fc;
        private DataTable _fields;
        private string _targetName = "";
        private int _maxFieldLength = 255;
        private FieldType _shapeFieldType = FieldType.Shape;

        public FeatureClassListViewItem(IFeatureClass fc, int maxFieldLength)
        {
            _maxFieldLength = (maxFieldLength != 0) ? maxFieldLength : _maxFieldLength;

            _fc = fc;
            _fields = new DataTable();
            _fields.Columns.Add(" ", typeof(bool)).DefaultValue = true;
            _fields.Columns.Add("Source Name", typeof(string)).DefaultValue = "";
            _fields.Columns.Add("Destination Name", typeof(string)).DefaultValue = "";
            _fields.Columns[2].MaxLength = _maxFieldLength;

            _fields.Columns.Add("Aliasname", typeof(string)).DefaultValue = "";

            if (_fc == null || _fc.Fields == null) return;
            this.Text = fc.Name;

            foreach (IField field in _fc.Fields.ToEnumerable())
            {
                DataRow row = _fields.NewRow();
                row["Source Name"] = field.name;
                row["Destination Name"] = FieldTranslation.CheckName(field.name);
                row["Destination Name"] = FieldTranslation.CheckNameLength(_fc.Fields, field, row["Destination Name"].ToString(), _maxFieldLength);
                row["Aliasname"] = field.aliasname;
                _fields.Rows.Add(row);
            }
            _targetName = _fc.Name;
        }

        public IFeatureClass FeatureClass
        {
            get { return _fc; }
        }

        public DataTable Fields
        {
            get { return _fields; }
        }

        public FieldTranslation ImportFieldTranslation
        {
            get
            {
                if (_fc == null || _fc.Fields==null) return null;
                FieldTranslation ftrans = new FieldTranslation();
                
                foreach (DataRow row in _fields.Rows)
                {
                    if ((bool)row[0] == false) continue;
                    IField field = _fc.FindField(row["Source Name"].ToString());
                    if (field == null) continue;

                    Field f = new Field(field);
                    f.aliasname = row["Aliasname"].ToString();

                    ftrans.Add(f, row["Destination Name"].ToString());
                }
                return ftrans;
            }
        }

        public string TargetName
        {
            get { return _targetName; }
            set { _targetName = value; }
        }

        public FieldType ShapeFieldType
        {
            get { return _shapeFieldType; }
            set { _shapeFieldType = value; }
        }

        public int MaxFieldLength
        {
            get { return _maxFieldLength; }
        }
    }
}