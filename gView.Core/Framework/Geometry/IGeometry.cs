using System;
using System.IO;
using gView.Framework.IO;
using System.Collections.Generic;
using gView.Framework.system;

namespace gView.Framework.Geometry
{
    public enum geometryType
    {
        Point = 0,
        Multipoint = 1,
        Polyline = 2,
        Polygon = 3,
        Aggregate = 4,
        Envelope = 5,
        Unknown = 6,
        Network = 7
    }

    /// <summary>
    /// Zusammenfassung für IGeometry.
    /// </summary>

    public interface IGeometry : ICloneable
    {
        geometryType GeometryType { get; }
        IEnvelope Envelope { get; }
        int? Srs { get; set; }

        void Serialize(BinaryWriter w, IGeometryDef geomDef);
        void Deserialize(BinaryReader r, IGeometryDef geomDef);

        bool Equals(object obj, double epsi);
    }

    public interface IGeometryDef
    {
        bool HasZ { get; }
        bool HasM { get; }
        ISpatialReference SpatialReference { get; }
        geometryType GeometryType { get; }
        //int DigitAccuracy { get; }
        //gView.Framework.Data.GeometryFieldType GeometryFieldType { get; }
    }

    public interface IEnvelope : IGeometry
    {
        double minx
        {
            get;
            set;
        }

        double miny
        {
            get;
            set;
        }

        double maxx
        {
            get;
            set;
        }

        double maxy
        {
            get;
            set;
        }

        IPoint LowerLeft { get; set; }
        IPoint LowerRight { get; set; }
        IPoint UpperLeft { get; set; }
        IPoint UpperRight { get; set; }
        IPoint Center { get; set; }
        IPolygon ToPolygon(int accuracy);
        IPointCollection ToPointCollection(int accuracy);

        void Union(IEnvelope envelope);

        double Width { get; }
        double Height { get; }

        void Translate(double mx, double my);
        void TranslateTo(double mx, double my);

        bool Intersects(IEnvelope envelope);
        bool Contains(IEnvelope envelope);
        bool Contains(IPoint point);

        string ToBBoxString();
    }

    public interface IPoint : IGeometry
    {
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        double M { get; set; }

        double Distance(IPoint p);
        double Distance2(IPoint p);
    }

    public interface IPointCollection : IEnumerable<IPoint>
    {
        void AddPoint(IPoint point);
        void InsertPoint(IPoint point, int pos);
        void RemovePoint(int pos);
        void AddPoints(IPointCollection pColl);

        int PointCount { get; }
        IPoint this[int pointIndex] { get; }

        IEnvelope Envelope { get; }

        void Serialize(BinaryWriter w, IGeometryDef geomDef);
        void Deserialize(BinaryReader r, IGeometryDef geomDef);

        bool Equals(object obj, double epsi);
    }

    public interface IMultiPoint : IGeometry, IPointCollection
    {
    }

    public interface IPath : IPointCollection, ICloneable
    {
        double Length { get; }
        bool Equals(object obj, double epsi);
        void ClosePath();

        void ChangeDirection();
        IPath Trim(double length);
    }

    public interface IRing : IPath
    {
        double Area { get; }
        IPoint Centroid { get; }
        void Close();
    }

    public interface IHole : IRing
    {
    }

    public interface IPolyline : IGeometry, IEnumerable<IPath>
    {
        void AddPath(IPath path);
        void InsertPath(IPath path, int pos);
        void RemovePath(int pos);

        int PathCount { get; }
        IPath this[int pathIndex] { get; }

        double Length { get; }
    }

    public interface IPolygon : IGeometry, IEnumerable<IRing>
    {
        void AddRing(IRing ring);
        void InsertRing(IRing ring, int pos);
        void RemoveRing(int pos);

        int RingCount { get; }
        IRing this[int ringIndex] { get; }

        void VerifyHoles();

        double Area { get; }
    }

    public interface IAggregateGeometry : IGeometry, IEnumerable<IGeometry>
    {
        void AddGeometry(IGeometry geometry);
        void InsertGeometry(IGeometry geometry, int pos);
        void RemoveGeometry(int pos);

        int GeometryCount { get; }
        IGeometry this[int geometryIndex] { get; }

        List<IPoint> PointGeometries { get; }
        IMultiPoint MergedPointGeometries { get; }
        List<IPolyline> PolylineGeometries { get; }
        IPolyline MergedPolylineGeometries { get; }
        List<IPolygon> PolygonGeometries { get; }
        IPolygon MergedPolygonGeometries { get; }
    }

    public interface ISpatialParameters : IClone
    {
        gView.Framework.Carto.GeoUnits Unit { get; }
        bool IsGeographic { get; }

        double lat_0 { get; }
        double lon_0 { get; }
        double x_0 { get; }
        double y_0 { get; }
    }

    public enum AxisDirection
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public interface ISpatialReference : IPersistable, IClone, IXmlString, IBase64String
    {
        string Name { get; }
        string Description { get; }

        string[] Parameters
        {
            get;
        }

        ISpatialParameters SpatialParameters { get; }

        AxisDirection Gml3AxisX { get; }
        AxisDirection Gml3AxisY { get; }

        int EpsgCode { get; }

        IGeodeticDatum Datum
        {
            get;
            set;
        }

        bool Equals(ISpatialReference sRef);
    }

    public interface IGeodeticDatum : IPersistable, IClone
    {
        double X_Axis { get; set; }
        double Y_Axis { get; set; }
        double Z_Axis { get; set; }
        double X_Rotation { get; set; }
        double Y_Rotation { get; set; }
        double Z_Rotation { get; set; }
        double Scale_Diff { get; set; }

        string Name { get; set; }
        string Parameter { get; set; }
    }

    public interface IGeometricTransformer
    {
        //ISpatialReference FromSpatialReference { set; get; }
        //ISpatialReference ToSpatialReference { set; get;  }

        void SetSpatialReferences(ISpatialReference from, ISpatialReference to);

        /*
        int FromID { get ; }
        int ToID { get ; }
        */

        object Transform2D(object geometry);
        object InvTransform2D(object geometry);

        void Release();
    }

    public interface ITopologicalOperation
    {
        IPolygon Buffer(double distance);
        void Clip(IEnvelope clipper);
        void Intersect(IGeometry geometry);
        void Difference(IGeometry geometry);
        void SymDifference(IGeometry geometry);
        void Union(IGeometry geometry);

        void Clip(IEnvelope clipper, out IGeometry result);
        void Intersect(IGeometry geometry, out IGeometry result);
        //void Difference(IGeometry geometry, out IGeometry result);
        //void SymDifference(IGeometry geometry, out IGeometry result);
        //void Union(IGeometry geometry, out IGeometry result);
    }

    public interface IDisplayPath : IGeometry
    {
        float Chainage { get; set; }

        gView.Framework.Symbology.IAnnotationPolygonCollision AnnotationPolygonCollision { get; set; }

        float Length { get; }
        global::System.Drawing.PointF? PointAt(float stat);

        void ChangeDirection();
    }

    public enum GmlVersion
    {
        v1 = 0,
        v2 = 2,
        v3 = 3
    }
}
