using gView.Framework.Core.Extensions;
using gView.Framework.Core.Xml;
using System.Xml;

namespace gView.Framework.Core.Tests.Extensions;

public class XmlDocumentExtensionsTests
{
    private const string BillionLaughs = """
        <?xml version="1.0"?>
        <!DOCTYPE lolz [
          <!ENTITY lol "lol">
          <!ENTITY lol2 "&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;">
          <!ENTITY lol3 "&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;">
          <!ENTITY lol4 "&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;">
          <!ENTITY lol5 "&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;&lol4;">
          <!ENTITY lol6 "&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;&lol5;">
          <!ENTITY lol7 "&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;&lol6;">
          <!ENTITY lol8 "&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;&lol7;">
          <!ENTITY lol9 "&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;&lol8;">
        ]>
        <lolz>&lol9;</lolz>
        """;

    [Fact]
    public void LoadXmlSafe_PlainXml_IsLoaded()
    {
        var doc = new XmlDocument();

        doc.LoadXmlSafe("<ARCXML version=\"1.1\"><REQUEST><GET_IMAGE /></REQUEST></ARCXML>");

        Assert.NotNull(doc.SelectSingleNode("ARCXML/REQUEST/GET_IMAGE"));
    }

    [Fact]
    public void LoadXmlSafe_BillionLaughs_ThrowsInsteadOfExpanding()
    {
        var doc = new XmlDocument();

        Assert.Throws<XmlException>(() => doc.LoadXmlSafe(BillionLaughs));
    }

    [Fact]
    public void LoadXmlSafe_ExternalEntity_IsNotResolved()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "secret-content");
            var xml = $"""
                <?xml version="1.0"?>
                <!DOCTYPE foo [ <!ENTITY xxe SYSTEM "{new Uri(file).AbsoluteUri}"> ]>
                <foo>&xxe;</foo>
                """;
            var doc = new XmlDocument();

            var ex = Record.Exception(() => doc.LoadXmlSafe(xml));

            Assert.IsType<XmlException>(ex);
            Assert.DoesNotContain("secret-content", doc.OuterXml);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void LoadXmlSafe_DoctypeWithoutEntityReferences_IsIgnored()
    {
        // e.g. WMS 1.1.1 capabilities of remote services
        var xml = """
            <?xml version="1.0"?>
            <!DOCTYPE WMT_MS_Capabilities SYSTEM "http://schemas.opengis.net/wms/1.1.1/WMS_MS_Capabilities.dtd">
            <WMT_MS_Capabilities version="1.1.1"><Service><Name>OGC:WMS</Name></Service></WMT_MS_Capabilities>
            """;
        var doc = new XmlDocument();

        doc.LoadXmlSafe(xml);

        Assert.Equal("OGC:WMS", doc.SelectSingleNode("WMT_MS_Capabilities/Service/Name")?.InnerText);
    }

    [Fact]
    public void LoadSafe_Stream_BillionLaughs_Throws()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(BillionLaughs));
        var doc = new XmlDocument();

        Assert.Throws<XmlException>(() => doc.LoadSafe(stream));
    }

    [Fact]
    public void SafeXml_CreateReader_Fragment_ReadsMultipleElements()
    {
        using var reader = SafeXml.CreateReader("<A>1</A><B>2</B>", ConformanceLevel.Fragment);

        int elements = 0;
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                elements++;
            }
        }

        Assert.Equal(2, elements);
    }
}
