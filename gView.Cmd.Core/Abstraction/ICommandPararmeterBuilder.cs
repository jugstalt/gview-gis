using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.Core.Abstraction;
internal interface ICommandPararmeterBuilder
{
    IEnumerable<ICommandParameterDescription> Parameters { get; }

    Task<T> Build<T>(IDictionary<string, object> parameters); 
}
