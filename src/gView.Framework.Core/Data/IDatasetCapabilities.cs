using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IDatasetCapabilities
    {
        IEnumerable<string> SupportedSubFieldFunctions();

        string CaseInsensitivLikeOperator { get; }
    }
}
