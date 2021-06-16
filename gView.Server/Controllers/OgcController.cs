using gView.Framework.system;
using gView.Interoperability.OGC;
using gView.Interoperability.OGC.Request.WMTS;
using gView.MapServer;
using gView.Server.AppCode;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace gView.Server.Controllers
{
    public class OgcController : BaseController
    {
        private readonly MapServiceManager _mapServiceMananger;
        private readonly LoginManager _loginMananger;

        public OgcController(
            MapServiceManager mapServiceMananger,
            LoginManager loginManager,
            EncryptionCertificateService encryptionCertificateService)
            : base(mapServiceMananger, loginManager, encryptionCertificateService)
        {
            _mapServiceMananger = mapServiceMananger;
            _loginMananger = loginManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        async public Task<IActionResult> OgcRequest(string id, string service = "")
        {
            try
            {
                #region Security

                Identity identity = Identity.FromFormattedString(_loginMananger.GetAuthToken(this.Request).Username);

                #endregion

                IServiceRequestInterpreter interpreter = null;

                switch (service.ToLower().Split(',')[0])
                {
                    case "wms":
                        interpreter = _mapServiceMananger.GetInterpreter(typeof(WMSRequest));
                        break;
                    case "wfs":
                        interpreter = _mapServiceMananger.GetInterpreter(typeof(WFSRequest));
                        break;
                    case "wmts":
                        interpreter = _mapServiceMananger.GetInterpreter(typeof(WMTSRequest));
                        break;
                    default:
                        throw new Exception("Missing or unknow service: " + service);
                }

                #region Request

                string requestString = Request.QueryString.ToString();
                if (Request.Method.ToLower() == "post" && Request.Body.CanRead)
                {
                    string body = await GetBody();

                    if (!String.IsNullOrWhiteSpace(body))
                    {
                        requestString = body;
                    }
                }

                while (requestString.StartsWith("?"))
                {
                    requestString = requestString.Substring(1);
                }

                ServiceRequest serviceRequest = new ServiceRequest(id.ServiceName(), id.FolderName(), requestString)
                {
                    OnlineResource = _mapServiceMananger.Options.OnlineResource + "/ogc/" + id,
                    OutputUrl = _mapServiceMananger.Options.OutputUrl,
                    Identity = identity
                };

                #endregion

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                   _mapServiceMananger.Instance,
                   interpreter,
                   serviceRequest);


                await _mapServiceMananger.TaskQueue.AwaitRequest(interpreter.Request, context);

                return Result(serviceRequest.Response, serviceRequest.ResponseContentType);
            }
            catch (Exception ex)
            {
                // ToDo: OgcXmlExcpetion
                return Result(ex.Message, "text/plain");
            }
        }


        // https://localhost:44331/tilewmts/tor_tiles/compact/ul/31256/default/8/14099/16266.jpg
        async public Task<IActionResult> TileWmts(string name, string cachetype, string origin, string epsg, string style, string level, string row, string col, string folder = "")
        {
            if (IfMatch())
            {
                return base.NotModified();
            }

            #region Security

            Identity identity = Identity.FromFormattedString(_loginMananger.GetAuthToken(this.Request).Username);

            #endregion

            var interpreter = _mapServiceMananger.GetInterpreter(typeof(WMTSRequest));

            #region Request

            string requestString = cachetype + "/" + origin + "/" + epsg + "/" + style + "/~" + level + "/" + row + "/" + col;

            ServiceRequest serviceRequest = new ServiceRequest(name, folder, requestString)
            {
                OnlineResource = _mapServiceMananger.Options.OnlineResource + "/ogc/" + name,
                OutputUrl = _mapServiceMananger.Options.OutputUrl,
                Identity = identity
            };

            #endregion

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                   _mapServiceMananger.Instance,
                   interpreter,
                   serviceRequest);

            //await interpreter.Request(context);
            await _mapServiceMananger.TaskQueue.AwaitRequest(interpreter.Request, context);

            string ret = serviceRequest.Response;
            string contentType = col.Contains(".") ? "image/" + col.Split('.')[1] : "image/png";

            if (ret.StartsWith("image:"))
            {
                ret = ret.Substring(6, ret.Length - 6);
                return Result(ret, contentType);
            }
            if (ret.StartsWith("{"))
            {
                try
                {
                    var mapServerResponse = gView.Framework.system.MapServerResponse.FromString(ret);

                    if (mapServerResponse.Expires != null)
                    {
                        base.AppendEtag((DateTime)mapServerResponse.Expires);
                    }

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
            if (response.StartsWith("base64:"))
            {
                response = response.Substring("base64:".Length);
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
            if (String.IsNullOrEmpty(contentType))
                contentType = "text/plain";

            return File(data, contentType);
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
    }
}