using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Cmd.MxlUtil.Lib.Abstraction;
using gView.Cmd.MxlUtil.Lib.Exceptions;
using gView.Cmd.MxlUtil.Lib.Untilities;
using gView.Cmd.MxlUtil.Lib.Utilities;
using gView.Framework.Core.Common;
using Mapbox.Vector.Tile;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using System.Xml;

namespace gView.Cmd.MxlUtil.Lib;
internal class MxlUtilCommand : ICommand
{
    private readonly IEnumerable<IMxlUtility> _mxlUtilities;

    public MxlUtilCommand()
    {
        _mxlUtilities =
            [
                new MxlDatasets(),
                new MxlToFdb(),
                new PublishService()
            ];
    }

    public string Name => "MxlUtil";

    public string Description => "Run a mxl uitilty";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
         [
            new RequiredCommandParameter<string>("u")
            {
                Description="name of the utility",
            }
        ];

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        var utilityName = parameters.GetValue<string>("u");

        var utility = _mxlUtilities.FirstOrDefault(u=>u.Name.Equals(utilityName, StringComparison.OrdinalIgnoreCase));
        if(utility is null)
        {
            logger?.LogLine("Usage -u {utility-name}");
            logger?.LogLine("");
            foreach (var mxlUtilitiy in _mxlUtilities)
            {
                logger?.LogLine($"{mxlUtilitiy.Description().Trim()}");
                logger?.LogLine("");
            }

            return false;
        }

        try
        {
            return await utility.Run(
                    parameters
                        .Where(p => p.Key != "u")
                        .SelectMany(p => new string[]{ $"-{p.Key}", p.Value.ToString() ?? "" })
                        .ToArray()
                        , 
                    logger
                );
        }
        catch (IncompleteArgumentsException)
        {
            logger?.LogLine(utility.HelpText().Trim());
            return false;
        }
    }
}
