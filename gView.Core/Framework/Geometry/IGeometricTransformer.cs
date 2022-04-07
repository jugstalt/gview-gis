using System;

namespace gView.Framework.Geometry
{
    public interface IGeometricTransformer : IDisposable
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
}
