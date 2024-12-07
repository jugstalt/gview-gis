namespace gView.GeoJsonService.DTOs;

// https://gist.github.com/sgillies/1233327
public class CoordinateReferenceSystem
{
    public string Type { get; set; } = "name";

    // name = urn:ogc:def:crs:OGC:1.3:CRS84
    // name = ...EPSG:4326
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    public string ToSpatialReferenceName()
    {
        if (this.Type.Equals("name", StringComparison.OrdinalIgnoreCase) == true)
        {
            var name = Properties.ContainsKey("name")
                ? Properties["name"]
                : "";

            if (name.Contains("epsg:", StringComparison.OrdinalIgnoreCase) == true)
            {
                return $"EPSG:{name.Substring(name.LastIndexOf(":") + 1)}";
            }
            else if("urn:ogc:def:crs:OGC:1.3:CRS84".Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return "EPSG:4326";
            }
        }

        return String.Empty;
    }

    static public CoordinateReferenceSystem CreateByName(string name)
    {
        var crs = new CoordinateReferenceSystem() { Type = "name" };

        crs.Properties["name"] = name;

        return crs;
    }
}
