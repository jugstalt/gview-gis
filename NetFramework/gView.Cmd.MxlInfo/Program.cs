using gView.Framework.IO;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.MxlInfo
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            try
            {
                string inFile = args.FirstOrDefault();

                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "-??" && i < args.Length - 1)
                    {

                    }
                }

                if (String.IsNullOrWhiteSpace(inFile))
                {
                    Console.WriteLine("Usage: gView.Cmd.MxlInfo mxl-file [Options]");
                }

                XmlStream stream = new XmlStream("");
                stream.ReadStream(inFile);

                MapDocument doc = new MapDocument();
                await stream.LoadAsync("MapDocument", doc);

                foreach (var map in doc.Maps)
                {

                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);

                return 1;
            }
        }
    }
}
