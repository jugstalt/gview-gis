using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.Core.Abstraction;

public interface ICommand
{
    string Name { get; }
    string Description { get; }

    IEnumerable<ICommandParameterDescription> ParameterDescriptions { get; }

    Task<bool> Run(IDictionary<string, object> parameters, ICommandLogger? logger = null);
}
