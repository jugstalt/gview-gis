using gView.Framework.Cartography;
using gView.Framework.Core.Data;
using gView.Framework.IO;
using gView.Cmd.MxlUtil.Lib.Abstraction;
using gView.Cmd.MxlUtil.Lib.Exceptions;
using gView.Cmd.MxUtil.Lib;
using gView.Cmd.Core.Abstraction;

namespace gView.Cmd.MxlUtil.Lib.Untilities
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

        async public Task<bool> Run(string[] args, ICommandLogger? logger = null)
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

            if(map is null)
            {
                throw new Exception("No map loaded");
            }

            if (map.HasErrorMessages)
            {
                throw new Exception($"Can't load source mxl {inFile}:{Environment.NewLine}{String.Join('\n', map.ErrorMessages)}");
            }

            bool saveOutput = false;
            switch (command)
            {
                case "info":
                    DatasetInfo(map, logger);
                    break;
                case "modify-connectionstring":
                case "modify-cs":
                    await ModifyConnectionString(map, datasetIndex, parameterNames, newParameterValues, logger);
                    saveOutput = true;
                    break;
                default:
                    throw new Exception($"Unkown command: {command}");
            }

            if (saveOutput)
            {
                stream = new XmlStream("");
                stream.Save("MapDocument", doc);

                logger?.LogLine($"Write: {outFile}");
                stream.WriteStream(outFile);
                logger?.LogLine("succeeded...");
            }

            return true;
        }

        #endregion

        private void DatasetInfo(Map map, ICommandLogger? logger)
        {
            IDataset? dataset = null;
            int index = 0;

            while ((dataset = map[index]) != null)
            {
                logger?.LogLine($"Dataset {index}");
                logger?.LogLine("==============================================================================");
                logger?.LogLine($"Type: {dataset.GetType()}");

                logger?.LogLine(Environment.NewLine);
                logger?.LogLine($"ConnectionString:");
                logger?.LogLine("------------------------------------------------------------------------------");

                var connectionParameters = ConfigTextStream.Extract(dataset.ConnectionString);
                foreach (string key in connectionParameters.Keys)
                {
                    logger?.LogLine($"{key}={ConfigTextStream.SecureConfigValue(key, connectionParameters[key])}");
                }

                if (!String.IsNullOrEmpty(dataset.LastErrorMessage))
                {
                    logger?.LogLine(Environment.NewLine);
                    logger?.LogLine("Errors:");
                    logger?.LogLine("------------------------------------------------------------------------------");
                    logger?.LogLine(dataset.LastErrorMessage);
                }

                logger?.LogLine(Environment.NewLine);

                index++;
            }
        }

        async private Task ModifyConnectionString(Map map, int dsIndex, List<string> parameterNames, List<string> newParameterValues, ICommandLogger? logger)
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
                        logger?.LogLine($"Set ConnectionString: {ConfigTextStream.SecureConfig(ConfigTextStream.Build(connectionParameters))}");
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
                IDataset? dataset = null;
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
