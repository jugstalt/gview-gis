using gView.Framework.Common;
using gView.Framework.Core.MapServer;
using gView.Interoperability.Server.TileService;
using gView.Server.AppCode;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Controllers;

/// <summary>
/// Vector tiles (MVT) of the tile service grid:
/// <c>vtc/[{folder}/]{name}/{exploded|compact}/{ul|ll}/{epsg}/{level}/{row}/{col}.pbf</c> and
/// <c>vtc/[{folder}/]{name}/{z}/{x}/{y}.pbf</c> (Mapbox conform, WebMercator only),
/// <c>vtc/[{folder}/]{name}/style.json[?epsg=]</c> (GL style) and <c>vtc/[{folder}/]{name}/tilejson.json[?epsg=]</c>.
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

    public Task<IActionResult> Style(string name, string epsg = "", string folder = "")
        => VectorTile(name, folder, $"style/{epsg}");

    public Task<IActionResult> TileJson(string name, string epsg = "", string folder = "")
        => VectorTile(name, folder, $"tilejson/{epsg}");

    /// <summary>
    /// <c>sprite.json</c>, <c>sprite.png</c>, <c>sprite@2x.json</c>, <c>sprite@2x.png</c>.
    /// </summary>
    public Task<IActionResult> Sprite(string name, string ratio, string ext, string folder = "")
        => VectorTile(name, folder, $"sprite/{(ratio == "@2x" ? 2 : 1)}/{(ext == "png" ? "png" : "json")}");

    /// <summary>
    /// <c>vtc-glyphs/{fontstack}/{start}-{end}.pbf</c>: glyph PBFs for MapLibre/Mapbox clients, rendered from the
    /// installed fonts and cached in the tile cache directory (<c>_glyphs</c>). Not bound to a service.
    /// </summary>
    async public Task<IActionResult> Glyphs(string fontstack, string range)
    {
        if (IfMatch())
        {
            return NotModified();
        }

        try
        {
            var match = System.Text.RegularExpressions.Regex.Match(range ?? "", @"^(\d+)-(\d+)(\.pbf)?$");
            if (!match.Success || String.IsNullOrWhiteSpace(fontstack) || fontstack.Length > 256)
            {
                return BadRequest("Invalid glyph request");
            }

            int start = int.Parse(match.Groups[1].Value);
            start -= start % 256;
            if (start < 0 || start > 65535)
            {
                return BadRequest("Invalid glyph range");
            }

            // cached per resolved (installed) font, not per requested stack: any number of
            // different font stacks cannot fill the disk
            var (family, style) = gView.Framework.Cartography.VectorTiles.GlyphPbfBuilder.ResolveFont(fontstack);
            string folder = gView.Framework.Cartography.VectorTiles.GlyphPbfBuilder.CacheName(family, style);
            var fi = new System.IO.FileInfo(System.IO.Path.Combine(
                _mapServiceMananger.Instance.TileCachePath, "_glyphs", folder, $"{start}-{start + 255}.pbf"));

            byte[] data;
            if (fi.Exists)
            {
                data = await System.IO.File.ReadAllBytesAsync(fi.FullName);
            }
            else
            {
                data = gView.Framework.Cartography.VectorTiles.GlyphPbfBuilder.Build(fontstack, start);

                fi.Directory?.Create();
                string temp = fi.FullName + "." + Guid.NewGuid().ToString("N") + ".tmp";
                await System.IO.File.WriteAllBytesAsync(temp, data);
                System.IO.File.Move(temp, fi.FullName, overwrite: true);
            }

            AppendEtag(DateTime.UtcNow.AddDays(30));
            return File(data, "application/x-protobuf");
        }
        catch (Exception ex)
        {
            return BadRequest(ToClientMessage(ex));
        }
    }

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
                OnlineResource = String.IsNullOrEmpty(folder)
                    ? $"{_mapServiceMananger.Options.OnlineResource}/vtc/{name}"
                    : $"{_mapServiceMananger.Options.OnlineResource}/vtc/{folder}/{name}",
                OutputUrl = _mapServiceMananger.Options.OutputUrl,
                Identity = identity
            };

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServiceMananger.Instance,
                interpreter,
                serviceRequest,
                checkSecurity: true);

            await _mapServiceMananger.TaskQueue.AwaitRequest(interpreter.Request, context);

            if (serviceRequest.Response is string json && serviceRequest.ResponseContentType == "application/json")
            {
                return Content(json, "application/json");
            }

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
            return BadRequest(ToClientMessage(ex));
        }
    }
}
