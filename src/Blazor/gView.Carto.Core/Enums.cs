using System;

namespace gView.Carto.Core;

[Flags]
public enum ToolType
{
    Userdefined = 0,
    Click = 1,
    BBox = 2,
    Sketch = 4,
}

public enum CartoToolTarget
{
    File,
    Map,
    SelectedTocItem,
    General,
    Tools
}
