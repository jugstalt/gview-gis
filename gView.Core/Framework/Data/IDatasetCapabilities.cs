using System.Collections.Generic;

namespace gView.Framework.Data
{
    public interface IDatasetCapabilities
    {
        IEnumerable<string> SupportedSubFieldFunctions();
    }
}
