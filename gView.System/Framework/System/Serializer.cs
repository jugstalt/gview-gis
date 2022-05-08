using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace gView.Framework.system
{
    public class XsdSchemaSerializer<T>
    {
        public T Deserialize(Stream stream)
        {
            // avoid errors => DTD parsing is not allowed per default:
            // https://stackoverflow.com/questions/13854068/dtd-prohibited-in-xml-document-exception

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            settings.MaxCharactersFromEntities = 1024;

            using (XmlReader xmlReader = XmlReader.Create(stream, settings))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                return (T)ser.Deserialize(xmlReader);
            }
        }

        public T FromString(string xml, global::System.Text.Encoding encoding)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = encoding.GetBytes(xml);
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            return Deserialize(ms);
        }

        public T FromUrl(string url, WebProxy proxy)
        {
            HttpWebRequest wReq = (HttpWebRequest)HttpWebRequest.Create(url);

            if (proxy != null)
            {
                wReq.Proxy = proxy;
            }

            wReq.Timeout = 360000;
            HttpWebResponse wresp = (HttpWebResponse)wReq.GetResponse();

            int Bytes2Read = 3500000;
            Byte[] b = new Byte[Bytes2Read];

            DateTime t1 = DateTime.Now;
            Stream stream = wresp.GetResponseStream();

            MemoryStream memStream = new MemoryStream();

            while (Bytes2Read > 0)
            {
                int len = stream.Read(b, 0, Bytes2Read);
                if (len == 0)
                {
                    break;
                }

                memStream.Write(b, 0, len);
            }
            memStream.Position = 0;
            string xml = Encoding.UTF8.GetString(memStream.GetBuffer()).Trim(' ', '\0').Trim();
            memStream.Close();
            memStream.Dispose();

            return FromString(xml, global::System.Text.Encoding.UTF8);
        }

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
