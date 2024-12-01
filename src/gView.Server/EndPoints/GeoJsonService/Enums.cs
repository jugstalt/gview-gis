using System;

namespace gView.Server.EndPoints.GeoJsonService;

[Flags]
public enum HttpMethod
{
    Get = 1,
    Post = 2,
    Put = 4,
    Delete = 8
}

public enum MapLayerVisibility
{
    Visible,
    Invisible,
    Include,
    Exclude,
}

public enum MapLayerVisibilityPattern
{
    Defaults,
    Normal,
    IncludeExclude
}
