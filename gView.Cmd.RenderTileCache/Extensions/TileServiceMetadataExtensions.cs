using gView.Framework.IO;
using gView.Framework.Metadata;
using System;
using System.Net.Http;

namespace gView.Cmd.RenderTileCache.Extensions
{
    static class TileServiceMetadataExtensions
    {
        public static TileServiceMetadata FromService(this TileServiceMetadata metadata, string server, string service)
        {
            var url = server.ToWmtsUrl(service, "GetMetadata");

            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"{ url } return with status code { response.StatusCode }");
                }

                var responseStream = response.Content.ReadAsStreamAsync().Result;
                XmlStream xmlStream = new XmlStream("WmtsMetadata");
                xmlStream.ReadStream(responseStream);

                return xmlStream.Load("TileServiceMetadata") as TileServiceMetadata;
            }
        }
    }
}
