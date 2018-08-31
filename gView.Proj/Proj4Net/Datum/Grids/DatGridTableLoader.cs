using System;
using System.IO;

namespace Proj4Net.Datum.Grids
{
    public class DatGridTableLoader : GridTableLoader
    {
        public DatGridTableLoader(Uri location) 
            : base(location)
        {
        }

        internal override bool ReadHeader(GridTable table)
        {
            using (var stream = OpenGridTableStream())
            {
                var header = new byte[176];
                if (stream.Read(header, 0, 176) != 176)
                    return false;

                table.LowerLeft = new PhiLambda
                    {
                        Phi = GetBigEndianDouble(header, 24),
                        Lambda = -GetBigEndianDouble(header, 72)
                    };

                table.UpperRight = new PhiLambda
                    {
                        Phi = GetBigEndianDouble(header, 40),
                        Lambda = -GetBigEndianDouble(header, 56)
                    };

                table.SizeOfGridCell = new PhiLambda
                    {
                        Phi = GetBigEndianDouble(header, 88),
                        Lambda = -GetBigEndianDouble(header, 104)
                    };

                var size = table.UpperRight - table.LowerLeft;
                table.NumLambdas = (int) (Math.Abs(size.Lambda)/table.SizeOfGridCell.Lambda + 0.5) + 1;
                table.NumPhis = (int)(Math.Abs(size.Phi) / table.SizeOfGridCell.Phi + 0.5) + 1;

                table.LowerLeft = PhiLambda.DegreesToRadians(table.LowerLeft);
                table.UpperRight = PhiLambda.DegreesToRadians(table.UpperRight);
                table.SizeOfGridCell = PhiLambda.DegreesToRadians(table.SizeOfGridCell);

                return true;
            }
        }

        internal override bool ReadData(GridTable table)
        {
            using (var br = new BinaryReader(OpenGridTableStream()))
            {
                br.BaseStream.Seek(176, SeekOrigin.Current);
                var coeffs = new PhiLambda[table.NumPhis][];
                for (var row = 0; row < table.NumPhis; row++)
                {
                    coeffs[row] = new PhiLambda[table.NumLambdas];

                    // NTV order is flipped compared with a normal CVS table
                    for (var col = table.NumLambdas - 1; col >= 0; col--)
                    {
                        const double degToArcSec = (Math.PI/180)/3600;
                        // shift values are given in "arc-seconds" and need to be converted to radians.
                        coeffs[row][col].Phi = ReadBigEndianDouble(br)*degToArcSec;
                        coeffs[row][col].Lambda = ReadBigEndianDouble(br)*degToArcSec;
                    }
                }
                table.Coefficients = coeffs;
                return true;
            }
        }
    }
}