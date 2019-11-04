using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.Cmd.CompactTileBundle
{
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
}
