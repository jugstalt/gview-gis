using System.IO;
using System.Xml;

namespace gView.Framework.Core.Xml;

/// <summary>
/// XML readers for untrusted input (service requests, SLD_BODY, responses of remote services).
/// DTDs are ignored (no entity expansion, "billion laughs") and no external resources are resolved (XXE).
/// Use <see cref="Extensions.XmlDocumentExtensions"/> to load an <see cref="XmlDocument"/>.
/// </summary>
public static class SafeXml
{
    /// <summary>
    /// New settings for untrusted input: DtdProcessing.Ignore, no XmlResolver
    /// </summary>
    public static XmlReaderSettings CreateReaderSettings(ConformanceLevel conformanceLevel = ConformanceLevel.Document)
        => new XmlReaderSettings()
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,
            ConformanceLevel = conformanceLevel
        };

    public static XmlReader CreateReader(string xml, ConformanceLevel conformanceLevel = ConformanceLevel.Document)
        => XmlReader.Create(new StringReader(xml ?? string.Empty), CreateReaderSettings(conformanceLevel));

    public static XmlReader CreateReader(TextReader textReader, ConformanceLevel conformanceLevel = ConformanceLevel.Document)
        => XmlReader.Create(textReader, CreateReaderSettings(conformanceLevel));

    public static XmlReader CreateReader(Stream stream, ConformanceLevel conformanceLevel = ConformanceLevel.Document)
        => XmlReader.Create(stream, CreateReaderSettings(conformanceLevel));
}
