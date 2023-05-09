using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerParentObject
{
    Task<List<IExplorerObject>> ChildObjects();
    Task<bool> Refresh();
    Task<bool> DiposeChildObjects();
}
