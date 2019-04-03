using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Collections;
using gView.Framework.UI;

namespace gView.Framework.system.UI
{
    public class ExplorerObjectCompareByName : IComparer<gView.Framework.UI.IExplorerObject>
    {
        #region IComparer<IExplorerObject> Member

        public int Compare(gView.Framework.UI.IExplorerObject x, gView.Framework.UI.IExplorerObject y)
        {
            return string.Compare(x.Name.ToLower(), y.Name.ToLower());
        }

        #endregion
    }
}
