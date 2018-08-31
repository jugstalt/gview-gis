using System;
using System.IO;

namespace Proj4Net.Datum.Grids
{
    internal class LasLosGridTableLoader : GridTableLoader
    {
        public LasLosGridTableLoader(Uri location)
            : base(location)
        {
        }

        /// <summary>
        /// Parses the grid table header and returns an appropriate <see cref="GridTable"/>
        /// </summary>
        /// <param name="table">The grid tabel to initialize</param>
        /// <returns>true if the header could be read.</returns>
        internal override bool ReadHeader(GridTable table)
        {
            using(var br = new BinaryReader(OpenGridTableStream()))
            {
                br.BaseStream.Seek(64, SeekOrigin.Current);

                table.NumLambdas = br.ReadInt32();
                table.NumPhis = br.ReadInt32();
                br.ReadBytes(4);

                PhiLambda ll, cs;
                ll.Lambda = br.ReadSingle();
                cs.Lambda = br.ReadSingle();
                ll.Phi = br.ReadSingle();
                cs.Phi = br.ReadSingle();

                table.LowerLeft = PhiLambda.DegreesToRadians(ll);
                table.SizeOfGridCell = PhiLambda.DegreesToRadians(cs);
                table.UpperRight = ll + cs.Times(table.NumPhis, table.NumLambdas);

                return true;
            }
        }

        /// <summary>
        /// Parses the grid table data
        /// </summary>
        /// <param name="table">The table to fill</param>
        /// <returns>true if the data could be read.</returns>
        internal override bool ReadData(GridTable table)
        {
            using(var brLos = new BinaryReader(OpenGridTableStream()))
            using(var brLas = new BinaryReader(OpenLasStream()))
            {
                var numPhis = table.NumPhis;
                var coeffs = new PhiLambda[numPhis][];
                var numLambdas = table.NumLambdas;
                
                //position the stream
                var offset = sizeof (float)*(numLambdas + 1);
                brLas.BaseStream.Seek(offset, SeekOrigin.Current);
                brLos.BaseStream.Seek(offset, SeekOrigin.Current);
                    
                for (var i = 0; i < numPhis; i++)
                {
                    //Skip first 'zero' value
                    brLas.ReadBytes(sizeof(float));
                    brLos.ReadBytes(sizeof(float));

                    coeffs[i] = new PhiLambda[numLambdas];
                    coeffs[i][0] = PhiLambda.ArcSecondsToRadians(brLas.ReadSingle(), brLos.ReadSingle());
                    for (var j = 1; j < numLambdas; j++)
                    {
                        coeffs[i][j] = coeffs[i][j - 1] +
                                       PhiLambda.ArcSecondsToRadians(brLas.ReadSingle(), brLos.ReadSingle());
                    }
                    table.Coefficients = coeffs;
                }
                
                return true;
            }
        }

        private Stream OpenLasStream()
        {
            if (UriEx.IsFile(_uri))
            {
                var uriLas = _uri.LocalPath;
                uriLas = Path.ChangeExtension(uriLas, ".las");
                return File.OpenRead(uriLas);
            }
            throw new NotSupportedException();
        }
    }
}