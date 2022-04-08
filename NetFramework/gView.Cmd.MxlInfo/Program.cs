using gView.Framework.Data;
using gView.Framework.IO;
using gView.Framework.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Cmd.MxlInfo
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
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
                    if (map?.MapElements == null)
                    {
                        continue;
                    }

                    Console.WriteLine($"Map: { map.Name }");
                    Console.WriteLine("==========================================================");

                    var featureLayers = map.TOC.Layers.Where(l => l is IFeatureLayer)
                                                      .Select(l => (IFeatureLayer)l);

                    Console.WriteLine("FeatureLayers:");
                    Console.WriteLine("-------------------------------------------------------------");

                    if (map.Datasets != null)
                    {
                        int datasetID = 0;
                        foreach (var dataset in map.Datasets)
                        {
                            Console.WriteLine($"Dataset: { dataset.DatasetName }");
                            Console.WriteLine($"         { dataset.GetType().ToString() }");
                            Console.WriteLine("-------------------------------------------------------");

                            foreach (var dsElement in map.MapElements.Where(e => e.DatasetID == datasetID))
                            {
                                if (dsElement?.Class == null)
                                {
                                    continue;
                                }

                                var featureLayer = featureLayers.Where(l => l.DatasetID == datasetID && l.Class == dsElement.Class)
                                                                .FirstOrDefault();

                                if (featureLayer == null)
                                {
                                    continue;
                                }

                                Console.WriteLine($"FeatureLayer: { featureLayer.Title }");
                                Console.WriteLine($"       Class: { dsElement.Class.Name }");
                                Console.WriteLine($"              { dsElement.Class.GetType().ToString() }");
                            }
                        }
                    }
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