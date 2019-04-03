using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gView.Framework.Geometry;
using gView.Framework.system.UI;

namespace gView.Framework.UI
{
    [gView.Framework.system.RegisterPlugIn("D65EA70D-2FBB-45e0-A253-1F679C22B991")]
    public class PrjExplorerObject : ExplorerObjectCls, IExplorerFileObject
    {
        private string _filename;
        private ISpatialReference _sRef;
        private PrjIcon _icon = new PrjIcon();

        public PrjExplorerObject() : base(null, typeof(SpatialReference), 2) { }
        public PrjExplorerObject(IExplorerObject parent, string filename)
            : base(parent, typeof(SpatialReference), 2)
        {
            _filename = filename;
        }
        #region IExplorerFileObject Member

        public string Filter
        {
            get { return "*.prj|*.wkt"; }
        }

        public IExplorerFileObject CreateInstance(IExplorerObject parent, string filename)
        {
            return new PrjExplorerObject(parent, filename);
        }

        #endregion

        #region IExplorerObject Member

        public string Name
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(_filename);
                    return fi.Name;
                }
                catch
                {
                    return "???.prj";
                }
            }
        }

        public string FullName
        {
            get { return _filename; }
        }

        public string Type
        {
            get { return "Projection file"; }
        }

        public IExplorerIcon Icon
        {
            get { return _icon; }
        }

        public new object Object
        {
            get
            {
                if (_sRef != null)
                    return null;

                try
                {
                    StreamReader sr = new StreamReader(_filename);
                    string wkt = sr.ReadToEnd();
                    sr.Close();

                    _sRef = SpatialReference.FromWKT(wkt);
                }
                catch
                {
                    _sRef = null;
                }
                return _sRef;
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion

        #region ISerializableExplorerObject Member

        public IExplorerObject CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
        {
            return null;
        }

        #endregion
    }

    internal class PrjIcon : IExplorerIcon
    {
        #region IExplorerIcon Member

        public Guid GUID
        {
            get { return new Guid("548BB839-1FED-4041-8866-0BFF6812C4BB"); }
        }

        public System.Drawing.Image Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.proj; }
        }

        #endregion
    }
}
