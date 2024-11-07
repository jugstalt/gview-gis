using gView.Framework.Core.Carto;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.GraphicsEngine;
using System;

namespace gView.Framework.Core.Data
{

    public delegate void AfterMapRequestEventHandler(IWebServiceClass sender, IDisplay display, GeorefBitmap image);

    
    

    public delegate void BeforeQueryEventHandler(ITableClass sender, ref IQueryFilter filter);

    public delegate void RowsAddedToTableEvent(int count);

    public delegate void PropertyChangedHandler();

    public delegate void BeforePointIdentifyEventHandler(object sender, IDisplay display, ref IPoint point, ref ISpatialReference sRef, IUserData userdata);

    
    public delegate void FeatureSelectionChangedEvent(IFeatureSelection sender);
    public delegate void BeforeClearSelectionEvent(IFeatureSelection sender);
}