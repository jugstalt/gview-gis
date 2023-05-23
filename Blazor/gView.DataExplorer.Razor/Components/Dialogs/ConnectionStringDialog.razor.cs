using gView.Framework.Db;
using gView.Framework.system;
using System.Xml;

namespace gView.DataExplorer.Razor.Components.Dialogs;

public partial class ConnectionStringDialog
{
    private bool _xmlLoaded = false;
    private XmlDocument? _xmlConfig;
    private string? _connectionString;

    private void LoadXml()
    {
        if (_xmlLoaded)
        {
            return;
        }

        try
        {
            Type obj = typeof(CommonDbConnection);
            string configName = SystemVariables.ApplicationDirectory + @"\gView.DB.UI.xml";

            _xmlConfig = new XmlDocument();
            _xmlConfig.Load(configName);

            _providers.Clear();

            foreach (XmlNode provider in _xmlConfig.SelectNodes("//provider[@id]")!)
            {
                if (!String.IsNullOrEmpty(Model.ProviderId))
                {
                    bool found = false;
                    foreach (string providerID in Model.ProviderId.Replace(",", ";").Split(';'))
                    {
                        if (providerID.ToLower() == provider.Attributes?["id"]?.Value.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        continue;
                    }
                }

                if (provider.Attributes?["name"]?.Value == null ||
                    provider.Attributes?["id"]?.Value == null)
                {
                    continue;
                }

                _providers.Add(provider.Attributes["id"]!.Value!, provider.Attributes["name"]!.Value!);
            }
        }
        catch (Exception ex)
        {

        }
    }

    private string[]? Parameters(XmlNode ConnectionType)
    {
        if (ConnectionType == null)
        {
            return null;
        }

        string commandLine = _connectionString = ConnectionType.InnerText.Trim();
        int pos1 = 0, pos2;
        pos1 = commandLine.IndexOf("[");
        string parameters = "";

        while (pos1 != -1)
        {
            pos2 = commandLine.IndexOf("]", pos1);
            if (pos2 == -1)
            {
                break;
            }

            if (parameters != "")
            {
                parameters += ";";
            }

            parameters += commandLine.Substring(pos1 + 1, pos2 - pos1 - 1);
            pos1 = commandLine.IndexOf("[", pos2);
        }
        if (parameters != "")
        {
            return parameters.Split(';');
        }
        else
        {
            return null;
        }
    }
}
