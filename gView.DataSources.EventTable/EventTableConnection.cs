using gView.Framework.Db;
using gView.Framework.Geometry;
using gView.Framework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.EventTable
{
    public class EventTableConnection : IPersistable, IXmlString
    {
        private DbConnectionString _dbConn = null;
        private string _table, _xField, _yField, _idField;
        private ISpatialReference _sRef;

        public EventTableConnection() { }
        public EventTableConnection(
            DbConnectionString dbConn,
            string table,
            string idField, string xField, string yField,
            ISpatialReference sRef)
        {
            _dbConn = dbConn;
            _table = table;
            _xField = xField;
            _yField = yField;
            _idField = idField;
            _sRef = sRef;
        }

        public DbConnectionString DbConnectionString
        {
            get { return _dbConn; }
        }
        public string TableName
        {
            get { return _table; }
        }
        public string IdFieldName
        {
            get { return _idField; }
        }
        public string XFieldName
        {
            get { return _xField; }
        }
        public string YFieldName
        {
            get { return _yField; }
        }
        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }

        #region IPersistable Member

        public Task<bool> Load(IPersistStream stream)
        {
            _dbConn = new DbConnectionString();
            _dbConn.FromString((string)stream.Load("dbconn", String.Empty));
            _table = (string)stream.Load("table", String.Empty);
            _idField = (string)stream.Load("idfield", String.Empty);
            _xField = (string)stream.Load("xfield", String.Empty);
            _yField = (string)stream.Load("yfield", String.Empty);


            string sr = (string)stream.Load("sref", String.Empty);
            if (String.IsNullOrEmpty(sr))
                _sRef = null;
            else
            {
                _sRef = new SpatialReference();
                _sRef.FromXmlString(sr);
            }

            return Task.FromResult(true);
        }

        public Task<bool> Save(IPersistStream stream)
        {
            stream.Save("dbconn", _dbConn.ToString());
            stream.Save("table", _table);
            stream.Save("idfield", _idField);
            stream.Save("xfield", _xField);
            stream.Save("yfield", _yField);
            if (_sRef != null)
                stream.Save("sref", _sRef.ToXmlString());

            return Task.FromResult(true);
        }

        #endregion

        #region IXmlString Member

        public string ToXmlString()
        {
            XmlStream xmlStream = new XmlStream("EventTableConnection");
            this.Save(xmlStream);

            return xmlStream.ToString();
        }

        public void FromXmlString(string xml)
        {
            XmlStream stream = new XmlStream("EventTableConnection");
            StringReader sr = new StringReader(xml);
            stream.ReadStream(sr);

            Load(stream);
        }

        #endregion
    }
}
