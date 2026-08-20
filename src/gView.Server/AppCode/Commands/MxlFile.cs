using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Server.AppCode.Commands;

/// <summary>
/// Extracts the &lt;MapDocument&gt; XML fragment from a raw .mxl file on
/// disk, the same way BrowseServicesController.AddService does for uploaded
/// files. An .mxl file's root is "MapServer" wrapping "MapDocument" (see
/// MapServiceDeploymentManager.SaveConfig), but
/// MapServiceDeploymentManager.AddMap expects just the inner "MapDocument"
/// fragment as its mapXml argument.
/// </summary>
static internal class MxlFile
{
    async static public Task<string> ExtractMapDocumentXmlAsync(string mxlPath)
    {
        var buffer = await File.ReadAllBytesAsync(mxlPath);

        foreach (var encoding in new Encoding[]
                 {
                     Encoding.UTF8,
                     Encoding.Unicode,
                     Encoding.UTF32,
                     //Encoding.UTF7,
                     Encoding.Default
                 })
        {
            try
            {
                string xml = encoding.GetString(buffer);

                int index = xml.IndexOf("<");
                if (index < 0)
                {
                    continue;
                }

                // Cut leading bytes -> often strange characters that are not XML conform
                xml = xml.Substring(index);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                var mapDocumentNode = doc.SelectSingleNode("//MapDocument");

                if (mapDocumentNode != null)
                {
                    return mapDocumentNode.OuterXml;
                }
            }
            catch
            {
                // try next encoding
            }
        }

        return String.Empty;
    }
}
