using System.Collections.Generic;

namespace gView.Framework.Core.IO
{
    public interface IErrorReport
    {
        void AddWarning(string warning, object source);
        void AddError(string error, object source);

        IEnumerable<string> Warnings { get; }
        IEnumerable<string> Errors { get; }

        void ClearErrorsAndWarnings();
    }
}
