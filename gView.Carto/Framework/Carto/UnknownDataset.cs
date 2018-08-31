using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.FDB;
using gView.Framework.IO;

namespace gView.Carto.Framework.Carto
{
    //[gView.Framework.system.RegisterPlugIn("B9D72B66-B716-4375-A01D-9386AC6235B8")]
    public class UnknownDataset : IDataset
    {
        public IDatasetElement this[string title]
        {
            get
            {
                return null;
            }
        }

        public string ConnectionString
        {
            get
            {
                return String.Empty;
            }

            set
            {
            }
        }

        public IDatabase Database
        {
            get
            {
                return null;
            }
        }

        public string DatasetGroupName
        {
            get
            {
                return "";
            }
        }

        public string DatasetName
        {
            get
            {
                return "Unkown Dataset";
            }
        }

        public List<IDatasetElement> Elements
        {
            get
            {
                return new List<IDatasetElement>();
            }
        }

        public string lastErrorMsg
        {
            get
            {
                return String.Empty;
            }
        }

        public string ProviderName
        {
            get
            {
                return String.Empty;
            }
        }

        public List<IMetadataProvider> Providers
        {
            get
            {
                return new List<IMetadataProvider>();
            }
        }

        public string Query_FieldPostfix
        {
            get
            {
                return String.Empty;
            }
        }

        public string Query_FieldPrefix
        {
            get
            {
                return String.Empty;
            }
        }

        public DatasetState State
        {
            get
            {
                return DatasetState.unknown;
            }
        }

        public void Dispose()
        {
            
        }

        public IMetadataProvider MetadataProvider(Guid guid)
        {
            return null;
        }

        public bool Open()
        {
            return true;
        }

        public void ReadMetadata(IPersistStream stream)
        {
           
        }

        public void RefreshClasses()
        {
            
        }

        public void WriteMetadata(IPersistStream stream)
        {
           
        }
    }
}
