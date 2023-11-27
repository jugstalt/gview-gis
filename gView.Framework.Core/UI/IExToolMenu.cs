using System.Collections.Generic;

namespace gView.Framework.Core.UI
{
    public interface IExToolMenu : IExTool
    {
        List<ITool> DropDownTools { get; }
        IExTool SelectedTool { get; set; }
    }


}