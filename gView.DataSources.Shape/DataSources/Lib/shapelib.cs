using System;
using System.Text;
using System.Runtime.InteropServices;

namespace gView.DataSources.Shape.Lib
{
internal class ShapeLib
	{
		public enum ShapeType
		{
			/// <summary>Shape with no geometric data</summary>
			NullShape = 0,			
			/// <summary>2D point</summary>
			Point = 1,		
			/// <summary>2D polyline</summary>
			PolyLine = 3,			
			/// <summary>2D polygon</summary>
			Polygon = 5,		
			/// <summary>Set of 2D points</summary>
			MultiPoint = 8,	
			/// <summary>3D point</summary>
			PointZ = 11,		
			/// <summary>3D polyline</summary>
			PolyLineZ = 13,		
			/// <summary>3D polygon</summary>
			PolygonZ = 15,	
			/// <summary>Set of 3D points</summary>
			MultiPointZ = 18,	
			/// <summary>3D point with measure</summary>
			PointM = 21,		
			/// <summary>3D polyline with measure</summary>
			PolyLineM = 23,		
			/// <summary>3D polygon with measure</summary>
			PolygonM = 25,	
			/// <summary>Set of 3d points with measures</summary>
			MultiPointM = 28,	
			/// <summary>Collection of surface patches</summary>
			MultiPatch = 31
		}

		/// <summary>
		/// Part type enumeration - everything but ShapeType.MultiPatch just uses PartType.Ring.
		/// </summary>
		public enum PartType
		{
			/// <summary>
			/// Linked strip of triangles, where every vertex (after the first two) completes a new triangle.
			/// A new triangle is always formed by connecting the new vertex with its two immediate predecessors.
			/// </summary>
			TriangleStrip = 0,	
			/// <summary>
			/// A linked fan of triangles, where every vertex (after the first two) completes a new triangle.
			/// A new triangle is always formed by connecting the new vertex with its immediate predecessor 
			/// and the first vertex of the part.
			/// </summary>
			TriangleFan = 1,	
			/// <summary>The outer ring of a polygon</summary>
			OuterRing = 2,	
			/// <summary>The first ring of a polygon</summary>
			InnerRing = 3,	
			/// <summary>The outer ring of a polygon of an unspecified type</summary>
			FirstRing = 4,	
			/// <summary>A ring of a polygon of an unspecified type</summary>
			Ring = 5
		}
		
		/// <summary>
		/// SHPObject - represents on shape (without attributes) read from the .shp file.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal class SHPObject 
		{	
			///<summary>Shape type as a ShapeType enum</summary>	
			public ShapeType shpType;	
			///<summary>Shape number (-1 is unknown/unassigned)</summary>	
			public int nShapeId;	
			///<summary>Number of parts (0 implies single part with no info)</summary>	
			public int nParts;	
			///<summary>Pointer to int array of part start offsets, of size nParts</summary>	
			public IntPtr paPartStart;
			///<summary>Pointer to PartType array (PartType.Ring if not ShapeType.MultiPatch) of size nParts</summary>	
			public IntPtr paPartType;	
			///<summary>Number of vertices</summary>	
			public int nVertices;	
			///<summary>Pointer to double array containing X coordinates</summary>	
			public IntPtr padfX;	
			///<summary>Pointer to double array containing Y coordinates</summary>		
			public IntPtr padfY;	
			///<summary>Pointer to double array containing Z coordinates (all zero if not provided)</summary>	
			public IntPtr padfZ;	
			///<summary>Pointer to double array containing Measure coordinates(all zero if not provided)</summary>	
			public IntPtr padfM;	
			///<summary>Bounding rectangle's min X</summary>	
			public double dfXMin;	
			///<summary>Bounding rectangle's min Y</summary>	
			public double dfYMin;	
			///<summary>Bounding rectangle's min Z</summary>	
			public double dfZMin;	
			///<summary>Bounding rectangle's min M</summary>	
			public double dfMMin;	
			///<summary>Bounding rectangle's max X</summary>	
			public double dfXMax;	
			///<summary>Bounding rectangle's max Y</summary>	
			public double dfYMax;	
			///<summary>Bounding rectangle's max Z</summary>	
			public double dfZMax;	
			///<summary>Bounding rectangle's max M</summary>	
			public double dfMMax;
		}
	}
}
