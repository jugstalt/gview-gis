#nullable enable

using System;

namespace gView.Framework.Reflection;

public class PropertyDescriptionAttribute : Attribute
{
    public bool AllowNull { get; set; } = false;
    public Type? DefaultInitializaionType { get; set; } = null;
    public Type? BrowsableRule { get; set; } = null;

    //public Func<object, bool>? IsBrowsable { get; set; }
}

