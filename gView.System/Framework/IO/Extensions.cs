using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.IO
{
    static public class Extensions
    {
        static public string ToPlattformPath(this string path)
        {
            return path.Replace(@"\", "/");
        }

        async static public Task SaveOrUpload(this Bitmap bitmap, string path, ImageFormat format)
        {
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                string url = path.Substring(0, path.LastIndexOf("/"));
                string filename = path.Substring(path.LastIndexOf("/") + 1);

                Console.WriteLine($"upload {filename} to {url}");

                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, format);

                var file_bytes = ms.ToArray();

                using (var client = new HttpClient())
                {
                    HttpClient httpClient = new HttpClient();
                    MultipartFormDataContent form = new MultipartFormDataContent();

                    form.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), "file", filename);
                    HttpResponseMessage response = await httpClient.PostAsync(url, form);

                    response.EnsureSuccessStatusCode();
                    //string sd = response.Content.ReadAsStringAsync().Result;
                }
            }
            else
            {
                bitmap.Save(path, format);
            }
        }
    }
}
