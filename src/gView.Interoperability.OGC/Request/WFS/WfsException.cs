#nullable enable

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// An error of a WFS request, answered with an exception report (OWS exceptionCode + locator)
/// </summary>
public class WfsException : Exception
{
    public WfsException(string code, string message, string? locator = null)
        : base(message)
    {
        Code = code;
        Locator = locator;
    }

    public string Code { get; }
    public string? Locator { get; }

    /// <summary>
    /// The HTTP status of the exception report (WFS 2.0.0, OWS Common 1.1). WFS 1.x answers with 200.
    /// </summary>
    public int HttpStatusCode => Code switch
    {
        "OperationNotSupported" or "OptionNotSupported" => 501,
        "NotFound" => 404,
        "NoApplicableCode" or "OperationProcessingFailed" => 500,
        _ => 400
    };

    public static WfsException MissingParameterValue(string locator)
        => new WfsException("MissingParameterValue", $"Mandatory parameter {locator} is missing", locator);

    public static WfsException InvalidParameterValue(string locator, string message)
        => new WfsException("InvalidParameterValue", message, locator);

    public static WfsException OperationNotSupported(string operation)
        => new WfsException("OperationNotSupported", $"Operation {operation} is not supported", operation);

    public static WfsException OptionNotSupported(string locator, string message)
        => new WfsException("OptionNotSupported", message, locator);

    public static WfsException OperationParsingFailed(string locator, string message)
        => new WfsException("OperationParsingFailed", message, locator);
}

/// <summary>
/// Writes the exception report of a WFS version
/// </summary>
public static class WfsExceptionReport
{
    public static byte[] Write(WfsVersion version, string code, string message, string? locator)
    {
        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, WfsXml.WriterSettings))
        {
            writer.WriteStartDocument();

            if (version == WfsVersion.V100)
            {
                writer.WriteStartElement("ServiceExceptionReport", WfsVersions.OgcNamespace);
                writer.WriteAttributeString("version", "1.2.0");
                writer.WriteStartElement("ServiceException", WfsVersions.OgcNamespace);
                writer.WriteAttributeString("code", code);
                if (!String.IsNullOrEmpty(locator))
                {
                    writer.WriteAttributeString("locator", locator);
                }
                writer.WriteString(message);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            else
            {
                string ows = version == WfsVersion.V200 ? WfsVersions.Ows11Namespace : WfsVersions.Ows1Namespace;

                writer.WriteStartElement("ows", "ExceptionReport", ows);
                writer.WriteAttributeString("version", version == WfsVersion.V200 ? "2.0.0" : "1.0.0");
                if (version == WfsVersion.V200)
                {
                    writer.WriteAttributeString("xml", "lang", null, "en");  // OWS 1.1
                }
                else
                {
                    writer.WriteAttributeString("language", "en");  // OWS 1.0
                }
                writer.WriteStartElement("ows", "Exception", ows);
                writer.WriteAttributeString("exceptionCode", code);
                if (!String.IsNullOrEmpty(locator))
                {
                    writer.WriteAttributeString("locator", locator);
                }
                writer.WriteElementString("ows", "ExceptionText", ows, message);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndDocument();
        }

        return ms.ToArray();
    }
}

internal static class WfsXml
{
    public static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings()
    {
        Encoding = new UTF8Encoding(false),
        Indent = false,
        NamespaceHandling = NamespaceHandling.OmitDuplicates
    };

    /// <summary>
    /// Removes characters that are not allowed in XML (e.g. control characters in attribute values)
    /// </summary>
    public static string ValidXmlText(string value)
    {
        foreach (char c in value)
        {
            if (!XmlConvert.IsXmlChar(c) && !Char.IsSurrogate(c))
            {
                var sb = new StringBuilder(value.Length);
                foreach (char ch in value)
                {
                    if (XmlConvert.IsXmlChar(ch) || Char.IsSurrogate(ch))
                    {
                        sb.Append(ch);
                    }
                }
                return sb.ToString();
            }
        }
        return value;
    }
}
