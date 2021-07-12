using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gView.Core.Framework.Exceptions;
using gView.Framework.system;
using gView.Interoperability.ArcXML;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.MapServer;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace gView.Server.Controllers
{
    public class ArcIMSController : BaseController
    {
        private readonly MapServiceManager _mapServerService;

        public ArcIMSController(
            MapServiceManager mapServerService,
            LoginManager loginManagerService,
            EncryptionCertificateService encryptionCertificateService)
            : base(mapServerService, loginManagerService, encryptionCertificateService)
        {
            _mapServerService = mapServerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public Task<IActionResult> EsriMap(string cmd, string ServiceName)
        {
            return EsriMap(cmd, ServiceName, String.Empty);
        }

        [HttpPost]
        async public Task<IActionResult> EsriMap(string cmd, string ServiceName, string content)
        {
            return await SecureMethodHandler(async (identity) =>
            {
                if (cmd == "ping")
                {
                    return Result("gView MapServer Instance v" + gView.Framework.system.SystemVariables.gViewVersion.ToString(), "text/plain");
                }
                if (cmd == "getversion")
                {
                    return Result(gView.Framework.system.SystemVariables.gViewVersion.ToString(), "text/plain");
                }
                if (cmd == "capabilities")
                {
                    content = @"<?xml version=""1.0"" encoding=""UTF-8""?><ARCXML version=""1.1""><REQUEST><GET_SERVICE_INFO fields=""true"" envelope=""true"" renderer=""true"" extensions=""true"" /></REQUEST></ARCXML>";
                }

                var interpreter = _mapServerService.GetInterpreter(typeof(ArcXMLRequest));

                #region Request 

                if(String.IsNullOrEmpty(content))
                {
                    content = await GetBody();
                }

                ServiceRequest serviceRequest = new ServiceRequest(ServiceName.ServiceName(), ServiceName.FolderName(), content)
                {
                    Identity = identity,
                    OnlineResource = _mapServerService.Options.OnlineResource,
                    OutputUrl = _mapServerService.Options.OutputUrl,
                };

                #endregion

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    _mapServerService.Instance,
                    interpreter,
                    serviceRequest,
                    checkSecurity: ServiceName.ToLower() != "catalog");

                await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                return Result(serviceRequest.ResponseAsString, "text/xml");
            });
        }

        #region Helper

        private IActionResult Result(string response, string contentType)
        {
            //ViewData["content-type"] = contentType;
            //ViewData["data"] = Encoding.UTF8.GetBytes(response);

            //return View("_binary");
            return File(Encoding.UTF8.GetBytes(response), contentType);
        }

        private IActionResult ErrorResult(int code, string message)
        {
            string errorXML =
@"<ARCXML version=""1.1"">
<RESPONSE>
<ERROR machine="""" processid="""" threadid="""" >[ERR" + code + "] " + message + @"</ERROR>
</RESPONSE>
</ARCXML>";

            return Result(errorXML, "text/xml");
        }

        async private Task<string> GetBody()
        {
            if (Request.Body.CanRead)
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    return body;
                }
            }

            return String.Empty;
        }

        #endregion

        protected override Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
        {
            if (onException == null)
            {
                onException = (e) =>
                {
                    try
                    {
                        throw e;
                    }
                    catch (NotAuthorizedException nae)
                    {
                        return ErrorResult(403, nae.Message);
                    }
                    catch (TokenRequiredException tre)
                    {
                        return ErrorResult(499, tre.Message);
                    }
                    catch (InvalidTokenException ite)
                    {
                        return ErrorResult(498, ite.Message);
                    }
                    catch (MapServerException mse)
                    {
                        return ErrorResult(999, mse.Message);
                    }
                    catch (Exception)
                    {
                        return ErrorResult(999, "unknown error");
                    }
                };
            }

            return base.SecureMethodHandler(func, onException);
        }
    }
}