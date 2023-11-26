#nullable enable

using System;

namespace gView.Framework.Reflection;

public class PropertyDescriptionAttribute : Attribute
{
    public bool AllowNull { get; set; } = false;
    public Type? DefaultInitializaionType { get; set; } = null;
    public Type? BrowsableRule { get; set; } = null;

    public float MinValue { get; set; } = 0f;
    public float MaxValue { get; set; } = 0f;
    public float RangeStep { get; set; } = 0f;
    public string LabelFormat { get; set; } = "";

    public (float min, float max, float step, string format)? Range =>
        MinValue < MaxValue
            ? ( MinValue, 
                MaxValue, 
                RangeStep <= 0f ? (MaxValue - MinValue) / 100f : RangeStep,
                String.IsNullOrEmpty(LabelFormat) ? "{0}" : LabelFormat
              )
            : null;
}