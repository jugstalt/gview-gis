using gView.Server.Services.MapServer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Server.Controllers
{
    public class OutputController : Controller
    {
        private readonly MapServiceManager _mapServerService;

        public OutputController(MapServiceManager mapServerService)
        {
            _mapServerService = mapServerService;
        }

        async public Task<IActionResult> Index(string id)
        {
            if (String.IsNullOrEmpty(_mapServerService.Options.OutputPath))
            {
                return null;
            }

            if (id.Contains("/") ||
               id.Contains("..") ||
               id.Contains("\\"))
            {
                return StatusCode(400);
            }

            var fileInfo = new FileInfo($"{ _mapServerService.Options.OutputPath }/{ id }");
            string contentType;
            switch (fileInfo.Extension.ToLower())
            {
                case ".png":
                    contentType = "image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".tif":
                case ".tiff":
                    contentType = "image/tiff";
                    break;
                case ".bmp":
                    contentType = "image/bmp";
                    break;
                default:
                    return StatusCode(403);
            }

            if (!fileInfo.Exists)
            {
                return StatusCode(404);
            }

            return File(await System.IO.File.ReadAllBytesAsync(fileInfo.FullName), contentType);
        }
    }
}
