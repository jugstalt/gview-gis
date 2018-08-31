using System;
using System.IO;

namespace Proj4Net.Datum.Grids
{
    internal class GsbGridTableLoader : GridTableLoader
    {
        private readonly long _dataOffset;

        internal GsbGridTableLoader(Uri location) 
            : base(location)
        {
        }

        internal GsbGridTableLoader(Uri location, long dataOffset)
            : this(location)
        {
            _dataOffset = dataOffset;
        }

        /// <summary>
        /// Parses the grid table header and returns an appropriate <see cref="GridTable"/>
        /// </summary>
        /// <param name="table">The grid tabel to initialize</param>
        /// <returns>true if the header could be read.</returns>
        internal override bool ReadHeader(GridTable table)
        {
            var headerBuffer = new byte[176];
            using(var s = OpenGridTableStream())
            {
                s.Seek(_dataOffset, SeekOrigin.Current);
                if (s.Read(headerBuffer, 0, 176) != 176)
                    return false;
            }

            return _dataOffset == 0 
                       ? ReadOverviewHeader(headerBuffer, table) 
                       : ReadHeader(headerBuffer, table);
        }


        private bool ReadOverviewHeader(byte[] headerBuffer, GridTable table)
        {
            var numGrids = GetInt32(headerBuffer, 40);
            long offset = 176;
            for (var i = 0; i < numGrids; i++)
            {
                var sub = new GridTable(new GsbGridTableLoader(_uri, offset));
                offset += 176 + 2*sub.NumPhis*sub.NumLambdas*sizeof (double);
                table.AddSubGrid(sub);
            }
            return true;
        }

        private bool ReadHeader(byte[] headerBuffer, GridTable table)
        {
            var ll = new PhiLambda
                {
                    Phi = GetBigEndianDouble(headerBuffer, 4 * 16 + 8),
                    Lambda = -GetBigEndianDouble(headerBuffer, 7 * 16 + 8)
                };

            var ur = new PhiLambda
                {
                    Phi = GetBigEndianDouble(headerBuffer, 5 * 16 + 8),
                    Lambda = -GetBigEndianDouble(headerBuffer, 6 * 16 + 8)
                };

            var cs = new PhiLambda
                {
                    Phi = GetBigEndianDouble(headerBuffer, 8 * 16 + 8),
                    Lambda = -GetBigEndianDouble(headerBuffer, 9 * 16 + 8)
                };

            table.NumLambdas = (int)(Math.Abs(ur.Lambda - ll.Lambda) / cs.Lambda + .5) + 1;
            table.NumPhis = (int)(Math.Abs(ur.Phi - ll.Phi) / cs.Phi + .5) + 1;

            table.LowerLeft = PhiLambda.DegreesToRadians(ll);
            table.UpperRight = PhiLambda.DegreesToRadians(ur);
            table.SizeOfGridCell = PhiLambda.DegreesToRadians(cs);

            return true;
        }

        private static int GetInt32(byte[] buffer, int offset)
        {
            var tmp = new byte[4];
            Buffer.BlockCopy(buffer, offset, tmp, 0, 4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(tmp);
            return BitConverter.ToInt32(tmp, 0);
        }

        /// <summary>
        /// Parses the grid table data
        /// </summary>
        /// <param name="table">The table to fill</param>
        /// <returns>true if the data could be read.</returns>
        internal override bool ReadData(GridTable table)
        {
            using (var s = OpenGridTableStream())
            {
                s.Seek(_dataOffset + 176, SeekOrigin.Current);
                using (var br = new BinaryReader(s))
                {
                    var numPhis = table.NumPhis;
                    var coeffs = new PhiLambda[numPhis][];
                    var numLambdas = table.NumLambdas;
                    for (var row = 0; row < numPhis; row++)
                    {
                        coeffs[row] = new PhiLambda[numLambdas];
                        // NTV order is flipped compared with a normal CVS table
                        for (var col = numLambdas - 1; col >= 0; col--)
                        {
                            // shift values are given in "arc-seconds" and need to be converted to radians.
                            coeffs[row][col] =
                                PhiLambda.ArcSecondsToRadians(
                                    ReadBigEndianDouble(br), ReadBigEndianDouble(br));
                        }
                    }
                }
            }
            return true;
        }
    }
}