using gView.Framework.system;
using System;

namespace gView.Framework.UI
{
    public interface IApplication : ILicense
    {
        event EventHandler OnApplicationStart;

        string Title { get; set; }

        void Exit();
    }
}