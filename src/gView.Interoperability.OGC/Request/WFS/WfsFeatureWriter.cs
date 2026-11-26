#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.OGC.GML;
using gView.Interoperability.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// The features of one feature type in a GetFeature response
/// </summary>
public record WfsFeatureSet(OgcCollectionLayer Collection, IReadOnlyList<OgcFeature> Features);

/// <summary>
/// Writes the wfs:FeatureCollection of a GetFeature response: WFS 1.0.0/1.1.0 with gml:featureMember,
/// WFS 2.0.0 with wfs:member. Features are written as gv:c{layerId} with gml:id (fid in GML 2)
/// c{layerId}.{objectId}, the geometry property (named after the shape field) and the property
/// fields (null values are omitted).
/// </summary>
public class WfsFeatureWriter
{
    private readonly WfsVersion _version;
    private readonly GmlFormat _format;
    private readonly bool _swapAxes;
    private readonly string? _srsName;

    /// <param name="srsName">The srsName of the geometries (in the notation of the request), null to omit it</param>
    /// <param name="swapAxes">Write northing first (see <see cref="WfsVersions.SwapAxes"/>)</param>
    public WfsFeatureWriter(WfsVersion version, GmlFormat format, string? srsName, bool swapAxes)
    {
        _version = version;
        _format = format;
        _srsName = srsName;
        _swapAxes = swapAxes;
    }

    /// <param name="describeFeatureTypeUrl">The DescribeFeatureType URL for xsi:schemaLocation, null to omit it</param>
    /// <param name="timeStamp">The timeStamp attribute (1.1.0, 2.0.0)</param>
    public byte[] Write(IReadOnlyList<WfsFeatureSet> featureSets, string? describeFeatureTypeUrl, DateTime timeStamp)
    {
        string wfs = _version.WfsNamespace;
        string gml = GmlGeometryWriter.NamespaceOf(_format);

        int numberReturned = 0;
        foreach (var set in featureSets)
        {
            numberReturned += set.Features.Count;
        }

        using var ms = new MemoryStream();
        using (var w = XmlWriter.Create(ms, WfsXml.WriterSettings))
        {
            var geometryWriter = new GmlGeometryWriter(w, _format, _swapAxes);

            w.WriteStartDocument();
            w.WriteStartElement("wfs", "FeatureCollection", wfs);
            w.WriteAttributeString("xmlns", "gml", null, gml);
            w.WriteAttributeString("xmlns", WfsVersions.FeatureTypePrefix, null, WfsVersions.FeatureTypeNamespace);
            w.WriteAttributeString("xmlns", "xsi", null, WfsVersions.XsiNamespace);

            string schemaLocation = $"{wfs} {WfsVersions.WfsSchemaLocation(_version)}";
            if (!String.IsNullOrEmpty(describeFeatureTypeUrl))
            {
                schemaLocation = $"{WfsVersions.FeatureTypeNamespace} {describeFeatureTypeUrl} {schemaLocation}";
            }
            if (_format == GmlFormat.Gml32)
            {
                schemaLocation += $" {gml} {WfsVersions.GmlSchemaLocation(GmlFormat.Gml32)}";
            }
            w.WriteAttributeString("xsi", "schemaLocation", WfsVersions.XsiNamespace, schemaLocation);

            string timeStampValue = timeStamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            if (_version == WfsVersion.V200)
            {
                w.WriteAttributeString("timeStamp", timeStampValue);
                w.WriteAttributeString("numberMatched", "unknown");
                w.WriteAttributeString("numberReturned", numberReturned.ToString(CultureInfo.InvariantCulture));
            }
            else if (_version == WfsVersion.V110)
            {
                w.WriteAttributeString("timeStamp", timeStampValue);
                w.WriteAttributeString("numberOfFeatures", numberReturned.ToString(CultureInfo.InvariantCulture));
            }

            if (_format == GmlFormat.Gml2)
            {
                // GML 2: boundedBy is mandatory for feature collections
                w.WriteStartElement("gml", "boundedBy", gml);
                w.WriteElementString("gml", "null", gml, "unknown");
                w.WriteEndElement();
            }

            foreach (var set in featureSets)
            {
                foreach (var feature in set.Features)
                {
                    w.WriteStartElement(_version == WfsVersion.V200 ? "wfs" : "gml",
                                        _version == WfsVersion.V200 ? "member" : "featureMember",
                                        _version == WfsVersion.V200 ? wfs : gml);
                    WriteFeature(w, geometryWriter, set.Collection, feature);
                    w.WriteEndElement();
                }
            }

            w.WriteEndElement();  // FeatureCollection
            w.WriteEndDocument();
        }

        return ms.ToArray();
    }

    private void WriteFeature(XmlWriter w, GmlGeometryWriter geometryWriter, OgcCollectionLayer collection, OgcFeature feature)
    {
        string gml = geometryWriter.Namespace;
        string gv = WfsVersions.FeatureTypeNamespace;
        string fid = $"{collection.Id}.{FormatValue(feature.Id) ?? "0"}";

        w.WriteStartElement(WfsVersions.FeatureTypePrefix, collection.Id, gv);
        if (_format == GmlFormat.Gml2)
        {
            w.WriteAttributeString("fid", fid);
        }
        else
        {
            w.WriteAttributeString("gml", "id", gml, fid);
        }

        var geometry = feature.Geometry;

        if (geometry is not null && _format != GmlFormat.Gml32 && geometry.Envelope is IEnvelope envelope)
        {
            w.WriteStartElement("gml", "boundedBy", gml);
            geometryWriter.WriteEnvelope(envelope, _srsName);
            w.WriteEndElement();
        }

        if (geometry is not null)
        {
            w.WriteStartElement(WfsVersions.FeatureTypePrefix, WfsFeatureTypes.GeometryElementName(collection), gv);
            geometryWriter.WriteGeometry(geometry, _srsName, $"{fid}.geom");
            w.WriteEndElement();
        }

        foreach (var property in feature.Properties)
        {
            string? value = FormatValue(property.Value);
            if (value is null)
            {
                continue;
            }

            w.WriteElementString(WfsVersions.FeatureTypePrefix, WfsFeatureTypes.ElementName(property.Key), gv, WfsXml.ValidXmlText(value));
        }

        w.WriteEndElement();
    }

    /// <summary>
    /// Values as XSD literals (invariant culture, ISO dates), null for null values
    /// </summary>
    public static string? FormatValue(object? value)
        => value switch
        {
            null or DBNull => null,
            string s => s,
            DateTime dt => dt.Kind == DateTimeKind.Utc
                ? dt.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFZ", CultureInfo.InvariantCulture)
                : dt.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz", CultureInfo.InvariantCulture),
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            double d => GmlGeometryWriter.Number(d),
            float f => f.ToString("R", CultureInfo.InvariantCulture),
            byte[] bytes => Convert.ToBase64String(bytes),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
}
