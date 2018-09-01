using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Rest.Json
{
    public enum EsriFieldType
    {
        esriFieldTypeSmallInteger = 0,  //	Integer.
        esriFieldTypeInteger = 1, //	Long Integer.
        esriFieldTypeSingle = 2, //	Single-precision floating-point number.
        esriFieldTypeDouble = 3, //	Double-precision floating-point number.
        esriFieldTypeString = 4, //	Character string.
        esriFieldTypeDate = 5, //	Date.
        esriFieldTypeOID = 6, //	Long Integer representing an object identifier.
        esriFieldTypeGeometry = 7, //	Geometry.
        esriFieldTypeBlob = 8, //	Binary Large Object.
        esriFieldTypeRaster = 9, //	Raster.
        esriFieldTypeGUID = 10, //	Globally Unique Idendifier.
        esriFieldTypeGlobalID = 11, //ESRI Global ID.
        esriFieldTypeXML = 12 // XML Document
    }

    public enum EsriGeometryType
    {
        esriGeometryNull = 0, //	A geometry of unknown type.
        esriGeometryPoint = 1, //	A single zero dimensional geometry.
        esriGeometryMultipoint = 2, //	An ordered collection of points.
        esriGeometryLine = 13, //	A straight line segment between two points.
        esriGeometryCircularArc = 14, //	A portion of the boundary of a circle.
        esriGeometryEllipticArc = 16, //	A portion of the boundary of an ellipse.
        esriGeometryBezier3Curve = 15, //	A third degree bezier curve (four control points).
        esriGeometryPath = 6, //	A connected sequence of segments.
        esriGeometryPolyline = 3, //	An ordered collection of paths.
        esriGeometryRing = 11, //	An area bounded by one closed path.
        esriGeometryPolygon = 4, //	A collection of rings ordered by their containment relationship.
        esriGeometryEnvelope = 5, //	A rectangle indicating the spatial extent of another geometry.
        esriGeometryAny = 7, //	Any of the geometry coclass types.
        esriGeometryBag = 17, //	A collection of geometries of arbitrary type.
        esriGeometryMultiPatch = 9, //	A collection of surface patches.
        esriGeometryTriangleStrip = 18, //	A surface patch of triangles defined by three consecutive points.
        esriGeometryTriangleFan = 19, //	A surface patch of triangles defined by the first point and two consecutive points.
        esriGeometryRay = 20, //	An infinite, one-directional line extending from an origin point.
        esriGeometrySphere = 21, //	A complete 3 dimensional sphere.
        esriGeometryTriangles = 22 //	A surface patch of triangles defined by non-overlapping sets of three consecutive points each.
    }

    public enum ImageFormat
    {
        png, 
        png24,
        png32,
        jpg
    }
}

