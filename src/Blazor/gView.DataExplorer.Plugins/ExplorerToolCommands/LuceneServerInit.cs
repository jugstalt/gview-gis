using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("4D4C2699-43EB-4727-ADFB-20E508B794E8")]
internal class LuceneServerInit : IExplorerToolCommand
{
    public string Name => "LuceneServer.Init";

    public string ToolTip => "Init a LuceneServer.Fill Json config file";

    public string Icon => "";

    public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        return Task.FromResult(true);
    }
}
