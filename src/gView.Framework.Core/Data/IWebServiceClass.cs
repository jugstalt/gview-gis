using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IWebServiceClass : IClass, IClone
    {
        event AfterMapRequestEventHandler AfterMapRequest;

        Task<bool> MapRequest(IDisplay display);
        Task<bool> LegendRequest(IDisplay display);
        GeorefBitmap Image { get; }
        GraphicsEngine.Abstraction.IBitmap Legend { get; }

        IEnvelope Envelope { get; }
        ISpatialReference SpatialReference { get; set; }
        List<IWebServiceTheme> Themes { get; }
    }
}