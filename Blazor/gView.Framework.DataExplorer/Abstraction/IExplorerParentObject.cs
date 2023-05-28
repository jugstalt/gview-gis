using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerParentObject
{
    Task<IEnumerable<IExplorerObject>> ChildObjects();
    Task<bool> Refresh();
    Task<bool> DiposeChildObjects();
}
