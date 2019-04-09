using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.UI.Controls.Wizard
{
    public interface IWizardPageNotification
    {
        Task OnShowWizardPage();
    }

    public interface IWizardPageNecessity
    {
        bool CheckNecessity();
    }
}
