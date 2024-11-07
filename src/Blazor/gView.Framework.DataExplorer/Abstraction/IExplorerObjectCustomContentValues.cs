using System.Collections.Generic;

namespace gView.Framework.DataExplorer.Abstraction;

public interface IExplorerObjectCustomContentValues
{
    IDictionary<string, object?> GetCustomContentValues();
}
