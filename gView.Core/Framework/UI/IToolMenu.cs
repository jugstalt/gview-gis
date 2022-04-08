using System.Collections.Generic;

namespace gView.Framework.UI
{
    public interface IToolMenu : ITool
    {
        List<ITool> DropDownTools { get; }
        ITool SelectedTool { get; set; }
    }

    
}