using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.UI.Controls.Filter;
using gView.Framework.UI.Controls;
using System.Threading.Tasks;

namespace gView.Framework.UI.Dialogs
{
    public partial class ExplorerDialog : Form
    {
        private bool _open;

        public ExplorerDialog(string title,List<ExplorerDialogFilter> filters,bool open)
        {
            InitializeComponent();

            this.Text = title;
            explorerDialogControl1.IsOpenDialog = _open = open;
            explorerDialogControl1.Filters = filters;
            explorerDialogControl1.ItemSelected += new ExplorerDialogControl.ItemClickedEvent(explorerDialogControl1_ItemSelected);
            explorerDialogControl1.ElementTextStatusChanged += new ExplorerDialogControl.ElementTextStatusChangedEvent(explorerDialogControl1_ElementTextStatusChanged);
            btnOK.Enabled = false;

            using (Graphics graphics = this.CreateGraphics())
            {
                this.FontScaleFactor = graphics.DpiX / 96f;
            }
        }

        private float FontScaleFactor { get; set; }

        void explorerDialogControl1_ElementTextStatusChanged(TextBox tb)
        {
            if (!_open && tb != null)
            {
                btnOK.Enabled = (tb.Enabled && tb.Text != "");
            }
        }

        void explorerDialogControl1_ItemSelected(List<gView.Framework.UI.IExplorerObject> exObject)
        {
            if (_open)
            {
                btnOK.Enabled = (exObject != null && exObject.Count > 0);
            }
            else
            {
                btnOK.Enabled = (exObject.Count == 0);
            }
        }

        public List<IExplorerObject> ExplorerObjects
        {
            get
            {
                return explorerDialogControl1.ExplorerObjects;
            }
        }
        public string TargetName
        {
            get { return explorerDialogControl1.TargetName; }
        }
        public ExplorerDialogFilter SelectedExplorerDialogFilter
        {
            get { return explorerDialogControl1.SelectedExplorerDialogFilter; }
        }

        async public Task<List<IDataset>> Datasets()
        {
            List<IDataset> datasets = new List<IDataset>();
            List<IExplorerObject> usedObjects = new List<IExplorerObject>();

            foreach (IExplorerObject exObject in ExplorerObjects)
            {
                if (exObject == null) continue;
                if (usedObjects.Contains(exObject)) continue;

                var instance = await exObject.GetInstanceAsync();
                if (instance is IDataset)
                {
                    datasets.Add((IDataset)instance);
                    usedObjects.Add(exObject);
                }
                else if (instance is IFeatureClass && ((IFeatureClass)instance).Dataset is IDataset2)
                {
                    IDataset2 dataset = (IDataset2)((IFeatureClass)instance).Dataset;
                    IDataset2 datasetClone = await dataset.EmptyCopy();
                    if (datasetClone == null) continue;

                    foreach (IExplorerObject exObject2 in ExplorerObjects)
                    {
                        if (exObject2 == null) continue;

                        var instance2 = await exObject2.GetInstanceAsync();
                        if (instance2 is IFeatureClass && ((IFeatureClass)instance2).Dataset == dataset)
                        {
                            await datasetClone.AppendElement(((IFeatureClass)instance2).Name);
                            usedObjects.Add(exObject2);
                        }
                    }

                    datasets.Add(datasetClone);
                }
                else if (instance is IRasterClass && ((IRasterClass)instance).Dataset is IDataset2)
                {
                    IDataset2 dataset = (IDataset2)((IRasterClass)instance).Dataset;
                    IDataset2 datasetClone = await dataset.EmptyCopy();
                    if (datasetClone == null) continue;

                    foreach (IExplorerObject exObject2 in ExplorerObjects)
                    {
                        if (exObject2 == null) continue;

                        var instance2 = await exObject2.GetInstanceAsync();
                        if (instance2 is IRasterClass && ((IRasterClass)instance2).Dataset == dataset)
                        {
                            await datasetClone.AppendElement(((IRasterClass)instance2).Name);
                            usedObjects.Add(exObject2);
                        }
                    }

                    datasets.Add(datasetClone);
                }
                else if (instance is IFeatureClass && ((IFeatureClass)instance).Dataset is IDataset)
                {
                    datasets.Add(((IFeatureClass)instance).Dataset);
                }
                else if (instance is IWebServiceClass && ((IWebServiceClass)instance).Dataset is IDataset)
                {
                    datasets.Add(((IWebServiceClass)instance).Dataset);
                }
                else if (instance is IRasterClass && ((IRasterClass)instance).Dataset is IDataset)
                {
                    datasets.Add(((IRasterClass)instance).Dataset);
                }
            }

            return datasets;

        }

        public bool MulitSelection
        {
            get
            {
                return explorerDialogControl1.MultiSelection;
            }
            set
            {
                explorerDialogControl1.MultiSelection = value;
            }
        }
    }
}