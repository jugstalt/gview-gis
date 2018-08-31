using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.IO;
using System.Xml;
using gView.Framework.system;
using gView.MapServer;

namespace gView.Interoperability.ArcXML.Dataset
{
    internal class ArcIMSThemeFeatureClass : gView.Framework.XML.AXLFeatureClass
    {
        public ArcIMSThemeFeatureClass(IDataset dataset, string id)
            : base(dataset, id)
        {
            if (dataset is IFeatureDataset)
            {
                this.Envelope = ((IFeatureDataset)dataset).Envelope;
                this.SpatialReference = ((IFeatureDataset)dataset).SpatialReference;
            }
        }

        protected override string SendRequest(IUserData userData, string axlRequest)
        {
            if (!(_dataset is ArcIMSDataset)) return "";
            string server = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "service");
            string user = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "pwd");
            IServiceRequestContext context = (userData != null) ? userData.GetUserData("IServiceRequestContext") as IServiceRequestContext : null;

            if ((user == "#" || user == "$") &&
                    context != null && context.ServiceRequest != null && context.ServiceRequest.Identity != null)
            {
                string roles = String.Empty;
                if (user == "#" && context.ServiceRequest.Identity.UserRoles != null)
                {
                    foreach (string role in context.ServiceRequest.Identity.UserRoles)
                    {
                        if (String.IsNullOrEmpty(role)) continue;
                        roles += "|" + role;
                    }
                }
                user = context.ServiceRequest.Identity.UserName + roles;
                pwd = context.ServiceRequest.Identity.HashedPassword;
            }

            dotNETConnector connector = new dotNETConnector();
            if (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pwd))
                connector.setAuthentification(user, pwd);

            string resp = String.Empty;
            ArcIMSClass.Log(context, "GetFeature Request", server, service, axlRequest);
            try
            {
                resp = connector.SendRequest(axlRequest, server, service, "Query");
            }
            catch (Exception ex)
            {
                ArcIMSClass.ErrorLog(context, "Query", server, service, ex);
                return String.Empty;
            }
            ArcIMSClass.Log(context, "GetFeature Response", server, service, resp);

            return resp;
        }

        private XmlNode _originalRendererNode = null;
        internal XmlNode OriginalRendererNode
        {
            get { return _originalRendererNode; }
            set { _originalRendererNode = value; }
        }
    }

    internal class ArcIMSThemeClass : IClass
    {
        private IDataset _dataset;
        private string _name;

        public ArcIMSThemeClass(IDataset dataset, string name)
        {
            _dataset = dataset;
            _name = name;
        }

        #region IClass Member

        public string Name
        {
            get { return _name; }
        }

        public string Aliasname
        {
            get { return _name; }
        }

        public IDataset Dataset
        {
            get { return _dataset; }
        }

        #endregion
    }

    internal class ArcIMSThemeRasterClass : gView.Framework.XML.AXLQueryableRasterClass
    {
        public ArcIMSThemeRasterClass(IDataset dataset, string id)
            : base(dataset, id)
        {
        }

        protected override string SendRequest(IUserData userData, string axlRequest)
        {
            if (!(_dataset is ArcIMSDataset)) return "";
            string server = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "service");
            string user = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "user");
            string pwd = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "pwd");
            IServiceRequestContext context = (userData != null) ? userData.GetUserData("IServiceRequestContext") as IServiceRequestContext : null;

            if ((user == "#" || user == "$") &&
                    context != null && context.ServiceRequest != null && context.ServiceRequest.Identity != null)
            {
                string roles = String.Empty;
                if (user == "#" && context.ServiceRequest.Identity.UserRoles != null)
                {
                    foreach (string role in context.ServiceRequest.Identity.UserRoles)
                    {
                        if (String.IsNullOrEmpty(role)) continue;
                        roles += "|" + role;
                    }
                }
                user = context.ServiceRequest.Identity.UserName + roles;
                pwd = context.ServiceRequest.Identity.HashedPassword;
            }

            dotNETConnector connector = new dotNETConnector();
            if (!String.IsNullOrEmpty(user) || !String.IsNullOrEmpty(pwd))
                connector.setAuthentification(user, pwd);

            string resp = String.Empty;
            ArcIMSClass.Log(context, "GetRasterInfo Request", server, service, axlRequest);
            try
            {
                resp = connector.SendRequest(axlRequest, server, service);
            }
            catch (Exception ex)
            {
                ArcIMSClass.ErrorLog(context, "Query", server, service, ex);
                return String.Empty;
            }
            ArcIMSClass.Log(context, "GetRasterInfo Response", server, service, resp);

            return resp;
        }
    }

    //internal class ArcIMSThemeRasterClass_old : gView.Framework.XML.AXLRasterClass
    //{
    //    public ArcIMSThemeRasterClass_old(IDataset dataset, string id)
    //        : base(dataset, id)
    //    {
    //    }
    //}
}
