using System;
using System.Xml;
using System.Text;
using System.IO;
 
namespace gView.Framework.XML
{
	/// <summary>
	/// Zusammenfassung für AXLRenderer.
	/// </summary>
	[Serializable]
	internal class AXLRenderer
	{
		string m_renderer;
		string m_labelrenderer;
		bool m_uselabel=false;
		double m_opacity=-1.0;

		double m_fontsizeFactor,m_widthFactor;

		public AXLRenderer(string axl)
		{
			m_renderer=axl;
			splitLabelRendererFromGrouprenderer();
		}
		
		public double Opacity 
		{
			get { return m_opacity; }
			set { m_opacity=value; }
		}

		public string Renderer 
		{
			get 
			{
				return (m_uselabel) ? addLabels2Renderer() : m_renderer;
			}
		}
		
		public string modifyRenderer(double dpi) 
		{
			return modifyRenderer(10.0,10.0,dpi);
		}
		public string modifyRenderer(double scale,double refscale,double dpi) 
		{
			if(m_renderer=="") return "";

			if(refscale>1.0) 
			{
				m_fontsizeFactor=m_widthFactor=refscale/Math.Max(scale,1.0)*(dpi/96.0);
			} 
			else
			{
				m_fontsizeFactor=m_widthFactor=-1.0;
			}
			XmlDocument xmldoc=new XmlDocument();
			xmldoc.LoadXml(this.Renderer);

			bool modified=false;
			modifyRenderer(xmldoc.ChildNodes,this.Opacity,ref modified);
			
			if(modified) return xmldoc.OuterXml;
			
			return Renderer;
		}

		protected void modifyRenderer(XmlNodeList nl,double opacity,ref bool modified) 
		{
			foreach(XmlNode xn in nl) 
			{
				if(opacity>=0.0) 
				{
					switch(xn.Name) 
					{
						case "SIMPLELINESYMBOL":
						case "SIMPLEMARKERSYMBOL":
						case "TEXTMARKERSYMBOL":
						case "TEXTSYMBOL":
						case "TRUETYPEMARKERSYMBOL":
							try 
							{
								if(xn.Attributes["transparency"]==null) 
								{
									if(opacity>=99.0) break;
									xn.Attributes.Append(xn.OwnerDocument.CreateAttribute("transparency"));
								}
							} 
							catch {}
							if(xn.Attributes["transparency"]!=null) 
							{
								xn.Attributes["transparency"].Value=(opacity/100.0).ToString();
								modified=true;
							}
							break;
						case "SIMPLEPOLYGONSYMBOL":
							try 
							{
								if(xn.Attributes["filltransparency"]==null) 
								{
									if(opacity>=99.0) break;
									xn.Attributes.Append(xn.OwnerDocument.CreateAttribute("filltransparency"));
								}
							} 
							catch {}
							if(xn.Attributes["filltransparency"]!=null) 
							{
								xn.Attributes["filltransparency"].Value=(opacity/100.0).ToString();
								modified=true;
							}
							break;
					}
				}	
				if(m_fontsizeFactor>0.0)
				{
					if(xn.Attributes["fontsize"]!=null) 
					{
						double val=Convert.ToDouble(xn.Attributes["fontsize"].Value.Replace(".",","));
						val*=m_fontsizeFactor;
						if(val<1.0) val=1.0;
						val=Math.Round(val,3);
						modified=true;
						xn.Attributes["fontsize"].Value=val.ToString();
					}
				}
				if(m_widthFactor>0.0)
				{
					if(xn.Attributes["width"]!=null) 
					{
						double val=Convert.ToDouble(xn.Attributes["width"].Value.ToString().Replace(".",","));
						val*=m_widthFactor;
						if(val<0.0) val=0.0;
						val=Math.Round(val,3);
						modified=true;
						xn.Attributes["width"].Value=val.ToString().Replace(",",".");
					}
					if(xn.Attributes["hotspot"]!=null)
					{
						string [] xy=xn.Attributes["hotspot"].Value.Split(',');
						if(xy.Length==2) 
						{
							double x=Convert.ToDouble(xy[0].Replace(".",",")),
								y=Convert.ToDouble(xy[1].Replace(".",","));
							x*=m_widthFactor;
							y*=m_widthFactor;
							x=Math.Round(x,5);
							y=Math.Round(y,5);
							modified=true;
							xn.Attributes["hotspot"].Value=
								x.ToString().Replace(",",".")+","+
								y.ToString().Replace(",",".");
						}
					}
				}
				if(xn.HasChildNodes) modifyRenderer(xn.ChildNodes,opacity,ref modified);
			}
		}
		

		protected XmlNode getParentXmlNode(XmlNode node,int maxChildNodes)
		{
			if(node.ParentNode==null) return node;
			if(node.ParentNode.ChildNodes==null) return node;
			if(node.ParentNode.ChildNodes.Count>maxChildNodes) return node;
			return getParentXmlNode(node.ParentNode,maxChildNodes);
		}

		protected void splitLabelRendererFromGrouprenderer()
		{
			try 
			{
				if(m_renderer=="") return;
				XmlDocument xmldoc=new XmlDocument();
				xmldoc.LoadXml(m_renderer);
			
				XmlNodeList label=xmldoc.SelectNodes("//SIMPLELABELRENDERER");
				if(label.Count!=1) 
				{
					m_labelrenderer="";
					m_uselabel=false;
				} 
				else 
				{
					XmlNode labelnode=getParentXmlNode(label[0],1);
					m_labelrenderer=labelnode.OuterXml;
					m_uselabel=true;
					m_renderer=xmldoc.OuterXml.Replace(labelnode.OuterXml,"");
				}
			} 
			catch 
			{
				m_labelrenderer="";
				m_uselabel=false;
			}
		}

		protected string addLabels2Renderer()
		{
			if(m_uselabel==false || m_labelrenderer=="") return m_renderer;

			string renderer="";
			
			try 
			{
				if(m_labelrenderer=="") return "";

				XmlDocument xmldoc=new XmlDocument();
				xmldoc.LoadXml(m_renderer);

				XmlNodeList Group=xmldoc.SelectNodes("GROUPRENDERER");
				if(Group.Count==1) 
				{
					renderer="<GROUPRENDERER>"+Group[0].InnerXml+m_labelrenderer+"</GROUPRENDERER>";
				} 
				else if(Group.Count==0) 
				{
					renderer="<GROUPRENDERER>"+xmldoc.OuterXml+m_labelrenderer+"</GROUPRENDERER>";
				} 
				else 
				{
					renderer=m_renderer;
				}
			} 
			catch 
			{
				return m_renderer;
			}
			return renderer;
		}
	}

	internal class SimpleAXLLabelRenderer
	{
		protected XmlNode m_label,m_slr;
		protected string m_field,m_borderstyle;

		public SimpleAXLLabelRenderer() {}
		
		public void Create(string field) 
		{
			m_field=field;

			StringBuilder axl=new StringBuilder();
			StringWriter sw=new StringWriter(axl);
			XmlTextWriter xml=new XmlTextWriter(sw);

			xml.WriteStartDocument();
			xml.WriteStartElement("SIMPLELABELRENDERER");
			xml.WriteAttributeString("field",m_field);
			xml.WriteStartElement("TEXTSYMBOL");
			xml.WriteAttributeString("antialiasing","true");
			xml.WriteAttributeString("font","Arial");
			xml.WriteAttributeString("fontsize","11");
			xml.WriteEndElement(); // TEXTSYMBOL
			xml.WriteEndElement(); // SIMPLELABELRENDERER
			xml.WriteEndDocument();

			XmlTextReader xr=new XmlTextReader(axl.ToString(),XmlNodeType.Element,null);
			XmlDocument doc=new XmlDocument();
			doc.Load(xr);
			m_label=m_slr=doc.SelectNodes("SIMPLELABELRENDERER")[0];
		}
		public string field 
		{
			get 
			{
				return getSlrAttribute("field");
			}
			set 
			{
				setSlrAttribute("field",value);
			}
		}
		public string howmanylabels
		{
			get 
			{
				return getSlrAttribute("howmanylabels");
			}
			set 
			{
				setSlrAttribute("howmanylabels",value);
			}
		}
		public string fontsize
		{
			get 
			{
				return getTextAttribute("fontsize");
			}
			set 
			{
				setTextAttribute("fontsize",value);
			}
		}
		public string fontstyle
		{
			get 
			{
				return getTextAttribute("fontstyle");
			}
			set 
			{
				setTextAttribute("fontstyle",value);
			}
		}
		public string fontcolor
		{
			get 
			{
				return getTextAttribute("fontcolor");
			}
			set 
			{
				setTextAttribute("fontcolor",value);
			}
		}
		public string borderstyle
		{
			get 
			{
				if(getTextAttribute("shadow")!="") return "shadow";
				if(getTextAttribute("glowing")!="") return "glowing";
				if(getTextAttribute("blockout")!="") return "blockout";
				return "none";
			}
			set 
			{
				m_borderstyle=value;
			}
		}
		public string bordercolor
		{
			get 
			{
				if(getTextAttribute("shadow")!="") return getTextAttribute("shadow");
				if(getTextAttribute("glowing")!="") return getTextAttribute("glowing");
				if(getTextAttribute("blockout")!="") return getTextAttribute("blockout");
				return "";
			}
			set 
			{
				if(m_borderstyle!="shadow") removeTextAttribute("shadow");
				if(m_borderstyle!="glowing") removeTextAttribute("glowing");
				if(m_borderstyle!="blockout") removeTextAttribute("blockout");
				if(m_borderstyle!="node") setTextAttribute(m_borderstyle,value);
			}
		}
		protected string getSlrAttribute(string name)
		{
			if(m_slr==null) return "";
			if(m_slr.Attributes[name]==null) return "";
			return m_slr.Attributes[name].Value;	
		}
		protected void setSlrAttribute(string name,string val)
		{
			if(m_slr==null) return;
			if(m_slr.Attributes[name]!=null)
				m_slr.Attributes[name].Value=val;
			else 
			{
				XmlAttribute attr = m_slr.OwnerDocument.CreateAttribute(name);
				attr.Value=val;
				m_slr.Attributes.Append(attr);
			}
		}
		protected void removeTextAttribute(string name)
		{
			if(m_slr==null) return;
			if(m_slr.ChildNodes.Count==0) return;
			if(m_slr.ChildNodes[0].Attributes[name]==null) return;
			m_slr.ChildNodes[0].Attributes.Remove(m_slr.ChildNodes[0].Attributes[name]);
		}
		protected string getTextAttribute(string name)
		{
			if(m_slr==null) return "";
			if(m_slr.ChildNodes.Count==0) return "";
			if(m_slr.ChildNodes[0].Attributes[name]==null) return "";
			return m_slr.ChildNodes[0].Attributes[name].Value;	
		}
		protected void setTextAttribute(string name,string val)
		{
			if(m_slr==null) return;
			if(m_slr.ChildNodes.Count==0) return;
			if(m_slr.ChildNodes[0].Attributes[name]!=null)
				m_slr.ChildNodes[0].Attributes[name].Value=val;
			else 
			{
				XmlAttribute attr = m_slr.OwnerDocument.CreateAttribute(name);
				attr.Value=val;
				m_slr.ChildNodes[0].Attributes.Append(attr);
			}
		}
		protected XmlNode getLabelRendererNode() 
		{
			if(m_label==null) return null;
			XmlNodeList slr=m_label.SelectNodes("//SIMPLELABELRENDERER");
			if(slr.Count!=1) return null;
			return slr[0];
		}
		public XmlNode renderer 
		{
			get 
			{
				return m_label;
			}
			set 
			{
				if(value==null) 
					Create("");
				else 
				{
					m_label=value;
					m_slr=getLabelRendererNode();
				}
			}
		}
	}
}
