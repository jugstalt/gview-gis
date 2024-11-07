using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace gView.Framework.Common
{
    public class XsdSchemaSerializer<T>
    {
        public string Serialize(T o, XmlSerializerNamespaces ns)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            MemoryStream ms = new MemoryStream();
            UTF8Encoding utf8e = new UTF8Encoding();
            XmlTextWriter xmlSink = new XmlTextWriter(ms, utf8e);
            xmlSink.Formatting = Formatting.Indented;
            if (ns != null)
            {
                ser.Serialize(xmlSink, o, ns);
            }
            else
            {
                ser.Serialize(xmlSink, o);
            }

            byte[] utf8EncodedData = ms.ToArray();
            return utf8e.GetString(utf8EncodedData);
        }

        public string Serialize(T o)
        {
            return Serialize(o, null);
        }
    }
}
