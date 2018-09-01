using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Framework.system;
using gView.Interoperability.ArcXML;
using gView.MapServer;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Mvc;

namespace gView.Server.Controllers
{
    public class ArcIMSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EsriMap(string cmd, string ServiceName)
        {
            return EsriMap(cmd, ServiceName, String.Empty);
        }

        [HttpPost]
        public IActionResult EsriMap(string cmd, string ServiceName, string content)
        {
            if (cmd == "ping")
            {
                return Result("gView MapServer Instance v" + gView.Framework.system.SystemVariables.gViewVersion.ToString(), "text/plain");
            }
            if (cmd == "getversion")
            {
                return Result(gView.Framework.system.SystemVariables.gViewVersion.ToString(), "text/plain");
            }
            if(cmd=="capabilities")
            {
                content = @"<?xml version=""1.0"" encoding=""UTF-8""?><ARCXML version=""1.1""><REQUEST><GET_SERVICE_INFO fields=""true"" envelope=""true"" renderer=""true"" extensions=""true"" /></REQUEST></ARCXML>";
            }

            var interperter = InternetMapServer.GetInterpreter(typeof(ArcXMLRequest));

            #region Request 

            if(String.IsNullOrEmpty(content) && Request.Body.CanRead)
            {
                MemoryStream ms = new MemoryStream();

                byte[] bodyData = new byte[1024];
                int bytesRead;
                while ((bytesRead = Request.Body.Read(bodyData, 0, bodyData.Length)) > 0)
                {
                    ms.Write(bodyData, 0, bytesRead);
                }
                content = Encoding.UTF8.GetString(ms.ToArray());
            }

            ServiceRequest serviceRequest = new ServiceRequest(ServiceName, content);
            serviceRequest.OnlineResource = InternetMapServer.OnlineResource;

            #endregion

            #region Security

            Identity identity = Identity.FromFormattedString(String.Empty);
            identity.HashedPassword = String.Empty;
            serviceRequest.Identity = identity;

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = new ServiceRequestContext(
                InternetMapServer.Instance,
                interperter,
                serviceRequest);

            InternetMapServer.ThreadQueue.AddQueuedThreadSync(interperter.Request, context);

            #endregion

            return Result(serviceRequest.Response, "text/xml");
        }

        public IActionResult Result(string response, string contentType)
        {
            ViewData["content-type"] = contentType;
            ViewData["data"] = Encoding.UTF8.GetBytes(response);

            return View("_binary");
        }
    }
}