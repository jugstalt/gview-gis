using gView.Core.Framework.Exceptions;
using gView.Framework.system;
using gView.MapServer;
using gView.Server.AppCode;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Server.Controllers
{
    public class MapServerController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        async public Task<IActionResult> Catalog(string format)
        {
            try
            {
                //string user, pwd;
                //var request = Request(out user, out pwd);

                if (format == "xml")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<RESPONSE><SERVICES>");
                    foreach (var service in await InternetMapServer.Instance.Maps(null))
                    {
                        if (service.Type == MapServiceType.Folder)  // if accessable for current user... => ToDo!!!
                        {
                            await InternetMapServer.ReloadServices(service.Name);
                            foreach (var folderService in InternetMapServer.MapServices.Where(s => s.Folder == service.Name))
                            {
                                sb.Append("<SERVICE ");
                                sb.Append("NAME='" + folderService.Fullname + "' ");
                                sb.Append("name='" + folderService.Fullname + "' ");
                                sb.Append("type='" + folderService.Type.ToString() + "' ");
                                sb.Append("/>");
                            }
                        }
                        else if (service.Type == MapServiceType.MXL && String.IsNullOrWhiteSpace(service.Folder))
                        {
                            sb.Append("<SERVICE ");
                            sb.Append("NAME='" + service.Name + "' ");
                            sb.Append("name='" + service.Name + "' ");
                            sb.Append("type='" + service.Type.ToString() + "' ");
                            sb.Append("/>");
                        }
                    }
                    sb.Append("</SERVICES></RESPONSE>");

                    return Result(sb.ToString(), "text/xml");
                }
                else
                {
                    throw new Exception("Unknown format");
                }
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
            catch (Exception ex)
            {
                return WriteError(ex.Message);
            }
        }

        async public Task<IActionResult> MapRequest(string guid, string name, string folder)
        {
            try
            {
                if (IfMatch())
                {
                    return NotModified();
                }

                #region Security

                Identity identity = Identity.FromFormattedString(base.GetAuthToken().Username);

                #endregion

                if (!String.IsNullOrWhiteSpace(folder))
                {
                    name = folder + "/" + name;
                }

                //DateTime td = DateTime.Now;
                //Console.WriteLine("Start Map Request " + td.ToLongTimeString() + "." + td.Millisecond + " (" + name + ")");
                //System.Threading.Thread.Sleep(10000);

                //string user, pwd;
                //var request = Request(out user, out pwd);

                #region Request

                string input = await GetBody();
                if (String.IsNullOrEmpty(input))
                {
                    input = this.Request.QueryString.ToString();
                }

                if (input.StartsWith("?"))
                {
                    input = input.Substring(1);
                }

                ServiceRequest serviceRequest = new ServiceRequest(name.ServiceName(), name.FolderName(), input)
                {
                    OnlineResource = InternetMapServer.OnlineResource + "/MapRequest/" + guid + "/" + name,
                    Identity = identity
                };

                #endregion

                IServiceRequestInterpreter interpreter =
                    InternetMapServer.GetInterpreter(new Guid(guid));

                #region Queue & Wait

                IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                    InternetMapServer.Instance,
                    interpreter,
                    serviceRequest);

                await InternetMapServer.TaskQueue.AwaitRequest(interpreter.Request, context);

                #endregion

                string ret = serviceRequest.Response;

                return Result(ret, "text/xml");
            }
            catch (MapServerException mse)
            {
                return WriteError(mse.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        #region Manage

        [HttpPost]
        async public Task<IActionResult> AddMap(string name, string folder)
        {
            try
            {
                string input = await GetBody();

                var credentials = GetRequestCredentials();

                name = String.IsNullOrWhiteSpace(folder) ? name : folder + "/" + name;

                bool ret = await InternetMapServer.AddMap(name, input, credentials.user, credentials.password);

                return Result(ret.ToString(), "text/plain");
            }
            catch (MapServerException mse)
            {
                return WriteError(mse.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        [HttpPost]
        async public Task<IActionResult> RemoveMap(string name, string folder)
        {
            try
            {
                string user = String.Empty, pwd = String.Empty;
                // var request = Request(out user, out pwd);

                name = String.IsNullOrWhiteSpace(folder) ? name : folder + "/" + name;

                bool ret = await InternetMapServer.RemoveMap(name, user, pwd);

                return Result(ret.ToString(), "text/plain");
            }
            catch (MapServerException mse)
            {
                return WriteError(mse.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        async public Task<IActionResult> GetMetadata(string name, string folder)
        {
            try
            {
                string user = String.Empty, pwd = String.Empty;
                // var request = Request(out user, out pwd);

                name = String.IsNullOrWhiteSpace(folder) ? name : folder + "/" + name;

                string ret = await InternetMapServer.GetMetadata(name, user, pwd);

                return Result(ret, "text/xml");
            }
            catch (MapServerException mse)
            {
                return WriteError(mse.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        [HttpPost]
        async public Task<IActionResult> SetMetadata(string name, string folder)
        {
            try
            {
                string input = await GetBody();

                name = String.IsNullOrWhiteSpace(folder) ? name : folder + "/" + name;

                string user = String.Empty, pwd = String.Empty;
                // var request = Request(out user, out pwd);

                bool ret = await InternetMapServer.SetMetadata(name, input, user, pwd);

                return Result(ret.ToString(), "text/plain");
            }
            catch (MapServerException mse)
            {
                return WriteError(mse.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return WriteUnauthorized();
            }
        }

        #endregion

        #region Helper

        private IActionResult WriteUnauthorized()
        {
            return WriteError("Unauthorized");
        }

        private IActionResult WriteError(string message)
        {
            return Result("<ERROR>" + message + "</ERROR>", "text/xml");
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

        private IActionResult Result(string response, string contentType)
        {
            byte[] data = null;
            if (response.StartsWith("base64:"))
            {
                response = response.Substring("base64:".Length);
                if (response.Contains(":"))
                {
                    int pos = response.IndexOf(":");
                    contentType = response.Substring(0, pos);
                    response = response.Substring(pos + 1);
                }
                data = Convert.FromBase64String(response);
            }
            else if (response.StartsWith("{"))
            {
                try
                {
                    var mapServerResponse = gView.Framework.system.MapServerResponse.FromString(response);

                    if (mapServerResponse.Expires != null)
                    {
                        AppendEtag((DateTime)mapServerResponse.Expires);
                    }

                    return Result(mapServerResponse.Data, mapServerResponse.ContentType);
                }
                catch { }
            }
            else
            {
                data = Encoding.UTF8.GetBytes(response);
            }
            return Result(data, contentType);
        }

        private IActionResult Result(byte[] data, string contentType)
        {
            //ViewData["content-type"] = contentType;
            //ViewData["data"] = data;

            //return View("_binary");
            return File(data, contentType);
        }

        private (string user, string password) GetRequestCredentials()
        {
            string user = String.Empty, password = String.Empty;
            string auth64 = this.Request.Headers["Authorization"];

            if (auth64 != null)
            {
                if (auth64.ToLower().StartsWith("basic "))
                {
                    auth64 = auth64.Substring("basic ".Length);
                }

                if (!String.IsNullOrEmpty(auth64))
                {
                    string auth = Encoding.ASCII.GetString(Convert.FromBase64String(auth64));
                    int index = auth.IndexOf(":");
                    if (index > 0)
                    {
                        user = auth.Substring(0, index);
                        password = auth.Substring(index + 1);
                    }
                }
            }

            return (user, password);
        }

        #region ETag

        private bool HasIfNonMatch()
        {
            return this.Request.Headers["If-None-Match"].ToString() != null;
        }

        private bool IfMatch()
        {
            try
            {
                if (HasIfNonMatch() == false)
                {
                    return false;
                }

                var etag = long.Parse(this.Request.Headers["If-None-Match"].ToString());

                DateTime etagTime = new DateTime(etag, DateTimeKind.Utc);
                if (DateTime.UtcNow > etagTime)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AppendEtag(DateTime expires)
        {
            this.Request.Headers.Add("ETag", expires.Ticks.ToString());
            this.Request.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));
            this.Request.Headers.Add("Expires", expires.ToString("R"));

            //this.Response.CacheControl = "private";
            //this.Response.Cache.SetMaxAge(new TimeSpan(24, 0, 0));
        }

        protected IActionResult NotModified()
        {
            Response.StatusCode = 304;
            return Content(String.Empty);
        }

        #endregion

        #endregion
    }
}