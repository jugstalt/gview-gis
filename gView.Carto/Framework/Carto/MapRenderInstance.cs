using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Carto
{
    public class MapRenderInstance : Map
    {
        private Map _original;
        private MapRenderInstance(Map original)
        {
            _original = original;
        }

        async static public Task<MapRenderInstance> CreateAsync(Map original)
        {
            var mapRenderInstance = new MapRenderInstance(original);

            mapRenderInstance._layers = original._layers;
            mapRenderInstance._datasets = original._datasets;

            mapRenderInstance.m_imageMerger = new ImageMerger2();

            mapRenderInstance.m_name = original.Name;
            mapRenderInstance._toc = original._toc;
            mapRenderInstance.Title = original.Title;

            //serviceMap._ceckLayerVisibilityBeforeDrawing = true;
            mapRenderInstance._mapUnits = original.MapUnits;
            mapRenderInstance._displayUnits = original.DisplayUnits;
            mapRenderInstance.refScale = original.refScale;

            mapRenderInstance.SpatialReference = original.Display.SpatialReference;
            mapRenderInstance.LayerDefaultSpatialReference = original.LayerDefaultSpatialReference != null ? original.LayerDefaultSpatialReference.Clone() as ISpatialReference : null;

            mapRenderInstance._drawScaleBar = false;

            // Metadata
            await mapRenderInstance.SetMetadataProviders(await original.GetMetadataProviders());
            mapRenderInstance._debug = false;

            mapRenderInstance._layerDescriptions = original.LayerDescriptions;
            mapRenderInstance._layerCopyrightTexts = original.LayerCopyrightTexts;

            mapRenderInstance.SetResourceContainer(original.ResourceContainer);

            mapRenderInstance.Display.iWidth = original.Display.iWidth;
            mapRenderInstance.Display.iHeight = original.Display.iHeight;
            mapRenderInstance.Display.ZoomTo(original.Envelope);
            mapRenderInstance.Display.dpi = original.Display.dpi;

            mapRenderInstance.DrawingLayer += (string layerName) =>
            {
                original.FireDrawingLayer(layerName);
            };
            mapRenderInstance.OnUserInterface += (sender, lockUI) =>
            {
                original.FireOnUserInterface(lockUI);
            };

            return mapRenderInstance;
        }

        public override void Dispose()
        {
            if (_original != null)
            {
                _original = null;
            }

            base.Dispose();
        }
    }
}
