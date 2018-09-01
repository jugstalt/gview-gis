using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace gView.Symbology.Framework.Symbology.IO
{
    class SoapFormatter
    {
        public void Serialize(MemoryStream ms, object obj)
        {
            // DoTo:
            if (obj?.GetType() == typeof(System.Drawing.Font))
            {
                var xml =
    @"<?xml version=""1.0""?>
<SOAP-ENV:Envelope SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:clr=""http://schemas.microsoft.com/soap/encoding/clr/1.0"" xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:SOAP-ENC=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<SOAP-ENV:Body>
<a1:Font xmlns:a1=""http://schemas.microsoft.com/clr/nsassem/System.Drawing/System.Drawing%2C%20Version%3D4.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Db03f5f7f11d50a3a"" id=""ref-1"">
<Name id=""ref-3"">{font-name}</Name>
<Size>{font-size}</Size>
<Style xmlns:a1=""http://schemas.microsoft.com/clr/nsassem/System.Drawing/System.Drawing%2C%20Version%3D4.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Db03f5f7f11d50a3a"" xsi:type=""a1:FontStyle"">{font-style}</Style>
<Unit xmlns:a1=""http://schemas.microsoft.com/clr/nsassem/System.Drawing/System.Drawing%2C%20Version%3D4.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Db03f5f7f11d50a3a"" xsi:type=""a1:GraphicsUnit"">{font-unit}</Unit>
</a1:Font>
</SOAP-ENV:Body>
</SOAP-ENV:Envelope>".Replace("\n", "").Replace("\n", "");

                var font = (System.Drawing.Font)obj;
                xml = xml.Replace("{font-name}", font.Name);
                xml = xml.Replace("{font-size}", font.Size.ToString().Replace(",", "."));
                xml = xml.Replace("{font-style}", font.Style.ToString());
                xml = xml.Replace("{font-unit}", font.Unit.ToString());

                var bytes = Encoding.ASCII.GetBytes(xml);
                ms.Write(bytes, 0, bytes.Length);
            }
        }

        public object Deserialize<T>(MemoryStream ms)
        {
            // ToDo:
            if(typeof(T)==typeof(System.Drawing.Font))
            {
                var soap = Encoding.ASCII.GetString(ms.ToArray());
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(soap);

                string name = "Arial";
                float size = 8;
                System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Regular;
                System.Drawing.GraphicsUnit grUnit = System.Drawing.GraphicsUnit.Point;

                XmlNode nameNode = doc.SelectSingleNode("//Name");
                if (nameNode != null)
                    name = nameNode.InnerText;

                XmlNode sizeNode = doc.SelectSingleNode("//Size");
                if (sizeNode != null)
                    size = NumberConverter.ToFloat(sizeNode.InnerText);

                XmlNode styleNode = doc.SelectSingleNode("//Style");
                if (styleNode != null)
                    fontStyle = (System.Drawing.FontStyle)Enum.Parse(typeof(System.Drawing.FontStyle), styleNode.InnerText);

                XmlNode unitNode = doc.SelectSingleNode("//Unit");
                if (styleNode != null)
                    grUnit = (System.Drawing.GraphicsUnit)Enum.Parse(typeof(System.Drawing.GraphicsUnit), unitNode.InnerText);


                var font = new System.Drawing.Font(name, size, fontStyle, grUnit);
                return font;
            }

            return default(T);
        }
    }
}
