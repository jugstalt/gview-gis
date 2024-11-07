using System;
using System.Collections.Generic;
using System.IO;

namespace gView.DataExplorer.Core.Models.Content;
public class TileBundleContent
{
    private const int MaxTilesPerRequest = 16;

    private readonly string _bundlePath;
    private readonly string _bundleIndexPath;

    public TileBundleContent(string bundlePath)
    {
        _bundlePath = bundlePath;
        _bundleIndexPath = bundlePath.Substring(0, bundlePath.LastIndexOf(".")) + ".tilebundlx";
    }

    public IEnumerable<int> Rows()
    {
        BundleIndex bundleIndex = new BundleIndex(_bundleIndexPath);
        Bundle bundle = new Bundle(_bundlePath);

        int startRow = bundle.StartRow;

        for (int row = 0; row < 128; row++)
        {
            for (int col = 0; col < 128; col++)
            {
                int tileLength;
                int tilePos = bundleIndex.TilePosition(row, col, out tileLength);

                if (tilePos >= 0 && tileLength >= 0)
                {
                    yield return (startRow + row);
                    break;
                }
            }
        }
    }

    public IDictionary<int, byte[]?> Images(int row, int fromCol=-1)
    {
        var result = new Dictionary<int, byte[]?>();

        BundleIndex bundleIndex = new BundleIndex(_bundleIndexPath);
        Bundle bundle = new Bundle(_bundlePath);

        int startRow = bundle.StartRow;
        int startCol = bundle.StartCol;

        int counter = 0;
        for (int col = (fromCol>=0 ? fromCol-startCol : 0); col < 128; col++)
        {
            int tileLength;
            int tilePos = bundleIndex.TilePosition(row - startRow, col, out tileLength);

            if (tilePos >= 0 && tileLength >= 0)
            {
                byte[] data = bundle.ImageData(tilePos, tileLength);

                result.Add(col + startCol, data);
                if (counter++ >= MaxTilesPerRequest)
                {
                    if(col<127) { result.Add(col + startCol + 1, null); }
                    break;
                }
            }
        }

        return result;
    }

    #region Classes

    class Bundle
    {
        public Bundle(string filename)
        {
            this.Filename = filename;
        }

        public string Filename { get; private set; }

        public byte[] ImageData(int pos, int length)
        {
            using (FileStream fs = new FileStream(this.Filename, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[length];
                fs.Position = pos;
                fs.Read(data, 0, data.Length);

                return data;
            }
        }

        public int StartRow
        {
            get
            {
                string fileTitle = (new FileInfo(this.Filename)).Name;

                string rHex = fileTitle.Substring(1, 8);
                int row = int.Parse(rHex, System.Globalization.NumberStyles.HexNumber);

                return row;
            }
        }

        public int StartCol
        {
            get
            {
                string fileTitle = (new FileInfo(this.Filename)).Name;

                string cHex = fileTitle.Substring(10, 8);
                int col = int.Parse(cHex, System.Globalization.NumberStyles.HexNumber);

                return col;
            }
        }
    }

    class BundleIndex
    {
        public BundleIndex(string filename)
        {
            this.Filename = filename;
        }

        public string Filename { get; private set; }

        public int TilePosition(int row, int col, out int tileLength)
        {
            if (row < 0 || row > 128 || col < 0 || col > 128)
            {
                throw new ArgumentException("Compact Tile Index out of range");
            }

            int indexPosition = ((row * 128) + col) * 8;

            using (FileStream fs = new FileStream(this.Filename, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[8];
                fs.Position = indexPosition;
                fs.Read(data, 0, 8);

                int position = BitConverter.ToInt32(data, 0);
                tileLength = BitConverter.ToInt32(data, 4);

                return position;
            }
        }
    }

    #endregion
}
