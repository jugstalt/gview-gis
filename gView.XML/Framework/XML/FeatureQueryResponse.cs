using System;
using System.Collections;
using System.Xml;
using System.Text;
using System.Data;
using gView.Framework;
using gView.Framework.Geometry;

namespace gView.Framework.XML
{
	internal enum featureQueryMethode 
	{
		Geometry,Query,ID,None,Buffer
	}
	/// <summary>
	/// Stellt die Ergebnisse einer Feature-Abfrage dar. Die Klasse ServiceManager stellt
	/// mehrere Instanzen die Klasse in einem Array bereit, wobei eine Instanz 
	/// die Ergebnisse einer Selektion, eine zweite Instanz die Ergebnisse aus Identifyabfragen
	/// enthalten kann.
	/// </summary>
	internal class FeatureQueryResponse
	{
		/// <summary>
		/// Die Geometrie der Feature-Abfrage
		/// </summary>
		public selectionGeometry selGeometry;
		protected featureQueryMethode m_bufferMethode;
		protected featureQueryMethode m_highlightMethod,m_selectionMethode,m_queryMethode;
		protected bool m_selectFeatures;
		protected string m_queryResponseAXL="";
		protected double m_bufferDist;
		protected int m_highlightID;
		protected int m_bufferID;
		protected bool m_dispBuffer;
		protected string m_selQuery="", m_appendWhereFilter="";
		protected ArrayList m_sortedFeatures;
		protected string m_bufferIDs="",m_queryName;
		protected int m_beginrecord=1;
		protected int m_sourceID;  // für Bufferung (was waren die ursprünglichen pufferFeatures
		protected int m_maxQueryResults=2;
		protected int m_maxQueryResultsFactor=1;
		protected int m_featureCount,m_lastQueryMaximum;
		protected bool m_hasmore=false;
		protected ArrayList m_joinIDs=new ArrayList(),m_domainsReplaced=new ArrayList();
		protected bool m_queryGeometry=false;
		protected AXLLayer m_layer;

		public FeatureQueryResponse()
		{
			highlightMethode=selectionMethode=featureQueryMethode.None;
			bufferMethode=featureQueryMethode.None;
			selectFeatures=false;
			selGeometry=new selectionGeometry();
			selGeometry.createNew();
			bufferDist=30.0;
			highlightID=bufferID=-1;
			dispBuffer=false;
			m_sortedFeatures=new ArrayList();
			queryMethode=featureQueryMethode.None;
		}

		public int maxQueryResults { get { return m_maxQueryResults; } set { m_maxQueryResults=m_lastQueryMaximum=value; } }
		public int maxQueryResultsFactor { get { return m_maxQueryResultsFactor; } set { m_maxQueryResultsFactor=value; } }
		public string queryName { get { return m_queryName; } set { m_queryName=value; } }
		public string bufferIDs { get { return m_bufferIDs; } set { m_bufferIDs=value; } }
		public ArrayList sortedFeatures { get { return m_sortedFeatures; } }
		public AXLLayer layer { get { return m_layer; } set { m_layer=value; } }
		public int sourceID { get { return m_sourceID; } set { m_sourceID=value; } }
		public bool queryGeometry { get { return m_queryGeometry; } set { m_queryGeometry=value; } }

		public string appendWhereFilter 
		{
			get { return m_appendWhereFilter; }
			set { m_appendWhereFilter=value; }
		}
		public int beginrecord 
		{ 
			get 
			{ 
				return m_beginrecord; 
			} 
			set 
			{ 
				m_beginrecord=value; 
				if(m_beginrecord<1) m_beginrecord=1;
			} 
		}
		public featureQueryMethode highlightMethode 
		{ 
			get 
			{ 
				return m_highlightMethod; 
			} 
			set 
			{ 
				m_highlightMethod=value; 
			} 
		}
		public featureQueryMethode selectionMethode 
		{ 
			get 
			{ 
				return m_selectionMethode; 
			} 
			set 
			{ 
				m_selectionMethode=value; 
			} 
		}
		public featureQueryMethode queryMethode 
		{ 
			get 
			{ 
				return m_queryMethode; 
			} 
			set 
			{ 
				m_queryMethode=value; 
				beginrecord=1;
				m_lastQueryMaximum=maxQueryResults;
			} 
		}
		public featureQueryMethode bufferMethode { get { return m_bufferMethode; } set { m_bufferMethode=value; } }
		public bool selectFeatures { get { return m_selectFeatures; } set { m_selectFeatures=value; } }
		public string queryResponseAXL 
		{
			get 
			{
				return m_queryResponseAXL;
			}
			set 
			{
				m_featureCount=0;
				try 
				{	
					XmlDocument doc=new XmlDocument();
					doc.LoadXml(value);

					XmlNodeList node=doc.GetElementsByTagName("FEATURECOUNT");
					if(node.Count>0) 
					{
						m_featureCount=Convert.ToInt32(node[0].Attributes["count"].Value);
						m_hasmore =Convert.ToBoolean(node[0].Attributes["hasmore"].Value);
					}
				} 
				catch 
				{
				}
				finally 
				{
					m_queryResponseAXL=value;
				}
				m_joinIDs=new ArrayList();
				m_domainsReplaced=new ArrayList();
				//Join(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source =C:\ARCIMS\database\fuerstenfeld\rminfoPASS.mdb; Jet OLEDB:Database Password=PASS",
				//	 @"oledb",
				//	 "SELECT RMD_GDB_Eigentuemer.Zaehler, RMD_GDB_Eigentuemer.Nenner, RMD_GDB_Eigentuemer.Vorname, RMD_GDB_Eigentuemer.Nachname FROM RMD_GDB_Grundstuecke INNER JOIN (RMD_GDB_Eigentuemer INNER JOIN RMD_GDB_Einlagezahlen ON RMD_GDB_Eigentuemer.ID_Einlagezahl = RMD_GDB_Einlagezahlen.ID_Einlagezahl) ON RMD_GDB_Grundstuecke.ID_Einlagezahl = RMD_GDB_Einlagezahlen.ID_Einlagezahl WHERE (((RMD_GDB_Grundstuecke.KG)=\"[KG]\") AND ((RMD_GDB_Grundstuecke.BauFlPkt)=\" \") AND ((RMD_GDB_Grundstuecke.StammNr)=[S]) AND ((RMD_GDB_Grundstuecke.UnterteilungsNr)=[u]));");

			}
		}
		public double bufferDist { get { return m_bufferDist; } set { m_bufferDist=value; } }
		public int highlightID { get { return m_highlightID; } set { m_highlightID=value; } }
		public int bufferID { get { return m_bufferID; } set { m_bufferID=value; } }
		public bool dispBuffer { get { return m_dispBuffer; } set { m_dispBuffer=value; } }
		public string selQuery { get { return m_selQuery; } set { m_selQuery=value; } }

		public void appendFeatures(string newFeatures)
		{
			if(queryResponseAXL==null || queryResponseAXL=="") 
			{
				queryResponseAXL=newFeatures;
				return;
			}
			try 
			{
				XmlDocument oldxml=new XmlDocument(),newxml=new XmlDocument();
				oldxml.LoadXml(queryResponseAXL);
				newxml.LoadXml(newFeatures);

				XmlNodeList newfeat=newxml.SelectNodes("//FEATURES");
				XmlNodeList featList=oldxml.SelectNodes("//FEATURES");

				// Features anhängen
				//foreach(XmlNode feat in newfeat)
				//	featList[0].AppendChild(feat);

				featList[0].InnerXml+=newfeat[0].InnerXml;

				//FeatureCount anpassen...
				newfeat=oldxml.SelectNodes("//FEATURE");
				XmlNodeList node1=oldxml.GetElementsByTagName("FEATURECOUNT");
				node1[0].Attributes["count"].Value=newfeat.Count.ToString();
				XmlNodeList node2=newxml.GetElementsByTagName("FEATURECOUNT");
				node1[0].Attributes["hasmore"].Value=node2[0].Attributes["hasmore"].Value;

				queryResponseAXL=oldxml.OuterXml;
			} 
			catch {}
		}
		public int featureCount 
		{
			get 
			{
				return m_featureCount;	
			}
		}
		public bool hasmore
		{
			get
			{
				return m_hasmore;
			}
		}
		public int lastQueryMaximum { get { return m_lastQueryMaximum; } set { m_lastQueryMaximum=value; } }

		protected string getGeometryFromAXL(XmlNode feature)
		{
			foreach(XmlNode child in feature.ChildNodes) 
			{
				if(child.Name=="POLYGON" ||
					child.Name=="POLYLINE" ||
					child.Name=="POINT" ||
					child.Name=="MULTIPOINT") 
				{
					return child.OuterXml;
				}
			}
			return "";
		}

		public string getAttributeString(int index,string attributes,int colwidth)
		{
			try 
			{
				XmlDocument doc=new XmlDocument();
				doc.LoadXml(queryResponseAXL);

				XmlNodeList features=doc.SelectNodes("//FEATURE");
				if(index<0 || index>=features.Count) return "";

				XmlDocument featDoc=new XmlDocument();
				featDoc.LoadXml(features[index].OuterXml);

				XmlNodeList fields=featDoc.SelectNodes("//FIELD");
				string [] attr=attributes.Split(';');

				StringBuilder sb=new StringBuilder();
				foreach(string a in attr) 
				{
					foreach(XmlNode field in fields) 
					{
						string name=field.Attributes["name"].Value;
						if(a==name) 
						{
							sb.Append(field.Attributes["value"].Value.PadLeft(colwidth,' '));
						}
					}
				}
				return sb.ToString();
			} 
			catch { return ""; }
		}
		public string getTomTomAttributeString(int index,string attributes) 
		{
			return getTomTomAttributeString(index,attributes,-1);
		}
		public string getTomTomAttributeString(int index,string attributes,int max) 
		{
			try 
			{
				XmlDocument doc=new XmlDocument();
				doc.LoadXml(queryResponseAXL);

				XmlNodeList features=doc.SelectNodes("//FEATURE");
				if(index<0 || index>=features.Count) return "\"\"";

				XmlDocument featDoc=new XmlDocument();
				featDoc.LoadXml(features[index].OuterXml);

				XmlNodeList fields=featDoc.SelectNodes("//FIELD");
				string [] attr=attributes.Split(';');

				StringBuilder sb=new StringBuilder();
				int counter=0;
				foreach(string a in attr) 
				{
					if(max>0 && max>=counter) break;
					foreach(XmlNode field in fields) 
					{
						string name=field.Attributes["name"].Value;
						if(a==name) 
						{
							if(sb.Length!=0) sb.Append(" ");
							sb.Append(field.Attributes["value"].Value.Replace(" ","-"));
						}
					}
					counter++;
				}
				return "\""+sb.ToString()+"\"";
			} 
			catch { return "\"\""; }	
		}
		public string getGeometryFromAXL()
		{
			return getGeometryFromAXL(-1);
		}
		public string getGeometryFromAXL(int index) 
		{
			try 
			{
				XmlDocument doc=new XmlDocument();
				doc.LoadXml(queryResponseAXL);

				XmlNodeList features=doc.SelectNodes("//FEATURE");
				if(index>=0 && index<features.Count)
					return "<GEOMETRY>"+getGeometryFromAXL(features[index])+"</GEOMETRY>";

				StringBuilder sb=new StringBuilder();
				foreach(XmlNode feature in features) 
				{
					sb.Append("<GEOMETRY>"+getGeometryFromAXL(feature)+"</GEOMETRY>");
				}
				return sb.ToString();
			} 
			catch { return ""; }
		}
		public string getAttributeList(string attname) 
		{
			if(queryResponseAXL==null) return "";
			if(queryResponseAXL=="") return "";

			StringBuilder sb=new StringBuilder();
			try 
			{	
				XmlDocument doc=new XmlDocument();
				doc.LoadXml(queryResponseAXL);

				XmlNodeList features=doc.SelectNodes("//FEATURE");
				foreach(XmlNode feature in features) 
				{
					XmlNodeList fields=feature.SelectNodes("FIELDS/FIELD");
					foreach(XmlNode field in fields) 
					{
						string name=field.Attributes["name"].Value;
						if(name==attname) 
						{
							if(sb.Length==0) 
							{
								sb.Append(field.Attributes["value"].Value);
							} 
							else 
							{
								sb.Append(";"+field.Attributes["value"].Value);
							}
						}
					}
				}
			} 
			catch { return ""; }
			return sb.ToString();
		}
	}

	/*
	public enum geometryType
	{
		Envelope,Point,Line,Polygon,userdef
	}
	*/
	internal class selectionGeometry
	{
		protected geometryType m_geomType;
		protected ArrayList m_points;
		protected string m_axl;
		double m_bufferDist;
		public selectionGeometry() 
		{
			m_points=new ArrayList();
			m_bufferDist=30.0;
		}
		public void createNew() 
		{
			m_points.Clear();
		}
		public geometryType GeometryType { get { return m_geomType; } set { m_geomType=value; } }
		public double bufferDist { get { return m_bufferDist; } set { m_bufferDist=value; } }
		public void addPoint(xyPoint point)
		{
			m_points.Add(point);
		}
		public void removeLastPoint() 
		{
			if(m_points.Count==0) return;
			m_points.RemoveAt(m_points.Count-1);
		}
		public void addGeomFromCoords(string points) 
		{
			string [] coords=points.Split(';');
			for(int i=0;i<coords.Length;i+=2) 
			{
				try 
				{
					xyPoint p=new xyPoint(Convert.ToDouble(coords[i].Replace(".",",")),
						Convert.ToDouble(coords[i+1].Replace(".",",")));
					addPoint(p);
				} 
				catch {}
			}
		}
		public bool isValid 
		{
			get 
			{
				if(GeometryType==geometryType.Point && m_points.Count>0) return true;
				if(GeometryType==geometryType.Polyline && m_points.Count>1) return true;
				if(GeometryType==geometryType.Polygon && m_points.Count>2) return true;
				if(GeometryType==geometryType.Envelope && m_points.Count==2) return true;
				if(GeometryType==geometryType.Unknown) return true;
				return false;
			}
				
		}
		public string userdef_axl 
		{
			set 
			{
				m_axl=value; 
				GeometryType=geometryType.Unknown;
			}
		}
		private void createBufferAXL(ref XmlTextWriter xml) 
		{
			xml.WriteStartElement("BUFFER");
			xml.WriteAttributeString("distance",m_bufferDist.ToString());
			xml.WriteAttributeString("bufferunits","meters");
			xml.WriteEndElement(); // BUFFER
		}
		public void createAXL(ref XmlTextWriter xml) 
		{
			if(!isValid) return;
			switch(GeometryType) 
			{
				case geometryType.Envelope:
					xyPoint p1=(xyPoint)m_points[0],
						p2=(xyPoint)m_points[1];
					double minx=Math.Min(p1.x,p2.x),
						miny=Math.Min(p1.y,p2.y),
						maxx=Math.Max(p1.x,p2.x),
						maxy=Math.Max(p1.y,p2.y);
					xml.WriteStartElement("ENVELOPE");
					xml.WriteAttributeString("minx",minx.ToString());
					xml.WriteAttributeString("miny",miny.ToString());
					xml.WriteAttributeString("maxx",maxx.ToString());
					xml.WriteAttributeString("maxy",maxy.ToString());
					xml.WriteEndElement(); // Enveolpe
					break;
				case geometryType.Point:
					if(m_bufferDist>1e-7) createBufferAXL(ref xml);
					xml.WriteStartElement("MULTIPOINT");
					foreach(xyPoint p in m_points) 
					{
						axlShapes.Point(ref xml,p);
					}
					xml.WriteEndElement(); // MULTIPOINT
					break;
				case geometryType.Polyline:
					if(m_bufferDist>1e-7) createBufferAXL(ref xml);
					xml.WriteStartElement("POLYLINE");
					xml.WriteStartElement("PATH");
					foreach(xyPoint p in m_points) 
					{
						axlShapes.Point(ref xml,p);
					}
					xml.WriteEndElement(); // PATH
					xml.WriteEndElement(); // POLYLINE
					break;
				case geometryType.Polygon:
					if(m_bufferDist>1e-7) createBufferAXL(ref xml);
					xml.WriteStartElement("POLYGON");
					xml.WriteStartElement("RING");
					foreach(xyPoint p in m_points) 
					{
						axlShapes.Point(ref xml,p);
					}
					xml.WriteEndElement(); // RING
					xml.WriteEndElement(); // POLYGON
					break;
				case geometryType.Unknown:
					if(m_bufferDist>1e-7) createBufferAXL(ref xml);
					xml.WriteRaw(m_axl);
					break;
			}
		}
		public void createBufferACETATE(ref XmlTextWriter xml)
		{
			if(m_bufferDist<1e-7) return;
			if(m_points.Count==0) return;
			if(GeometryType==geometryType.Envelope) return;

			xml.WriteStartElement("LAYER");
			xml.WriteAttributeString("type","acetate");
			xml.WriteAttributeString("id","selectionGeometryBuffer");
			xml.WriteAttributeString("name","selectionGeometryBuffer");

			axlShapes.openObject(ref xml);
			axlShapes.PolygonSymbol(ref xml,"200,200,200","0,0,0",1.0,0.5);
			//xml.WriteStartElement("SPATIALQUERY");
			xml.WriteStartElement("BUFFER");
			xml.WriteStartElement("distance",m_bufferDist.ToString());
			xml.WriteEndElement(); // BUFFER;
			//xml.WriteStartElement("SPATIALFILTER");

			xml.WriteStartElement("MULTIPOINT");
			foreach(xyPoint p in m_points) 
			{
				axlShapes.Point(ref xml,p);
			}
			xml.WriteEndElement(); // MULTIPOINT
			axlShapes.closeObject(ref xml);

			//xml.WriteEndElement(); // SPATIALFILTER
			//xml.WriteEndElement(); // SPATIALQUERY;
			xml.WriteEndElement(); // LAYER
		}
		public void createACETATE(ref XmlTextWriter xml) 
		{
			if(m_points.Count==0) return;
			if(GeometryType==geometryType.Envelope) return;

			xml.WriteStartElement("LAYER");
			xml.WriteAttributeString("type","acetate");
			xml.WriteAttributeString("id","selectionGeometry");
			xml.WriteAttributeString("name","selectionGeometry");
			// Punkte Zeichnen
			axlShapes.openObject(ref xml);
			axlShapes.PointSymbol(ref xml,5.0,0.6,"255,0,0");
			xml.WriteStartElement("MULTIPOINT");
			foreach(xyPoint p in m_points) 
			{
				axlShapes.Point(ref xml,p);
			}
			xml.WriteEndElement(); // MULTIPOINT
			axlShapes.closeObject(ref xml);
			if((GeometryType==geometryType.Polyline && isValid) ||
				(GeometryType==geometryType.Polygon && m_points.Count==2)) 
			{
				axlShapes.openObject(ref xml);
				axlShapes.LineSymbol(ref xml,3.0,"255,0,0");
				xml.WriteStartElement("POLYLINE");
				xml.WriteStartElement("PATH");
				foreach(xyPoint p in m_points) 
				{
					axlShapes.Point(ref xml,p);
				}
				xml.WriteEndElement(); // PATH
				xml.WriteEndElement(); // POLYLINE
				axlShapes.closeObject(ref xml);
			}
			if(GeometryType==geometryType.Polygon && isValid)
			{
				axlShapes.openObject(ref xml);
				axlShapes.PolygonSymbol(ref xml,"255,255,0","250,0,0",1.0,0.4);
				xml.WriteStartElement("POLYGON");
				xml.WriteStartElement("RING");
				foreach(xyPoint p in m_points) 
				{
					axlShapes.Point(ref xml,p);
				}
				xml.WriteEndElement(); // RING
				xml.WriteEndElement(); // POLYGON
				axlShapes.closeObject(ref xml);
			}
			xml.WriteEndElement(); // LAYER
		}
		public xyPoint this[int index] 
		{
			get 
			{
				if(index<0 || index>=m_points.Count) return null;
				return (xyPoint)m_points[index];
			}
		}

	}

	internal class xyPoint
	{ 
		public xyPoint(double x_,double y_) { x=x_; y=y_; } 
		public xyPoint(xyPoint p) 
		{
			x=p.x;
			y=p.y;
		}
		public bool ident(xyPoint p)
		{
			if(p==null) return false;
			if(Math.Abs(p.x-x)<1e-7 && Math.Abs(p.y-y)<1e-7) return true;
			return false;
		}
		public double x=0.0,y=0.0,z=0.0; 
		public void Round(int digits) 
		{
			x=Math.Round(x,digits);
			y=Math.Round(y,digits);
		}
	}
}
