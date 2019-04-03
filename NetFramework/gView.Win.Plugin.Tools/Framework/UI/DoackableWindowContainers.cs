using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.system;
using gView.Framework.UI;

namespace gView.Framework.UI
{
    [gView.Framework.system.RegisterPlugIn("843A3285-4465-45b8-BC7B-59CEBD992825")]
    public class DataTableContainer : IDockableWindowContainer
    {
        #region IDockableWindowContainer Members

        public string Name
        {
            get { return "Data Tables"; }
        }

        #endregion
    }
}
