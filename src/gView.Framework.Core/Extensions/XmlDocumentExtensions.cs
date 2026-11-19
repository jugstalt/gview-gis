using gView.Framework.Core.Xml;
using System.IO;
using System.Xml;

namespace gView.Framework.Core.Extensions;

/// <summary>
/// Loads untrusted XML (service requests, SLD_BODY, responses of remote services) into an <see cref="XmlDocument"/>
/// without DTD processing and without resolving external resources (see <see cref="SafeXml"/>).
/// Use these instead of <see cref="XmlDocument.LoadXml"/> / <see cref="XmlDocument.Load(string)"/> for such input.
/// </summary>
public static class XmlDocumentExtensions
{
    public static void LoadXmlSafe(this XmlDocument doc, string xml)
    {
        doc.XmlResolver = null;
        using var reader = SafeXml.CreateReader(xml);
        doc.Load(reader);
    }

    public static void LoadSafe(this XmlDocument doc, Stream stream)
    {
        doc.XmlResolver = null;
        using var reader = SafeXml.CreateReader(stream);
        doc.Load(reader);
    }

    public static void LoadSafe(this XmlDocument doc, TextReader textReader)
    {
        doc.XmlResolver = null;
        using var reader = SafeXml.CreateReader(textReader);
        doc.Load(reader);
    }
}
