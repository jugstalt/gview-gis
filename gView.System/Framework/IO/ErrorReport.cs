using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.IO
{
    public class ErrorReport : IErrorReport
    {
        #region IErrorReport 

        private ConcurrentBag<string> _warnings = new ConcurrentBag<string>();
        private ConcurrentBag<string> _errors = new ConcurrentBag<string>();


        public void AddWarning(string warning, object source)
        {
            if (source != null)
            {
                warning = $"{ warning } ({ source.ToString() })";
            }

            _warnings.Add($"Warning: { warning }");
        }
        public void AddError(string error, object source)
        {
            if (source != null)
            {
                error = $"{error} ({ source.ToString() })";
            }

            _errors.Add($"Error: { error  }");
        }

        public IEnumerable<string> Warnings { get { return _warnings.ToArray(); } }
        public IEnumerable<string> Errors { get { return _errors.ToArray(); } }

        public void ClearErrorsAndWarnings()
        {
            if (_warnings.Count > 0)
            {
                _warnings = new ConcurrentBag<string>();
            }

            if (_errors.Count > 0)
            {
                _errors = new ConcurrentBag<string>();
            }
        }

        #endregion
    }
}
