using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Blazor.Core.Services.Abstraction;
public interface IScopeContextService
{
    IDictionary<string,string> RequestParameters { get; }
}
