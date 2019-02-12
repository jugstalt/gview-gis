using gView.MapServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    class MapService : IMapService
    {
        private string _filename = String.Empty,
                       _folder = String.Empty,
                       _name = String.Empty;
        private MapServiceType _type = MapServiceType.MXL;

        public MapService() { }
        public MapService(string filename, string folder, MapServiceType type)
        {
            _type = type;
            try
            {
                _filename = filename;
                FileInfo fi = new FileInfo(filename);
                _name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                _folder = folder;
            }
            catch { }
        }


        #region IMapService Member

        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public MapServiceType Type
        {
            get { return _type; }
        }

        public string Folder { get { return _folder; } }

        #endregion
    }
}
