using System.Collections.Generic;

namespace gView.Framework.Core.UI
{
    public interface IToolMenu : ITool
    {
        List<ITool> DropDownTools { get; }
        ITool SelectedTool { get; set; }
    }


}