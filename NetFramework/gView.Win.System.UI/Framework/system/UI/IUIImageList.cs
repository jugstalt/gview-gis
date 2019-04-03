using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace gView.Framework.system.UI
{
    public interface IUIImageList
    {
        Image this[int index] {get;}
    }
}
