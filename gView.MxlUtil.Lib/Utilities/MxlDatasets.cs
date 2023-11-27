using gView.Framework.Carto;
using gView.Framework.Core.Data;
using gView.Framework.IO;
using gView.MxlUtil.Lib.Abstraction;
using gView.MxlUtil.Lib.Exceptions;
using gView.MxUtil.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.MxlUtil.Lib.Untilities
{
    public class MxlDatasets : IMxlUtility
    {
        #region IMxlUtility

        public string Name => "MxlDatasets";

        public string Description()
        {
            return
@"
MxlDatasts
----------

Manage dataset connection in a mxl file.
";
        }

        public string HelpText()
        {
            return
@"
Required arguments:
-mxl <mxl-file>
-cmd <info|modify-cs|modify-connectionstring>

Optional arguments:
-out-xml <name/path of the out xml>

Commands:
info
----
Shows all dataset connections and connection string parameters in the mxl file.

modify-cs|modify-connectionstring
---------------------------------
Changes the value of an parameter in a connection  in the mxl file.

Required arguments:
-parameter|-parameter-name <name of the parameter>
-new-value|-new-parameter-value <set this value for the parameter>
-parameter|-parameter-name <next name of the parameter>
-new-value|-new-parameter-value <next set this value for the parameter>
...

Optional arguments:
-dsindex|-dataset-index <the index of the dataset you want change the parameter> default = -1 => all datasets

";
        }

        async public Task Run(string[] args)
        {
            string inFile = String.Empty;
            string outFile = String.Empty;
            string command = "info";

            int datasetIndex = -1;
            List<string> parameterNames = new List<string>();
            List<string> newParameterValues = new List<string>();

            for (int i = 1; i < args.Length - 1; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-cmd":
                        command = args[++i];
                        break;
                    case "-mxl":
                        inFile = args[++i];
                        break;
                    case "-out-mxl":
                        outFile = args[++i];
                        break;
                    case "-dsindex":
                    case "-dataset-index":
                        datasetIndex = int.Parse(args[++i]);
                        break;
                    case "-parameter-name":
                    case "-parameter":
                        parameterNames.Add(args[++i]);
                        break;
                    case "-new-value":
                    case "-new-parameter-value":
                        newParameterValues.Add(args[++i]);
                        break;
                }
            }

            if (String.IsNullOrEmpty(inFile))
            {
                throw new IncompleteArgumentsException();
            }

            XmlStream stream = new XmlStream("");
            stream.ReadStream(inFile);

            MxlDocument doc = new MxlDocument();
            await stream.LoadAsync("MapDocument", doc);

            var map = doc.Maps.FirstOrDefault() as Map;

            if (map.HasErrorMessages)
            {
                throw new Exception($"Can't load source mxl {inFile}:{Environment.NewLine}{String.Join('\n', map.ErrorMessages)}");
            }

            bool saveOutput = false;
            switch (command)
            {
                case "info":
                    DatasetInfo(map);
                    break;
                case "modify-connectionstring":
                case "modify-cs":
                    await ModifyConnectionString(map, datasetIndex, parameterNames, newParameterValues);
                    saveOutput = true;
                    break;
                default:
                    throw new Exception($"Unkown command: {command}");
            }

            if (saveOutput)
            {
                stream = new XmlStream("");
                stream.Save("MapDocument", doc);

                Console.WriteLine($"Write: {outFile}");
                stream.WriteStream(outFile);
                Console.WriteLine("succeeded...");
            }
        }

        #endregion

        private void DatasetInfo(Map map)
        {
            IDataset dataset = null;
            int index = 0;

            while ((dataset = map[index]) != null)
            {
                Console.WriteLine($"Dataset {index}");
                Console.WriteLine("==============================================================================");
                Console.WriteLine($"Type: {dataset.GetType()}");

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"ConnectionString:");
                Console.WriteLine("------------------------------------------------------------------------------");

                var connectionParameters = ConfigTextStream.Extract(dataset.ConnectionString);
                foreach (string key in connectionParameters.Keys)
                {
                    Console.WriteLine($"{key}={ConfigTextStream.SecureConfigValue(key, connectionParameters[key])}");
                }

                if (!String.IsNullOrEmpty(dataset.LastErrorMessage))
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine("Errors:");
                    Console.WriteLine("------------------------------------------------------------------------------");
                    Console.WriteLine(dataset.LastErrorMessage);
                }

                Console.WriteLine(Environment.NewLine);

                index++;
            }
        }

        async private Task ModifyConnectionString(Map map, int dsIndex, List<string> parameterNames, List<string> newParameterValues)
        {
            if (parameterNames.Count == 0 || newParameterValues.Count == 0 || parameterNames.Count != newParameterValues.Count)
            {
                throw new IncompleteArgumentsException();
            }

            foreach (var dataset in GetDatasets(map, dsIndex))
            {
                for (int p = 0; p < parameterNames.Count; p++)
                {
                    var connectionParameters = ConfigTextStream.Extract(dataset.ConnectionString);
                    bool modifyConnectionString = false;

                    foreach (string key in connectionParameters.Keys.ToArray())
                    {
                        if (key.Equals(parameterNames[p], StringComparison.InvariantCultureIgnoreCase))
                        {
                            connectionParameters[key] = newParameterValues[p];
                            modifyConnectionString = true;
                        }
                    }

                    if (modifyConnectionString)
                    {
                        Console.WriteLine($"Set ConnectionString: {ConfigTextStream.SecureConfig(ConfigTextStream.Build(connectionParameters))}");
                        if (!(await dataset.SetConnectionString(ConfigTextStream.Build(connectionParameters))) ||
                            !(await dataset.Open()))
                        {
                            throw new Exception($"Can't change connectionstring:\n{dataset.LastErrorMessage}");
                        }
                    }
                }
            }
        }

        #region Helper

        private IEnumerable<IDataset> GetDatasets(Map map, int index)
        {
            List<IDataset> datasets = new List<IDataset>();

            if (index >= 0)
            {
                datasets.Add(map[index]);
            }
            else
            {
                IDataset dataset = null;
                index = 0;

                while ((dataset = map[index++]) != null)
                {
                    datasets.Add(dataset);
                }
            }

            return datasets.Where(ds => ds != null);
        }

        #endregion
    }
}
