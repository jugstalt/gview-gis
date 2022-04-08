using System;
using gView.Framework.system;

namespace gView.Framework.UI
{
    public interface IApplication : ILicense
    {
        event EventHandler OnApplicationStart;

        string Title { get; set; }

        void Exit();
    }

    
}