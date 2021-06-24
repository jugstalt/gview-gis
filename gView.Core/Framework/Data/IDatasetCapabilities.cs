using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data
{
    public interface IDatasetCapabilities
    {
        IEnumerable<string> SupportedSubFieldFunctions();
    }
}
