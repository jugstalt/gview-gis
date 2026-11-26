#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Interoperability.Extensions;
using gView.Interoperability.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// The feature types of a WFS: the feature collections of the OGC API ("c{layerId}"),
/// named gv:c{layerId} in the namespace <see cref="WfsVersions.FeatureTypeNamespace"/>.
/// </summary>
public class WfsFeatureTypes
{
    private readonly List<OgcCollectionLayer> _collections;

    public WfsFeatureTypes(IServiceMap map)
        : this(map.OgcCollectionLayers().Where(c => c.HasItems))
    {
    }

    public WfsFeatureTypes(IEnumerable<OgcCollectionLayer> collections)
    {
        _collections = collections.ToList();
    }

    public IReadOnlyList<OgcCollectionLayer> All => _collections;

    public static string QualifiedName(OgcCollectionLayer collection)
        => $"{WfsVersions.FeatureTypePrefix}:{collection.Id}";

    /// <summary>
    /// Finds the feature type of a type name. Lenient: c0, gv:c0, any prefix (e.g. wfs:c0 as advertised
    /// by older gView versions) and {namespace}c0 are accepted, the local name is compared case insensitive.
    /// </summary>
    public OgcCollectionLayer? Find(string typeName)
    {
        string localName = LocalName(typeName);
        return _collections.FirstOrDefault(c => c.Id.Equals(localName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resolves the type names of a request, all feature types if there are none
    /// </summary>
    /// <exception cref="WfsException">Unknown type name</exception>
    public List<OgcCollectionLayer> Resolve(IEnumerable<string> typeNames, string locator, bool allIfEmpty)
    {
        var result = new List<OgcCollectionLayer>();

        foreach (var typeName in typeNames)
        {
            var collection = Find(typeName)
                ?? throw WfsException.InvalidParameterValue(locator, $"Unknown feature type '{typeName}'");
            if (!result.Contains(collection))
            {
                result.Add(collection);
            }
        }

        if (result.Count == 0 && allIfEmpty)
        {
            result.AddRange(_collections);
        }

        return result;
    }

    public static string LocalName(string typeName)
    {
        typeName = typeName.Trim();

        if (typeName.StartsWith("{") && typeName.Contains('}'))  // {namespace}c0
        {
            typeName = typeName.Substring(typeName.IndexOf('}') + 1);
        }

        int pos = typeName.LastIndexOf(':');
        return pos >= 0 ? typeName.Substring(pos + 1) : typeName;
    }

    /// <summary>
    /// The XML element name of a field (the same name in the schema and the features)
    /// </summary>
    public static string ElementName(string fieldName)
        => XmlConvert.EncodeLocalName(fieldName.Replace("#", "").Replace(".", "_").Replace("()", ""));  // like the GML of former versions (Shape.STLength() => Shape_STLength)

    /// <summary>
    /// The name of the geometry property: the shape field of the feature class
    /// </summary>
    public static string GeometryElementName(OgcCollectionLayer collection)
        => ElementName(String.IsNullOrEmpty(collection.FeatureClass?.ShapeFieldName) ? "SHAPE" : collection.FeatureClass!.ShapeFieldName);

    /// <summary>
    /// The extent of the data in WGS84 (lon/lat), null if unknown
    /// </summary>
    public static IEnvelope? Wgs84Extent(IServiceMap map, OgcCollectionLayer collection)
    {
        if (collection.NativeExtent is null || collection.SpatialReference is null)
        {
            return null;
        }

        try
        {
            var env = GeometricTransformerFactory.Transform2D(
                collection.NativeExtent,
                collection.SpatialReference,
                SpatialReference.FromID("epsg:4326"),
                map.Display?.DatumTransformations)?.Envelope;

            return env is null || Double.IsNaN(env.MinX) || Double.IsInfinity(env.MinX) ? null : env;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// The default CRS of the feature types: the CRS of the map, otherwise the CRS of the data
    /// </summary>
    public static CrsReference? DefaultCrs(IServiceMap map, OgcCollectionLayer? collection, WfsVersion version)
    {
        int epsg = map.Display?.SpatialReference?.EpsgCode ?? 0;
        if (epsg <= 0)
        {
            epsg = collection?.SpatialReference?.EpsgCode ?? 0;
        }

        return epsg > 0 ? CrsReference.FromEpsg(epsg, version.DefaultCrsNotation) : null;
    }

    /// <summary>
    /// The other CRS: the EPSG codes of the WFS export metadata of the service
    /// </summary>
    public static IEnumerable<CrsReference> OtherCrs(IServiceMap map, CrsReference? defaultCrs, WfsVersion version)
    {
        var codes = (map.MetadataProvider(WFS_Export_Metadata.MetadataGuid) as IEpsgMetadata)?.EpsgCodes ?? [];
        var result = new List<CrsReference>();

        foreach (var code in codes)
        {
            if (CrsReference.TryParse(code, out var crs) && !crs.IsCrs84)
            {
                crs = crs.WithNotation(version.DefaultCrsNotation);
                if (!crs.Equals(defaultCrs) && !result.Contains(crs))
                {
                    result.Add(crs);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// The XSD type of a field
    /// </summary>
    public static string XsdType(IField field)
        => field.type switch
        {
            FieldType.smallinteger => "short",
            FieldType.integer => "int",
            FieldType.biginteger or FieldType.ID => "long",
            FieldType.Float => "float",
            FieldType.Double => "double",
            FieldType.boolean => "boolean",
            FieldType.Date => "dateTime",
            FieldType.binary => "base64Binary",
            _ => "string"
        };
}
