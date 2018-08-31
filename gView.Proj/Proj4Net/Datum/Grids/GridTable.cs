using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Proj4Net.Utility;

namespace Proj4Net.Datum.Grids
{
    public class GridTable
    {
        private static readonly object GridTablesLock = new object();
        private static readonly Dictionary<Uri, GridTable> GridTables =
            new Dictionary<Uri, GridTable>(); 
        
        private const double HugeValue = double.MaxValue;
        private const int MaxIterations = 9;
        private const double Tolerance = 1E-12;
        
        /// <summary>
        /// Factory method to load a grid table
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static GridTable Load(Uri location)
        {
            GridTable table;
            if (!GridTables.TryGetValue(location, out table))
            {
                lock (GridTablesLock)
                {
                    if (!GridTables.TryGetValue(location, out table))
                    {
                        var ext = Path.GetExtension(location.LocalPath) ?? string.Empty;
                        GridTableLoader loader;
                        switch (ext.ToLower())
                        {
                            case "":
                            case ".lla":
                                loader = new LlaGridTableLoader(location);
                                break;
                            case ".gsb":
                                loader = new GsbGridTableLoader(location);
                                break;
                            case ".dat":
                                loader = new DatGridTableLoader(location);
                                break;
                            case ".los":
                                loader = new LasLosGridTableLoader(location);
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        table = new GridTable(loader);
                        GridTables.Add(location, table);
                    }
                }
            }
            return table;
        }

        private bool _fullyLoaded;
        private readonly GridTableLoader _loader;

        //Grid table name
        public string Name { get; internal set; }

        // Grid metrics
        internal PhiLambda LowerLeft;
        internal PhiLambda UpperRight;
        internal PhiLambda SizeOfGridCell;
        
        internal int NumPhis { get; set; }
        internal int NumLambdas{ get; set; }

        private readonly List<GridTable> _subGridTables = new List<GridTable>();
        private readonly object _lockReadData = new object();

        internal GridTable(GridTableLoader gridTableLoader) //, string name, int numPhis, int numLambdas, PhiLambda lowerLeft, PhiLambda sizeOfGridCell)
        {
            _loader = gridTableLoader;
            _loader.ReadHeader(this);
        }

        internal PhiLambda[][] Coefficients { get; set; }

        internal bool Applies(PhiLambda location, out GridTable table)
        {
            //Set inital return value
            table = null;

            //Test finer sub grid tables
            foreach (var subGridTable in _subGridTables)
            {
                if (subGridTable.Applies(location, out table))
                    return true;
            }

            if (location.Lambda < LowerLeft.Lambda || 
                location.Lambda > UpperRight.Lambda ||
                location.Phi < LowerLeft.Phi ||
                location.Phi > UpperRight.Phi) return false;

            table = this;
            return true;
        }

        internal Coordinate Apply(Coordinate geoCoord, bool inverse)
        {
            var input = new PhiLambda {Lambda = geoCoord.X, Phi = geoCoord.Y};
            if (input.Lambda == HugeValue)
                return geoCoord;

            if (!_fullyLoaded)
            {
                lock(_lockReadData)
                {
                    if (!_fullyLoaded)
                        _loader.ReadData(this);
                    _fullyLoaded = true;
                }
            }

            var output = Convert(input, inverse);
            
            geoCoord.X = output.Lambda;
            geoCoord.Y = output.Phi;

            return geoCoord;
        }

        private PhiLambda Convert(PhiLambda input, bool inverse)
        {
            var tb = input;
            tb.Lambda -= LowerLeft.Lambda;
            tb.Phi -= LowerLeft.Phi;
            tb.Lambda = ProjectionMath.NormalizeLongitude(tb.Lambda - Math.PI) + Math.PI;
            
            var t = InterpolateGrid(tb);
            if (inverse)
            {
                PhiLambda dif;
                int i = MaxIterations;
                if (t.Lambda == HugeValue) return t;
                t.Lambda = tb.Lambda + t.Lambda;
                t.Phi = tb.Phi - t.Phi;
                do
                {
                    var del = InterpolateGrid(t);
                    /* This case used to return failure, but I have
                       changed it to return the first order approximation
                       of the inverse shift.  This avoids cases where the
                       grid shift *into* this grid came from another grid.
                       While we aren't returning optimally correct results
                       I feel a close result in this case is better than
                       no result.  NFW
                       To demonstrate use -112.5839956 49.4914451 against
                       the NTv2 grid shift file from Canada. */
                    if (del.Lambda == HugeValue)
                    {
                        Debug.WriteLine("InverseShiftFailed");
                        break;
                    }
                    t.Lambda -= dif.Lambda = t.Lambda - del.Lambda - tb.Lambda;
                    t.Phi -= dif.Phi = t.Phi + del.Phi - tb.Phi;
                } while (i-- > 0 && Math.Abs(dif.Lambda) > Tolerance && Math.Abs(dif.Phi) > Tolerance);
                if (i < 0)
                {
                    Debug.WriteLine("InvShiftConvergeFailed");
                    t.Lambda = t.Phi = HugeValue;
                    return t;
                }
                input.Lambda = ProjectionMath.NormalizeLongitude(t.Lambda + LowerLeft.Lambda);
                input.Phi = t.Phi + LowerLeft.Phi;
            }
            else
            {
                if (t.Lambda == HugeValue)
                {
                    input = t;
                }
                else
                {
                    input.Lambda -= t.Lambda;
                    input.Phi += t.Phi;
                }
            }
            return input;
        }
        
        private PhiLambda InterpolateGrid(PhiLambda t)
        {
            PhiLambda result, remainder;
            result.Phi = HugeValue;
            result.Lambda = HugeValue;
            
            // find indices and normalize by the cell size (so fractions range from 0 to 1)
            var iLam = (int)Math.Floor(t.Lambda /= SizeOfGridCell.Lambda);
            var iPhi = (int)Math.Floor(t.Phi /= SizeOfGridCell.Phi);

            // use the index to determine the remainder
            remainder.Lambda = t.Lambda - iLam;
            remainder.Phi = t.Phi - iPhi;

            //int offLam = 0; // normally we look to the right and bottom neighbor cells
            //int offPhi = 0;
            //if (remainder.Lambda < .5) offLam = -1; // look to cell left of the current cell
            //if (remainder.Phi < .5) offPhi = -1; // look to cell above the of the current cell

            //// because the fractional weights are between cells, we need to adjust the
            //// "remainder" so that it is now relative to the center of the top left
            //// cell, taking into account that the definition of the top left cell
            //// depends on whether the original remainder was larger than .5
            //remainder.Phi = (remainder.Phi > .5) ? remainder.Phi - .5 : remainder.Phi + .5;
            //remainder.Lambda = (remainder.Lambda > .5) ? remainder.Lambda - .5 : remainder.Phi + .5;

            if (iLam < 0)
            {
                if (iLam == -1 && remainder.Lambda > 0.99999999999)
                {
                    iLam++;
                    remainder.Lambda = 0;
                }
                else
                {
                    return result;
                }
            }
            else if (iLam + 1 >= NumLambdas)
            {
                if (iLam + 1 == NumLambdas && remainder.Lambda < 1e-11)
                {
                    iLam--;
                }
                else
                {
                    return result;
                }
            }
            if (iPhi < 0)
            {
                if (iPhi == -1 && remainder.Phi > 0.99999999999)
                {
                    iPhi++;
                    remainder.Phi = 0;
                }
                else
                {
                    return result;
                }
            }
            else if (iPhi + 1 >= NumPhis)
            {
                if (iPhi + 1 == NumPhis && remainder.Phi < 1e-11)
                {
                    iPhi--;
                    remainder.Phi = 1;
                }
                else
                {
                    return result;
                }
            }

            var f00 = GetValue(iPhi, iLam);
            var f01 = GetValue(iPhi + 1, iLam);
            var f10 = GetValue(iPhi, iLam + 1);
            var f11 = GetValue(iPhi + 1, iLam + 1);

            // The cell weight is equivalent to the area of a cell sized square centered
            // on the actual point that overlaps with the cell.

            // Since the overlap must add up to 1, any portion that does not overlap
            // on the left must overlap on the right, hence (1-remainder.Lambda)
            var m00 = (1 - remainder.Lambda) * (1 - remainder.Phi);
            var m01 = (1 - remainder.Lambda) * remainder.Phi;
            var m10 = remainder.Lambda * (1 - remainder.Phi);
            var m11 = remainder.Lambda * remainder.Phi;

            result.Lambda = m00 * f00.Lambda + m01 * f01.Lambda + m10 * f10.Lambda + m11 * f11.Lambda;
            result.Phi = m00 * f00.Phi + m01 * f01.Phi + m10 * f10.Phi + m11 * f11.Phi;

            return result;
        }

        /// <summary>
        /// Checks the edges to make sure that we are not attempting to interpolate
        /// from cells that don't exist.
        /// </summary>
        /// <param name="iPhi">The cell index in the phi direction</param>
        /// <param name="iLam">The cell index in the lambda direction</param>
        /// <returns>A PhiLam that has the shift coefficeints.</returns>
        private PhiLambda GetValue(int iPhi, int iLam)
        {
            if (iPhi < 0) iPhi = 0;
            if (iPhi >= NumPhis) iPhi = NumPhis - 1;
            if (iLam < 0) iLam = 0;
            if (iLam >= NumLambdas) iLam = NumPhis - 1;
            return Coefficients[iPhi][iLam];
        }

        internal void AddSubGrid(GridTable subGridTable)
        {
            _subGridTables.Add(subGridTable);
        }
    }
}
