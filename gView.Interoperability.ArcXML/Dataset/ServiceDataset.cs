using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.Reflection;
using gView.Framework.UI;

namespace gView.Interoperability.ArcXML.Dataset
{
    [gView.Framework.system.RegisterPlugIn("D7B6835C-2C42-42ca-8E10-8DEA6B03D8E5")]
    public class ServiceDataset : IServiceableDataset
    {
        ArcIMSDataset _dataset = null;

        #region IServiceableDataset Member

        public string Name
        {
            get { return "ArcIMS Service Dataset"; }
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
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Interoperability.ArcXML.UI.dll");

                IModalDialog dlg = uiAssembly.CreateInstance("gView.Interoperability.ArcXML.UI.FormSelectService") as IModalDialog;
                if (dlg is IConnectionString)
                {
                    if (dlg.OpenModal())
                    {
                        string connectionString = ((IConnectionString)dlg).ConnectionString;

                        _dataset = new ArcIMSDataset();
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
                _dataset = new ArcIMSDataset();
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
