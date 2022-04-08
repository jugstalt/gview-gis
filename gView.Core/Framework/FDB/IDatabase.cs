using gView.Framework.system;
using System;
using System.Threading.Tasks;

namespace gView.Framework.FDB
{
    public interface IDatabase : IErrorMessage, IDisposable
    {
        bool Create(string name);
        Task<bool> Open(string name);

        string LastErrorMessage { get; }
        Exception LastException { get; }
    }
}
