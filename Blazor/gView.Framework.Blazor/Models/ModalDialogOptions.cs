using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Blazor.Models;
public class ModalDialogOptions
{
    public bool ShowCloseButton { get; set; } = true;
    public bool CloseOnEscapeKey { get; set; } = false;
}
