using gView.Framework.Common;
using gView.Framework.Core.MapServer;
using gView.Interoperability.Server.TileService;
using gView.Server.AppCode;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace gView.Server.Controllers;

/// <summary>
/// Vector tiles (MVT) of the tile service grid:
/// <c>vtc/[{folder}/]{name}/{exploded|compact}/{ul|ll}/{epsg}/{level}/{row}/{col}.pbf</c> and
/// <c>vtc/[{folder}/]{name}/{z}/{x}/{y}.pbf</c> (Mapbox conform, WebMercator only).
/// </summary>
public class VtcController : BaseController
{
    private readonly MapServiceManager _mapServiceMananger;
    private readonly LoginManager _loginMananger;

    public VtcController(
        MapServiceManager mapServiceMananger,
        LoginManager loginManager,
        EncryptionCertificateService encryptionCertificateService)
        : base(mapServiceMananger, loginManager, encryptionCertificateService)
    {
        _mapServiceMananger = mapServiceMananger;
        _loginMananger = loginManager;
    }

    public Task<IActionResult> Tile(string name, string cachetype, string origin, string epsg, string level, string row, string col, string folder = "")
        => VectorTile(name, folder, $"{cachetype}/{origin}/{epsg}/{level}/{row}/{col}");

    public Task<IActionResult> TileXyz(string name, string z, string x, string y, string folder = "")
        => VectorTile(name, folder, $"xyz/{z}/{x}/{y}");

    async private Task<IActionResult> VectorTile(string name, string folder, string requestString)
    {
        if (IfMatch())
        {
            return NotModified();
        }

        try
        {
            Identity identity = Identity.FromFormattedString(_loginMananger.GetAuthToken(this.Request).Username);

            var interpreter = _mapServiceMananger.GetInterpreter(typeof(VectorTileRequest));
            var serviceRequest = new ServiceRequest(name, folder, requestString)
            {
                OnlineResource = $"{_mapServiceMananger.Options.OnlineResource}/vtc/{name}",
                OutputUrl = _mapServiceMananger.Options.OutputUrl,
                Identity = identity
            };

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServiceMananger.Instance,
                interpreter,
                serviceRequest,
                checkSecurity: true);

            await _mapServiceMananger.TaskQueue.AwaitRequest(interpreter.Request, context);

            if (serviceRequest.Response is not byte[] data)
            {
                return BadRequest(serviceRequest.Response?.ToString());
            }

            if (data.Length == 0)
            {
                return NoContent();  // empty tile
            }

            if (serviceRequest.ResponseExpries.HasValue)
            {
                AppendEtag(serviceRequest.ResponseExpries.Value);
            }

            if (data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B)
            {
                Response.Headers.ContentEncoding = "gzip";
            }

            return File(data, serviceRequest.ResponseContentType);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
