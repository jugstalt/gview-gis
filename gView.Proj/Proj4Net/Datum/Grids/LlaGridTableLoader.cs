using System;
using System.Globalization;
using System.IO;
using Proj4Net.Utility;

namespace Proj4Net.Datum.Grids
{
    public class LlaGridTableLoader : GridTableLoader
    {
        public LlaGridTableLoader(Uri location)
            :base(location)
        {
        }

        internal override bool ReadHeader(GridTable table)
        {
            using (var sr = new StreamReader(OpenGridTableStream()))
            {
                table.Name = sr.ReadLine();
                var definition = sr.ReadLine();
                if (string.IsNullOrEmpty(definition))
                    return false;

                var definitionParts = definition.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                table.NumLambdas = int.Parse(definitionParts[0], NumberStyles.Integer);
                table.NumPhis = int.Parse(definitionParts[0], NumberStyles.Integer);
                table.LowerLeft = new PhiLambda
                    {
                        Lambda = ProjectionMath.ToRadians(double.Parse(definitionParts[3], CultureInfo.InvariantCulture)),
                        Phi = ProjectionMath.ToRadians(double.Parse(definitionParts[5], CultureInfo.InvariantCulture))
                    };
                table.SizeOfGridCell = new PhiLambda
                    {
                        Lambda = ProjectionMath.ToRadians(double.Parse(definitionParts[4], CultureInfo.InvariantCulture)),
                        Phi = ProjectionMath.ToRadians(double.Parse(definitionParts[6], CultureInfo.InvariantCulture))
                    };
                table.UpperRight = table.LowerLeft + 
                                   table.SizeOfGridCell.Times(table.NumPhis, table.NumLambdas);
                
                return true; 
            }
        }

        internal override bool ReadData(GridTable table)
        {
            using (var sr = new StreamReader(OpenGridTableStream()))
            {
                //Skip first 2 lines
                sr.ReadLine();
                sr.ReadLine();

                var phiIndex = -1;
                var lambdaIndex = 0;

                double phi = 0, lambda = 0;
                var coeff = new PhiLambda[table.NumPhis][];
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    var posColon = line.IndexOf(':');
                    string[] values;
                    int valueIndex;
                    if (posColon > 0)
                    {
                        phiIndex = int.Parse(line.Substring(0, posColon), NumberStyles.Integer);
                        coeff[phiIndex] = new PhiLambda[table.NumLambdas];
                        line = line.Substring(posColon + 1);
                        values = line.Split(new[] {' '});
                        lambda = ProjectionMath.ArcSecondsToRadians(long.Parse(values[0], NumberStyles.Integer));
                        phi = ProjectionMath.ArcSecondsToRadians(long.Parse(values[1], NumberStyles.Integer));
                        coeff[phiIndex][0].Phi = phi;
                        coeff[phiIndex][0].Lambda = lambda;
                        lambdaIndex = 1;
                        valueIndex = 2;
                    }
                    else
                    {
                        values = line.Split(new[] {' '});
                        valueIndex = 0;
                    }

                    if (phiIndex >= 0)
                    {
                        while (lambdaIndex < table.NumLambdas)
                        {
                            lambda += ProjectionMath.ArcMicroSecondsToRadians(
                                long.Parse(values[valueIndex++], NumberStyles.Integer));
                            phi += ProjectionMath.ArcMicroSecondsToRadians(
                                long.Parse(values[valueIndex++],NumberStyles.Integer));

                            coeff[phiIndex][lambdaIndex].Phi = phi;
                            coeff[phiIndex][lambdaIndex].Lambda = lambda;
                            lambdaIndex++;
                        }
                    }
                }
                table.Coefficients = coeff;
            }
            return true;
        }
    }
}