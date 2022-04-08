using System;
using System.IO;

namespace gView.Cmd.CompactTileBundle
{
    class Program
    {
        public enum Action
        {
            None = 0,
            Explode = 1
        };

        static int Main(string[] args)
        {
            Action action = Action.None;

            string bundlePath = String.Empty;
            string target = String.Empty;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-explode")
                {
                    action = Action.Explode;
                }
                if (args[i] == "-bundle")
                {
                    bundlePath = args[++i];
                }
                if (args[i] == "-target")
                {
                    target = args[++i];
                }
            }

            if (String.IsNullOrWhiteSpace(bundlePath) || action == Action.None)
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.CompactTileBundle <-explode> -bundle <Filename>");
                Console.WriteLine("                      [-jpeg-qual <quality  0..100>]");
                return 1;
            }

            FileInfo bundleFile = new FileInfo(bundlePath);
            if (!bundleFile.Exists)
            {
                throw new ArgumentException("Bundle file not exits: " + bundleFile.FullName);
            }

            FileInfo bundlxFile = new FileInfo(bundleFile.FullName.Substring(0, bundleFile.FullName.Length - bundleFile.Extension.Length) + ".tilebundlx");
            if (!bundlxFile.Exists)
            {
                throw new ArgumentException("Bundle index file not exists: " + bundlxFile.FullName);
            }

            if (action == Action.Explode)
            {
                #region Explode

                Console.WriteLine("Explode Bundle: " + bundleFile.FullName);

                BundleIndex bundleIndex = new BundleIndex(bundlxFile.FullName);
                Bundle bundle = new Bundle(bundleFile.FullName);

                int startRow = bundle.StartRow;
                int startCol = bundle.StartCol;

                DirectoryInfo targetDirectory = new DirectoryInfo(String.IsNullOrWhiteSpace(target) ? bundleFile.Directory.FullName : target);

                for (int r = 0; r < 128; r++)
                {
                    DirectoryInfo rDir = new DirectoryInfo(targetDirectory.FullName + "/" + (r + startRow));

                    for (int c = 0; c < 128; c++)
                    {
                        int tileLength;
                        int tilePos = bundleIndex.TilePosition(r, c, out tileLength);

                        if (tilePos >= 0 && tileLength >= 0)
                        {
                            Console.WriteLine("Extract Tile: " + (r + startRow) + "/" + (c + startCol));

                            byte[] data = bundle.ImageData(tilePos, tileLength);

                            if (!rDir.Exists)
                            {
                                rDir.Create();
                            }

                            File.WriteAllBytes(rDir.FullName + @"/" + (c + startCol).ToString() + ".jpg", data);
                        }
                    }
                }

                #endregion
            }

            return 0;
        }
    }
}
