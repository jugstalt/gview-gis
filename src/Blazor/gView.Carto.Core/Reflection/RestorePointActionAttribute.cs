using System;

namespace gView.Carto.Core.Reflection;

public enum RestoreAction
{
    None,
    SetRestorePointOnClick
}

[AttributeUsage(AttributeTargets.Class)]
public class RestorePointActionAttribute(RestoreAction restoreAction) : Attribute
{
    public RestoreAction RestoreAction { get; } = restoreAction;
    public string? Description { get; }
}
