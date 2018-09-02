using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.system;
using gView.Interoperability.OGC;
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
                    default:
                        throw new Exception("Missing or unknow service: " + service);
                }

                #region Request

                string requestString =
                    Request.HasFormContentType ?
                    Request.Form.ToString() :
                    Request.QueryString.ToString();
                while (requestString.StartsWith("?"))
                    requestString = requestString.Substring(1);

                ServiceRequest serviceRequest = new ServiceRequest(id, requestString);
                serviceRequest.OnlineResource = InternetMapServer.OnlineResource;

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
            return Result(Encoding.UTF8.GetBytes(response), contentType);
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