using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using gView.Framework.system;

namespace gView.Framework.system
{
    public class Acl
    {
        private FileInfo _fi;
        private XmlDocument _doc = null;
        private DateTime _lastWriteTime = DateTime.Now;

        public Acl(FileInfo fi)
        {
            _fi = fi;
            ReloadXmlDocument();
        }

        public void ReloadXmlDocument()
        {
            try
            {
                _fi.Refresh();
                if (!_fi.Exists)
                {
                    _doc = null;
                    return;
                }

                if (_doc == null || _fi.LastWriteTimeUtc != _lastWriteTime)
                {
                    _doc = new XmlDocument();
                    _doc.Load(_fi.FullName);
                    _lastWriteTime = _fi.LastWriteTimeUtc;
                }
            }
            catch /*(Exception ex)*/
            {
                _doc = null;
            }
        }

        public bool HasAccess(IIdentity identity, string password, string service)
        {
            if (identity == null) return false;

            //ReloadXmlDocument(); // wird immer aufgerufen, wenn Service = "catalog" siehe MapServerConnector
            if (_doc == null || String.IsNullOrEmpty(service)) return true;

            if (service.Contains(","))
            {
                foreach (string queryService in service.Split(','))
                {
                    if (!HasAccess(identity, password, queryService))
                        return false;
                }

                return true;
            }

            foreach (XmlNode userNode in _doc.SelectNodes("ACL/USER[@name]"))
            {
                if (userNode.Attributes["services"] == null) continue;

                string userName = userNode.Attributes["name"].Value;
                WildcardEx wex = new WildcardEx(userName);

                if (wex.IsMatch(identity.UserName))
                {
                    if (password == null ||
                        userNode.Attributes["password"] == null ||
                        (userNode.Attributes["password"] != null &&
                          (userNode.Attributes["password"].Value == password ||
                           Identity.HashPassword(userNode.Attributes["password"].Value) == password)))
                    {
                        foreach (string s in userNode.Attributes["services"].Value.Split(','))
                        {
                            wex = new WildcardEx(s);
                            if (wex.IsMatch(service))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            if (identity.UserRoles != null && identity.UserRoles.Count > 0)
            {
                foreach (XmlNode roleNode in _doc.SelectNodes("ACL/ROLE[@name]"))
                {
                    if (roleNode.Attributes["services"] == null) continue;

                    string roleName = roleNode.Attributes["name"].Value;
                    WildcardEx wex = new WildcardEx(roleName);

                    foreach (string idRole in identity.UserRoles)
                    {
                        if (wex.IsMatch(idRole))
                        {
                            if (password == null ||
                                roleNode.Attributes["password"] == null ||
                                (roleNode.Attributes["password"] != null &&
                                   (roleNode.Attributes["password"].Value == password ||
                                    Identity.HashPassword(roleNode.Attributes["password"].Value) == password)))
                            {
                                foreach (string s in roleNode.Attributes["services"].Value.Split(','))
                                {
                                    wex = new WildcardEx(s);
                                    if (wex.IsMatch(service))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
