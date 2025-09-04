using System;

namespace gView.Framework.Core.Carto
{
    [Flags]
    public enum DrawPhase
    {
        All = 15,
        Highlighing = 8,
        Geography = 4,
        Selection = 2,
        Graphics = 1,
        Empty = 0
    }

    public enum GeoUnits
    {
        Unknown = 0,
        Inches = 1,
        Feet = 2,
        Yards = 3,
        Miles = 4,
        NauticalMiles = 5,
        Millimeters = 6,
        Centimeters = 7,
        Decimeters = 8,
        Meters = 9,
        Kilometers = 10,
        DecimalDegrees = -1,
        DegreesMinutesSeconds = -2
    }

    public enum LabelAppendResult { Succeeded = 0, Overlap = 1, Outside = 2, WrongArguments = 3 }
    public enum GrabberMode { Pointer, Vertex }

    public enum LabelRenderMode { RenderWithFeature, UseRenderPriority }

    public enum WebMercatorScaleBehavior
    {
        Default = 0,
        IncludeLatitudeWhenCalculateMapScale = 1
    }

    public enum ErrorMessageType 
    { 
        Any = 0, 
        Warning = 1,
        Error = 2
    }
}
