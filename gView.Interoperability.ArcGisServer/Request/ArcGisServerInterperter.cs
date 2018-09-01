using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Interoperability.ArcGisServer.Rest.Json;
using gView.Interoperability.ArcGisServer.Rest.Json.Request;
using gView.Interoperability.ArcGisServer.Rest.Json.Response;
using gView.MapServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Request
{
    [gView.Framework.system.RegisterPlugIn("6702B376-8848-435B-8611-6516A9726D3F")]
    public class ArcGisServerInterperter : IServiceRequestInterpreter
    {
        private IMapServer _mapServer;
        private JsonExportMap _exportMap = null;

        #region IServiceRequestInterpreter

        public string IntentityName => "ags";

        public InterpreterCapabilities Capabilities =>
            new InterpreterCapabilities(new InterpreterCapabilities.Capability[]{
                    new InterpreterCapabilities.SimpleCapability("Emulating ArcGIS Server ",InterpreterCapabilities.Method.Post,"{server}/arcgis/rest/services/default/{service}/MapServer","1.0")
            });


        public void OnCreate(IMapServer mapServer)
        {
            _mapServer = mapServer;
        }

        public void Request(IServiceRequestContext context)
        {
            _exportMap = JsonConvert.DeserializeObject<JsonExportMap>(context.ServiceRequest.Request);
            using (var serviceMap = context.CreateServiceMapInstance())
            {
                #region Display

                serviceMap.Display.dpi = _exportMap.Dpi;

                var size= _exportMap.Size.ToSize();
                serviceMap.Display.iWidth = size[0];
                serviceMap.Display.iHeight = size[1];

                var bbox = _exportMap.BBox.ToBBox();
                serviceMap.Display.ZoomTo(new Envelope(bbox[0], bbox[1], bbox[2], bbox[3]));

                if (_exportMap.Transparent)
                {
                    serviceMap.Display.MakeTransparent = true;
                    serviceMap.Display.TransparentColor = System.Drawing.Color.White;
                }
                else
                {
                    serviceMap.Display.MakeTransparent = false;
                }

                #endregion

                var imageFormat = (ImageFormat)Enum.Parse(typeof(ImageFormat), _exportMap.ImageFormat);

                serviceMap.BeforeRenderLayers += ServiceMap_BeforeRenderLayers;
                serviceMap.Render();

                if (serviceMap.MapImage!=null)
                {
                    var iFormat = System.Drawing.Imaging.ImageFormat.Png;
                    if (imageFormat == ImageFormat.jpg)
                        iFormat = System.Drawing.Imaging.ImageFormat.Jpeg;

                    string fileName = serviceMap.Name.Replace(",", "_") + "_" + System.Guid.NewGuid().ToString("N") + "." + iFormat.ToString().ToLower();
                    string path = (_mapServer.OutputPath + @"/" + fileName).ToPlattformPath();
                    serviceMap.SaveImage(path, iFormat);

                    context.ServiceRequest.Succeeded = true;
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonExportResponse()
                    {
                        Href = _mapServer.OutputUrl + "/" + fileName,
                        Width = serviceMap.Display.iWidth,
                        Height = serviceMap.Display.iHeight,
                        ContentType = "image/" + iFormat.ToString().ToLower(),
                        Scale = serviceMap.Display.mapScale,
                        Extent = new JsonExtent()
                        {
                            Xmin = serviceMap.Display.Envelope.minx,
                            Ymin = serviceMap.Display.Envelope.miny,
                            Xmax = serviceMap.Display.Envelope.maxx,
                            Ymax = serviceMap.Display.Envelope.maxy
                            // ToDo: SpatialReference
                        }
                    });
                }
                else
                {
                    context.ServiceRequest.Succeeded = false;
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonError()
                    {
                        error = new JsonError.Error()
                        {
                            code = -1,
                            message = "No image data"
                        }
                    });
                }
            }
        }

        private void ServiceMap_BeforeRenderLayers(Framework.Carto.IServiceMap sender, List<Framework.Data.ILayer> layers)
        {
            if (String.IsNullOrWhiteSpace(_exportMap?.Layers) || !_exportMap.Layers.Contains(":"))
                return;

            string option = _exportMap.Layers.Substring(0, _exportMap.Layers.IndexOf(":")).ToLower();
            int[] layerIds = _exportMap.Layers.Substring(_exportMap.Layers.IndexOf(":") + 1)
                                    .Split(',').Select(l => int.Parse(l)).ToArray();

            foreach (var layer in layers)
            {
                switch(option)
                {
                    case "show":
                        layer.Visible = layerIds.Contains(layer.ID);
                        break;
                    case "hide":
                        layer.Visible = !layerIds.Contains(layer.ID);
                        break;
                    case "include":
                        if (layerIds.Contains(layer.ID))
                            layer.Visible = true;
                        break;
                    case "exclude":
                        if (layerIds.Contains(layer.ID))
                            layer.Visible = false;
                        break;
                }
                
            }
        }

        #endregion
    }
}
