using System;
using System.Collections.Generic;

namespace gView.Server.Models;

/// <summary>
/// The test map of a service (admins only): the map reads the capabilities of the
/// chosen interface itself, like a client; the page only knows where to start
/// </summary>
public class BrowseServicesTestMapModel
{
    /// <summary>folder@service</summary>
    public string Id { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Folder { get; set; } = "";

    /// <summary>Anonymous users have access to the service</summary>
    public bool IsPublic { get; set; }

    /// <summary>EPSG code of the service map (display), 0 if unknown</summary>
    public int Epsg { get; set; }

    /// <summary>The interfaces the test map can drive, in the order of the selector</summary>
    public IEnumerable<TestMapInterface> Interfaces { get; set; } = Array.Empty<TestMapInterface>();
}

/// <param name="Key">identity name of the interpreter (wms, geoservices, geojsonservice)</param>
/// <param name="Title">label in the interface selector</param>
/// <param name="Url">base url of the interface for this service (no query string)</param>
public record TestMapInterface(string Key, string Title, string Url);
