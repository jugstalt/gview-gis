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
    public class OgcController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult OgcRequest(string id, string service="")
        {
            try
            {
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

                ServiceRequest serviceRequest = new ServiceRequest(id, requestString);
                serviceRequest.OnlineResource = InternetMapServer.OnlineResource + "/ogc/" + id;

                #endregion

                #region Security

                Identity identity = Identity.FromFormattedString(String.Empty);
                identity.HashedPassword = String.Empty;
                serviceRequest.Identity = identity;

                #endregion

                IServiceRequestContext context = new ServiceRequestContext(
                   InternetMapServer.Instance,
                   interpreter,
                   serviceRequest);

                InternetMapServer.ThreadQueue.AddQueuedThreadSync(interpreter.Request, context);

                return Result(serviceRequest.Response, "text/xml");
            }
            catch (Exception ex)
            {
                // ToDo: OgcXmlExcpetion
                return Result(ex.Message, "text/plain");
            }
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