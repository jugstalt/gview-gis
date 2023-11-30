using gView.Framework.Core.system;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Core.FDB
{
    public interface IDatabase : IErrorMessage, IDisposable
    {
        bool Create(string name);
        Task<bool> Open(string name);

        Exception LastException { get; }
    }
}
