using gView.Framework.system;
using gView.Framework.UI.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace gView.Framework.UI
{
    //internal class CommandInterpregerArgs
    //{
    //    public string Command="";
    //    public ExplorerExecuteBatchCallback Callback = null;

    //    public CommandInterpregerArgs(string command)
    //    {
    //        Command = command;
    //    }
    //    public CommandInterpregerArgs(string command, ExplorerExecuteBatchCallback callback)
    //        : this(command)
    //    {
    //        Callback = callback;
    //    }
    //}

    internal class CommandInterpreter
    {
        private static Process _proc = null;
        public static IOutput Standardoutput = null;

        static public void AppendMenuItems(ContextMenuStrip strip, IExplorerObject ExplorerObject)
        {
            if (ExplorerObject == null)
            {
                return;
            }

            Guid ExplorerObjectGUID = PlugInManager.PlugInID(ExplorerObject);
            PlugInManager compManager = new PlugInManager();

            var commands = compManager.GetPlugins(Plugins.Type.IExplorerCommand);
            if (commands == null)
            {
                return;
            }

            foreach (var commandType in commands)
            {
                try
                {
                    IExplorerCommand command = compManager.CreateInstance<IExplorerCommand>(commandType);
                    if (command == null ||
                        (command.ExplorerObjectGUID != ExplorerObjectGUID && command.ExplorerObjectGUID != KnownExplorerObjectIDs.Any))
                    {
                        continue;
                    }

                    foreach (XmlNode node in command.CommandDefs)
                    {
                        if (node.Attributes["name"] == null)
                        {
                            continue;
                        }

                        ToolStripItem item = new CommandToolStripItem(ExplorerObject, node);
                        item.Click += new EventHandler(CommandItem_Click);

                        if (strip.Items.Count == 0)
                        {
                            strip.Items.Add(new ToolStripSeparator());
                        }

                        strip.Items.Add(item);
                    }
                }
                catch
                {
                }
            }
        }

        async static private void CommandItem_Click(object sender, EventArgs e)
        {
            if (_proc != null)
            {
                return;
            }

            if (!(sender is CommandToolStripItem) || ((CommandToolStripItem)sender).CommandDef == null)
            {
                return;
            }

            IExplorerObject exObject = ((CommandToolStripItem)sender).ExplorerObject;
            XmlNode commandDef = ((CommandToolStripItem)sender).CommandDef;
            FormCommandParameters dlg = new FormCommandParameters(exObject, commandDef);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //if (Standardoutput != null) Standardoutput.Text = "";

                await RunProcess(dlg.Command, null);
            }
        }

        async static internal Task RunProcess(string batch, ExplorerExecuteBatchCallback callback)
        {

            string path = SystemVariables.MyApplicationConfigPath + @"\explorer\tmp";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }

            string filename = di.FullName + @"\bat_" + Guid.NewGuid().ToString("N") + ".bat";
            StreamWriter sw = new StreamWriter(filename);
            String2DOS(sw.BaseStream, "echo off\n" + batch.ToString());
            sw.Close();

            Thread thread1 = new Thread(new ThreadStart(ReadProcessStandardOutput));
            Thread thread2 = new Thread(new ThreadStart(ReadProccessStandardError));

            _proc = new Process();
            _proc.StartInfo.FileName = filename;
            _proc.StartInfo.CreateNoWindow = true;
            _proc.StartInfo.RedirectStandardError = true;
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardInput = false;
            _proc.StartInfo.UseShellExecute = false;
            _proc.Start();

            thread1.Start();
            thread2.Start();

            _proc.WaitForExit();

            thread1.Abort();
            thread2.Abort();

            FileInfo fi = new FileInfo(filename);
            fi.Delete();

            _proc = null;

            if (callback != null)
            {
                var task = callback.Invoke();
                await task;
            }
        }

        static private void ReadProcessStandardOutput()
        {
            if (_proc == null)
            {
                return;
            }

            StreamReader sr = _proc.StandardOutput;

            char[] buffer = new char[128];
            int count = 0;
            ASCIIEncoding encoder = new ASCIIEncoding();
            while ((count = sr.Read(buffer, 0, 128)) != 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(buffer, 0, count);

                if (Standardoutput != null)
                {
                    lock (Standardoutput)
                    {
                        Standardoutput.Append2StandardOutput(sb.ToString());
                    }
                }
            }
            sr.Close();
        }
        static private void ReadProccessStandardError()
        {
            if (_proc == null)
            {
                return;
            }

            StreamReader sr = _proc.StandardError;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (Standardoutput != null)
                {
                    lock (Standardoutput)
                    {
                        Standardoutput.Append2StandardOutput("ERROR: " + line + "\r\n");
                    }
                }
            }
            sr.Close();
        }

        static private bool String2DOS(Stream stream, string str)
        {
            Encoding encoding = null;
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage == 850)   // imb850...Westeuropäisch(DOS)
                {
                    encoding = ei.GetEncoding();
                }
            }

            if (encoding == null)
            {
                return false;
            }

            byte[] bytes = encoding.GetBytes(str);
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(bytes);

            return true;
        }
    }

    internal class CommandToolStripItem : ToolStripMenuItem
    {
        XmlNode _commandNode;
        IExplorerObject _exObject;

        public CommandToolStripItem(IExplorerObject exObject, XmlNode commandNode)
        {
            _commandNode = commandNode;
            _exObject = exObject;

            if (_commandNode != null && _commandNode.Attributes["name"] != null)
            {
                base.Text = _commandNode.Attributes["name"].Value;
            }
            else
            {
                base.Text = "???";
            }
        }

        public XmlNode CommandDef
        {
            get { return _commandNode; }
        }

        public IExplorerObject ExplorerObject
        {
            get { return _exObject; }
        }
    }
}
