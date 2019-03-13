using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.system;
using gView.Interoperability.OGC;
using gView.Interoperability.OGC.Request.WMTS;
using gView.MapServer;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Mvc;

namespace gView.Server.Controllers
{
    public class OgcController : BaseController
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        async public Task<IActionResult> OgcRequest(string id, string service="")
        {
            try
            {
                #region Security

                Identity identity = Identity.FromFormattedString(base.GetAuthToken().Username);

                #endregion

                IServiceRequestInterpreter interpreter = null;
                
                switch(service.ToLower().Split(',')[0])
                {
                    case "wms":
                        interpreter = InternetMapServer.GetInterpreter(typeof(WMSRequest));
                        break;
                    case "wfs":
                        interpreter = InternetMapServer.GetInterpreter(typeof(WFSRequest));
                        break;
                    case "wmts":
                        interpreter = InternetMapServer.GetInterpreter(typeof(WMTSRequest));
                        break;
                    default:
                        throw new Exception("Missing or unknow service: " + service);
                }

                #region Request

                string requestString = Request.QueryString.ToString();

                if (Request.Method.ToLower()=="post" && Request.Body.CanRead)
                {
                    MemoryStream ms = new MemoryStream();

                    byte[] bodyData = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = Request.Body.Read(bodyData, 0, bodyData.Length)) > 0)
                    {
                        ms.Write(bodyData, 0, bytesRead);
                    }
                    string body = Encoding.UTF8.GetString(ms.ToArray());

                    if (!String.IsNullOrWhiteSpace(body))
                        requestString = body;
                }

                while (requestString.StartsWith("?"))
                    requestString = requestString.Substring(1);

                ServiceRequest serviceRequest = new ServiceRequest(id.ServiceName(), id.FolderName(), requestString)
                {
                    OnlineResource = InternetMapServer.OnlineResource + "/ogc/" + id,
                    Identity = identity
                };

                #endregion

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                   InternetMapServer.Instance,
                   interpreter,
                   serviceRequest);

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                return Result(serviceRequest.Response, "text/xml");
            }
            catch (Exception ex)
            {
                // ToDo: OgcXmlExcpetion
                return Result(ex.Message, "text/plain");
            }
        }


        // https://localhost:44331/tilewmts/tor_tiles/compact/ul/31256/default/8/14099/16266.jpg
        async public Task<IActionResult> TileWmts(string name, string cachetype, string origin, string epsg, string style, string level, string row, string col, string folder="")
        {
            //if (IfMatch())
            //{
            //    OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            //    context.StatusCode = HttpStatusCode.NotModified;
            //    return null;
            //}

            #region Security

            Identity identity = Identity.FromFormattedString(base.GetAuthToken().Username);

            #endregion

            var interpreter = InternetMapServer.GetInterpreter(typeof(WMTSRequest));

            #region Request

            string requestString = cachetype + "/" + origin + "/" + epsg + "/" + style + "/~" + level + "/" + row + "/" + col;

            ServiceRequest serviceRequest = new ServiceRequest(name, folder, requestString)
            {
                OnlineResource = InternetMapServer.OnlineResource + "/ogc/" + name,
                Identity = identity
            };

            #endregion

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                   InternetMapServer.Instance,
                   interpreter,
                   serviceRequest);

            await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

            string ret = serviceRequest.Response;
            string contentType = col.Contains(".") ? "image/" + col.Split('.')[1] : "image/png";

            if (ret.StartsWith("image:"))
            {
                //OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                //context.ContentType = "image/png";

                ret = ret.Substring(6, ret.Length - 6);
                return Result(ret, contentType);
            }
            if (ret.StartsWith("{"))
            {
                try
                {
                    var mapServerResponse = gView.Framework.system.MapServerResponse.FromString(ret);

                    //OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                    //context.ContentType = mapServerResponse.ContentType;

                    //if (mapServerResponse.Expires != null)
                    //    AppendEtag((DateTime)mapServerResponse.Expires);

                    return Result(mapServerResponse.Data, contentType);
                }
                catch { }
            }

            return null;
        }

        #region Helper

        private IActionResult Result(string response, string contentType)
        {
            byte[] data = null;
            if(response.StartsWith("base64:"))
            {
                response = response.Substring("base64:".Length);
                if(response.Contains(":"))
                {
                    int pos = response.IndexOf(":");
                    contentType = response.Substring(0, pos);
                    response = response.Substring(pos + 1);
                }
                data = Convert.FromBase64String(response);
            }
            else
            {
                data = Encoding.UTF8.GetBytes(response);
            }
            return Result(data, contentType);
        }

        private IActionResult Result(byte[] data, string contentType)
        {
            ViewData["content-type"] = contentType;
            ViewData["data"] = data;

            return View("_binary");
        }

        #endregion
    }
}