#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace gView.Framework.Core.Reflection;

public class PropertyDescriptionAttribute : Attribute
{
    public bool AllowNull { get; set; } = false;
    public Type? DefaultInitializaionType { get; set; } = null;
    public Type? BrowsableRule { get; set; } = null;

    public float MinValue { get; set; } = 0f;
    public float MaxValue { get; set; } = 0f;
    public float RangeStep { get; set; } = 0f;
    public string LabelFormat { get; set; } = "";

    public string SelectOptionsPropertyName { get; set; } = "";

    public Type? EditorPropertyType { get; set; } = null;

    /// <summary>
    /// Comma-separated list of file extensions (e.g. ".svg" or ".png,.jpg,.jpeg")
    /// this property's editor should offer/accept. Currently used by
    /// <see cref="EditorPropertyType"/> == typeof(FileInfo) editors (e.g. the
    /// map-resource picker) to filter out irrelevant resources. Empty means
    /// "no filter".
    /// </summary>
    public string FileExtensions { get; set; } = "";

    public (float min, float max, float step, string format)? Range =>
        MinValue < MaxValue
            ? (MinValue,
                MaxValue,
                RangeStep <= 0f ? (MaxValue - MinValue) / 100f : RangeStep,
                string.IsNullOrEmpty(LabelFormat) ? "{0}" : LabelFormat
              )
            : null;
}