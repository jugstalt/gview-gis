using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using gView.Framework.UI;
using gView.Framework.system;

namespace gView.DataSources.Fdb.UI.MSAccess
{
    /*
    public class AccessFDBCommands : IExplorerCommand
    {
        #region IExplorerCommand Members

        public System.Xml.XmlNodeList CommandDefs
        {
            get
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SystemVariables.MyApplicationConfigPath + @"\explorer\accessfdb_commands.xml");

                    return doc.SelectNodes("//ExplorerCommand[@category='Directory']");
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
                return KnownExplorerObjectIDs.Directory;
            }
        }

        #endregion
    }
    */
    /*
    public class AccessFDBCommands2 : IExplorerCommand
    {
        #region IExplorerCommand Members

        public System.Xml.XmlNodeList CommandDefs
        {
            get
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SystemVariables.ApplicationDirectory + @"\config\explorer\accessfdb_commands.xml");

                    return doc.SelectNodes("//ExplorerCommand[@category='AccessFDB']");
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
                AccessFDBExplorerObject obj = new AccessFDBExplorerObject();
                return obj.GUID;
            }
        }

        #endregion
    }

    public class AccessFDBCommands3 : IExplorerCommand
    {
        #region IExplorerCommand Members

        public System.Xml.XmlNodeList CommandDefs
        {
            get
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SystemVariables.ApplicationDirectory + @"\config\explorer\accessfdb_commands.xml");

                    return doc.SelectNodes("//ExplorerCommand[@category='AccessFDB Dataset']");
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
                AccessFDBDatasetExplorerObject obj = new AccessFDBDatasetExplorerObject();
                return obj.GUID;
            }
        }

        #endregion
    }
     * */
}
