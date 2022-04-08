namespace gView.Interoperability.GeoServices.Rest.Json.DynamicLayers
{
    class DynamicLayerSouce
    {
        public DynamicLayerSouce()
        {
            this.type = "mapLayer";
        }
        public string type { get; set; }

        public int mapLayerId { get; set; }
    }
}
