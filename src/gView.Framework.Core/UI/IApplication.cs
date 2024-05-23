using gView.Framework.Core.Common;
using System;

namespace gView.Framework.Core.UI
{
    public interface IApplication : ILicense
    {
        event EventHandler OnApplicationStart;

        string Title { get; set; }

        void Exit();
    }
}