using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.MxlUtil.Lib.Abstraction;
using gView.MxlUtil.Lib.Exceptions;
using gView.MxUtil.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
";
        }

        public string HelpText()
        {
            return
@"
";
        }

        async public Task Run(string[] args)
        {
            string inFile = String.Empty;
            string outFile = String.Empty;
            string command = "info";

            for (int i = 1; i < args.Length - 1; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-mxl":
                        inFile = args[++i];
                        break;
                    case "-out-mxl":
                        outFile = args[++i];
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

            switch(command)
            {
                case "info":
                    DatasetInfo(map);
                    break;
            }
        }

        #endregion

        private void DatasetInfo(Map map)
        {
            IDataset dataset = null;
            int index = 0;

            while((dataset = map[index])!=null)
            {
                Console.WriteLine($"Dataset {index}");
                Console.WriteLine("------------------------------------------------------------------------------");
                Console.WriteLine($"Type            : { dataset.GetType() }");
                Console.WriteLine($"ConnectionString: { dataset.ConnectionString }");
                Console.WriteLine(Environment.NewLine);

                index++;
            }
        }
    }
}
