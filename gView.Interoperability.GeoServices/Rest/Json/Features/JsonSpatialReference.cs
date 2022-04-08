using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json.Features
{
    public class JsonSpatialReference
    {
        public JsonSpatialReference() { }

        public JsonSpatialReference(int id)
        {
            this.Wkid = id;
            //this.Wkt = "PROJCS[\"Austria_Gauss_Krueger_M34_Nord_5Mio\",GEOGCS[\"GCS_BESSEL_AUT\",DATUM[\"D_BESSEL_AUT\",SPHEROID[\"Bessel_1841\",6377397.155,299.1528128]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"false_easting\",0.0],PARAMETER[\"false_northing\",-5000000.0],PARAMETER[\"central_meridian\",16.33333333],PARAMETER[\"scale_factor\",1.0],PARAMETER[\"latitude_of_origin\",0.0],UNIT[\"Meter\",1.0]]";
        }

        [JsonProperty("wkt", NullValueHandling = NullValueHandling.Ignore)]
        public string Wkt { get; set; }

        [JsonProperty("wkid")]
        public int Wkid { get; set; }
    }
}
