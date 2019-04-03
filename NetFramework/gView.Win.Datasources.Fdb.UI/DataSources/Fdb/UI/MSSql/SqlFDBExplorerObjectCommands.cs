using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using gView.Framework.UI;
using gView.Framework.system;

namespace gView.DataSources.Fdb.UI.MSSql
{
    /*
    public class SqlFDBGroupCommands : IExplorerCommand
    {
        System.Guid _exObjGUID;
        public SqlFDBGroupCommands()
        {
            _exObjGUID = new SqlFDBExplorerGroupObject().GUID;
        }
        #region IExplorerCommand Members

        public System.Xml.XmlNodeList CommandDefs
        {
            get
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SystemVariables.MyApplicationConfigPath + @"\explorer\sqlfdb_commands.xml");

                    return doc.SelectNodes("//ExplorerCommand[@group='GroupObject']");
                }
                catch
                {
                    return null;
                }
            }
        }

        public Guid ExplorerObjectGUID
        {
            get
            {   
                return _exObjGUID; 
            }
        }

        #endregion
    }

    public class SqlFDBConnectionCommands : IExplorerCommand
    {
        System.Guid _exObjGUID;
        public SqlFDBConnectionCommands()
        {
            _exObjGUID = new SqlFDBExplorerObject("","").GUID;
        }
        #region IExplorerCommand Members

        public XmlNodeList CommandDefs
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(SystemVariables.MyApplicationConfigPath + @"\explorer\sqlfdb_commands.xml");

                return doc.SelectNodes("//ExplorerCommand[@group='Connection']");
            }
        }

        public Guid ExplorerObjectGUID
        {
            get { return _exObjGUID; }
        }
    }
     * */
}
