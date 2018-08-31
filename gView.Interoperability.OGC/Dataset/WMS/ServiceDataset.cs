using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.UI;
using System.Reflection;
using gView.Framework.IO;

namespace gView.Interoperability.OGC.Dataset.WMS
{
    [gView.Framework.system.RegisterPlugIn("386D6A7E-FA7E-4b99-A5F7-DFAB16E4516D")]
    class ServiceDataset : IServiceableDataset
    {
        WMSDataset _dataset = null;

        #region IServiceableDataset Member

        public string Name
        {
            get { return "OGC Service Dataset"; }
        }

        public string Provider
        {
            get { return "gView GIS"; }
        }

        public List<IDataset> Datasets
        {
            get
            {
                List<IDataset> datasets = new List<IDataset>();
                datasets.Add(_dataset);
                return datasets;
            }
        }

        public bool GenerateNew()
        {
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Interoperability.OGC.UI.dll");

                IModalDialog dlg = uiAssembly.CreateInstance("gView.Interoperability.OGC.UI.Dataset.WMS.FormNewConnection") as IModalDialog;
                if (dlg is IConnectionString)
                {
                    if (dlg.OpenModal())
                    {
                        string connectionString = ((IConnectionString)dlg).ConnectionString;

                        _dataset = new WMSDataset();
                        _dataset.ConnectionString = connectionString;
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        #endregion

        #region IPersistable Member

        public void Load(gView.Framework.IO.IPersistStream stream)
        {
            _dataset = null;
            string connectionString = (string)stream.Load("ConnectionString", String.Empty);

            if (connectionString != String.Empty)
            {
                _dataset = new WMSDataset();
                _dataset.ConnectionString = connectionString;
            }
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            if (_dataset == null) return;

            stream.Save("ConnectionString", _dataset.ConnectionString);
        }

        #endregion
    }
}
