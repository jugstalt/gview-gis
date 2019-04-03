using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.UI.Controls.Wizard
{
    public interface IWizardPageNotification
    {
        void OnShowWizardPage();
    }

    public interface IWizardPageNecessity
    {
        bool CheckNecessity();
    }
}
