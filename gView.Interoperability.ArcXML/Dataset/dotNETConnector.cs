using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using gView.Framework.Web;

namespace gView.Interoperability.ArcXML.Dataset
{
	/// <summary>
	/// Zusammenfassung für dotNETConnector.
	/// </summary>
	internal class dotNETConnector
	{
		private int m_timeout=5000;
        private char m_comma = ',';
        private string _user = "", _passwd = "";
        private Encoding _encoding = Encoding.UTF8;

		public dotNETConnector()
		{
			
		}

		public int timeout { get { return m_timeout/1000; } set { m_timeout=value*1000; } }

        //public string AuthentificationBase64 
        //{
        //    get { return m_authBase64; }
        //    set 
        //    {
        //        if(value==null) value="";
        //        m_authBase64=value; 
        //    }
        //}

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public void setAuthentification(string user, string passwd)
        {
            _user = user.Trim();
            _passwd = passwd.Trim();
            //if (user == "" && passwd == "")
            //    m_authBase64 = "";
            //else
            //    m_authBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + passwd));
        }
		public List<string> getServiceNames(string server)
		{
			return getServiceNames(server,false);
		}
		public List<string> getServiceNames(string server,bool onlyPublic)
		{
			string resp=SendRequest("<GETCLIENTSERVICES/>",server,"catalog");
			List<string> names=new List<string>();
			try 
			{
				XmlDocument doc=new XmlDocument();
				doc.LoadXml(resp);
				XmlNodeList services = doc.SelectNodes("//SERVICE");

				foreach(XmlNode service in services) 
				{
					if(onlyPublic) 
					{
						if(service.Attributes["access"].Value.ToString().ToLower()!="public") continue;
					}
					names.Add(service.Attributes["name"].Value);
				}
			} 
			catch(Exception ex) 
			{
				//log("ERROR@Connector.getServiceNames:\n"+ex.Message,"");
			}
			return names;
		}
		public string checkXML2(string xml)
		{
			int cursor=0;
			int pos=xml.IndexOf("=\"",cursor);
			while(pos!=-1) 
			{
				pos=pos+1;   // weil suche nach =" und nicht nach " -> pos zeigt auf =

				int pos2=xml.IndexOf("\"",pos+1);

				if(pos2==-1) break;
				string attr=xml.Substring(pos+1,pos2-pos-1);
					
				if(attr.IndexOf("<")!=-1 ||
					attr.IndexOf(">")!=-1 ||
					attr.IndexOf("&")!=-1 ||
					attr.IndexOf("\"")!=-1)
				{
					attr=attr.Replace("<","&lt;");
					attr=attr.Replace(">","&gt;");
						
					if(attr.IndexOf("&")!=attr.IndexOf("&amp;"))
						attr=attr.Replace("&","&amp;");

					attr=attr.Replace("\"","'");

					xml=xml.Substring(0,pos+1)+attr+xml.Substring(pos2,xml.Length-pos2);
				}
				pos=xml.IndexOf("=\"",pos+attr.Length+2);
			}
			return xml;
		}
		public string checkXML(string xml) 
		{
			if(xml==null) return null;

            if (xml.Contains("\u00c3\u201e")) xml = xml.Replace("\u00c3\u201e", "\u00C4");  // A umlaut
            if (xml.Contains("\u00c3\u2013")) xml = xml.Replace("\u00c3\u2013", "\u00D6");  // O umlaut
            if (xml.Contains("\u00c3\u0153")) xml = xml.Replace("\u00c3\u0153", "\u00DC");  // U umlaut
            if (xml.Contains("\u00c3\u00a4")) xml = xml.Replace("\u00c3\u00a4", "\u00E4");  // a umlaut 
            if (xml.Contains("\u00c3\u00b6")) xml = xml.Replace("\u00c3\u00b6", "\u00F6");  // o umlaut
            if (xml.Contains("\u00c3\u00bc")) xml = xml.Replace("\u00c3\u00bc", "\u00FC");  // u umlaut
            if (xml.Contains("\u00c3\u0178")) xml = xml.Replace("\u00c3\u0178", "\u00df");  // SZ
            if (xml.Contains("\u00c2\u00b2")) xml = xml.Replace("\u00c2\u00b2", "\u00b2");  // A umlaut

			XmlDocument doc=new XmlDocument();
			try 
			{
				doc.LoadXml(xml);
			} 
			catch 
			{
				int cursor=0;
				int pos=xml.IndexOf("=\"",cursor);
				while(pos!=-1) 
				{
					pos=pos+1;   // weil suche nach =" und nicht nach " -> pos zeigt auf =
					/*
					int pos2=-1;
					int p1=xml.IndexOf("\" ",pos+1);    // " ... 
					int p2=xml.IndexOf("\"/",pos+1);    // "/...
					int p3=xml.IndexOf("\"?",pos+1);    // "?...
					int p4=xml.IndexOf("\">",pos+1);    // ">...

					if((p1!=-1) || pos2==-1) pos2=p1;
					if((p2!=-1 && p2<pos2) || pos2==-1) pos2=p2;
					if((p3!=-1 && p3<pos2) || pos2==-1) pos2=p3;
					if((p4!=-1 && p4<pos2) || pos2==-1) pos2=p4;
					*/

					int pos2=xml.IndexOf("\"",pos+1);

					if(pos2==-1) break;
					string attr=xml.Substring(pos+1,pos2-pos-1);
					
					if(attr.IndexOf("<")!=-1 ||
						attr.IndexOf(">")!=-1 ||
						attr.IndexOf("&")!=-1 ||
					    attr.IndexOf("\"")!=-1)
					{
						attr=attr.Replace("<","&lt;");
						attr=attr.Replace(">","&gt;");
						
						if(attr.IndexOf("&")!=attr.IndexOf("&amp;"))
							attr=attr.Replace("&","&amp;");

						attr=attr.Replace("\"","'");

						xml=xml.Substring(0,pos+1)+attr+xml.Substring(pos2,xml.Length-pos2);
					}
					pos=xml.IndexOf("=\"",pos+attr.Length+2);
				}
			}
			return xml;
		}
		public string SendRequest(string axl,string ServerName,string ServiceName)
		{
			return checkXML(SendRequest(new StringBuilder(axl),ServerName,ServiceName,""));
		}
		public string SendRequest(string axl,string ServerName,string ServiceName,string CustomService)
		{
			return checkXML(SendRequest(new StringBuilder(axl),ServerName,ServiceName,CustomService));
		}
		public string SendRequest(StringBuilder sb,string ServerName,string ServiceName)
		{
			return checkXML(SendRequest(sb,ServerName,ServiceName,""));
		}
		public string SendRequest(StringBuilder sb,string ServerName,string ServiceName,string CustomService)
		{
			return checkXML(SendRequest_ServletExec(sb,ServerName,ServiceName,CustomService));
		}

        protected string ServerUrl(string ServerName)
        {
            if (ServerName.LastIndexOf("/") > 8) return "";
            return "servlet/com.esri.esrimap.Esrimap";
            //if (serverUrls == null) return "servlet/com.esri.esrimap.Esrimap";
            //if (!serverUrls.Contains(ServerName)) return "servlet/com.esri.esrimap.Esrimap";
            //return serverUrls[ServerName].ToString();
        }
		private string SendRequest_ServletExec(StringBuilder sb, string ServerName, string ServiceName, string CustomService) 
		{
			// 
			// Sollte beim GET_IMAGE Request auch das file Attribut im OUTPUT tag enthalten sein,
			// muss im esrimap_prop file (ServletExec/servlets/...) der Wert:
			// spatialServer.AllowResponsePath=True gesetzt werden (Standard ist false)
			// 

			string theURL=ServerName;
			if(ServerName.IndexOf("http")!=0) theURL="http://"+theURL;

			if(theURL.IndexOf("/",10)==-1)
				theURL+="/"+ServerUrl(ServerName);
			
			if(theURL.IndexOf("?")==-1) theURL+="?";
			if(theURL.IndexOf("?")!=(theURL.Length-1)) theURL+="&";

			//string theURL = "http://" + ServerName + "/"+ServerUrl(ServerName)+"?ServiceName=" + ServiceName;// + "&ClientVersion=4.0";

			if(ServiceName!="") 
				theURL+="ServiceName=" + ServiceName +"&ClientVersion=4.0";
			else
				theURL+="ClientVersion=4.0";

			if (CustomService != "") theURL += "&CustomService=" + CustomService;
			
            if(CommaFormat!=',') 
			{
				string temp=replaceComma(sb.ToString());
				sb=new StringBuilder();
				sb.Append(temp);
			}
            //System.Text.UTF8Encoding encoder=new UTF8Encoding();
			//byte [] POSTbytes=encoder.GetBytes(sb.ToString());
            byte[] POSTbytes = _encoding.GetBytes(sb.ToString());

            return WebFunctions.HttpSendRequest(theURL, "POST", POSTbytes, _user, _passwd, _encoding);
		}

		public char CommaFormat 
		{
			get { return m_comma; }
			set { m_comma=value; }
		}
		public string replaceComma(string axl) 
		{
			axl=replaceComma(axl,"minx",',',CommaFormat);
			axl=replaceComma(axl,"miny",',',CommaFormat);
			axl=replaceComma(axl,"maxx",',',CommaFormat);
			axl=replaceComma(axl,"maxy",',',CommaFormat);

			axl=replaceComma(axl,"x",',',CommaFormat);
			axl=replaceComma(axl,"y",',',CommaFormat);

			axl=replaceComma(axl,"width",',',CommaFormat);
			axl=replaceComma(axl,"fontsize",',',CommaFormat);

			axl=replaceComma(axl,"transparency",',',CommaFormat);
			axl=replaceComma(axl,"filltransparency",',',CommaFormat);

			return axl;
		}
		protected string replaceComma(string axl,string attribute,char from,char to) 
		{
			int pos=axl.IndexOf(attribute+"=\"");
			StringBuilder sb=new StringBuilder();
			sb.Append(axl);
			while(pos!=-1) 
			{
				int pos2=axl.IndexOf("\"",pos+attribute.Length+2);
				if(pos2==-1) return sb.ToString();
				int posC=0;
				while(true) 
				{
					posC=axl.IndexOf(from,(posC==0) ? pos+attribute.Length+2 : posC+1);
					if(posC!=-1 && posC<pos2)
					{
						sb[posC]=to;
					} 
					else 
					{
						break;
					}
				}
				pos=axl.IndexOf(attribute+"=\"",pos2);
			}
			return sb.ToString();
		}

		static public string getLayerID(string server,string service,string theme) 
		{
			string axl="<ARCXML version=\"1.1\"><REQUEST><GET_SERVICE_INFO fields=\"false\" envelope=\"false\" renderer=\"false\" extensions=\"false\" /></REQUEST></ARCXML>";
			dotNETConnector conn=new dotNETConnector();
			string resp=conn.SendRequest(axl,server,service);
			
			XmlDocument doc=new XmlDocument();
			doc.LoadXml(resp);
			foreach(XmlNode layer in doc.SelectNodes("//LAYERINFO")) 
			{
				if(layer.Attributes["name"]==null || layer.Attributes["id"]==null) continue;
				if(layer.Attributes["id"].Value==theme) return theme;

				if(layer.Attributes["name"].Value==theme) return layer.Attributes["id"].Value;
			}

			return "";
		}
	}
}
