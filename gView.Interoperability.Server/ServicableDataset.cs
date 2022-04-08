using gView.Framework.Data;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace gView.Interoperability.Server
{
    //[gView.Framework.system.RegisterPlugIn("FD3FC1C5-1E10-41e3-8955-3441402950CC")]
    class ServicableDataset : IServiceableDataset
    {
        MapServerDataset _dataset = null;

        #region IServiceableDataset Member

        public string Name
        {
            get { return "gView.MapServer Service"; }
        }

        public string Provider
        {
            get { return "gView.GIS"; }
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
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.MapServer.Lib.UI.dll");

            IModalDialog dlg = uiAssembly.CreateInstance("gView.MapServer.Lib.UI.FormSelectService") as IModalDialog;
            if (dlg is IConnectionString)
            {
                if (dlg.OpenModal())
                {
                    string connectionString = ((IConnectionString)dlg).ConnectionString;

                    _dataset = new MapServerDataset();
                    _dataset.ConnectionString = connectionString;
                    return true;
                }
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
                _dataset = new MapServerDataset();
                _dataset.ConnectionString = connectionString;
            }
        }

        public void Save(gView.Framework.IO.IPersistStream stream)
        {
            if (_dataset == null)
            {
                return;
            }

            stream.Save("ConnectionString", _dataset.ConnectionString);
        }

        #endregion
    }
}
