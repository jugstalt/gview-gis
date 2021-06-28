using System;

namespace gView.GraphicsEngine
{
    public enum PixelFormat
    {
        DontCare = 0,
        Undefined = 0,
        Max = 15,
        Indexed = 65536,
        Gdi = 131072,
        Format16bppRgb555 = 135173,
        Format16bppRgb565 = 135174,
        Format24bppRgb = 137224,
        Format32bppRgb = 139273,
        Format1bppIndexed = 196865,
        Format4bppIndexed = 197634,
        Format8bppIndexed = 198659,
        Alpha = 262144,
        Format16bppArgb1555 = 397319,
        PAlpha = 524288,
        Format32bppPArgb = 925707,
        Extended = 1048576,
        Format16bppGrayScale = 1052676,
        Format48bppRgb = 1060876,
        Format64bppPArgb = 1851406,
        Canonical = 2097152,
        Format32bppArgb = 2498570,
        Format64bppArgb = 3424269
    }

    public enum ImageFormat
    {
        Bmp = 0,
        Gif = 1,
        Ico = 2,
        Jpeg = 3,
        Png = 4,
        Wbmp = 5,
        Webp = 6,
        Pkm = 7,
        Ktx = 8,
        Astc = 9,
        Dng = 10,
        Heif = 11
    }

    public enum CompositingMode
    {
        SourceOver = 0,
        SourceCopy = 1
    }

    public enum BitmapLockMode
    {
        ReadOnly = 1,
        WriteOnly = 2,
        ReadWrite = 3
    }

    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }

    public enum GraphicsUnit
    {
        World = 0,
        Display = 1,
        Pixel = 2,
        Point = 3,
        Inch = 4,
        Document = 5,
        Millimeter = 6
    }

    public enum InterpolationMode
    {
        Invalid = -1,
        Default = 0,
        Low = 1,
        High = 2,
        Bilinear = 3,
        Bicubic = 4,
        NearestNeighbor = 5,
        HighQualityBilinear = 6,
        HighQualityBicubic = 7
    }

    public enum SmoothingMode
    {
        Default = 0,
        None = 3,
        AntiAlias = 4
    }

    public enum TextRenderingHint
    {
        SystemDefault = 0,
        SingleBitPerPixelGridFit = 1,
        SingleBitPerPixel = 2,
        AntiAliasGridFit = 3,
        AntiAlias = 4,
        ClearTypeGridFit = 5
    }

    public enum LineDashStyle
    {
        Solid = 0,
        Dash = 1,
        Dot = 2,
        DashDot = 3,
        DashDotDot = 4,
        Custom = 5
    }

    public enum LineCap
    {
        Flat = 0,
        Square = 1,
        Round = 2,
        Triangle = 3,
        NoAnchor = 16,
        SquareAnchor = 17,
        RoundAnchor = 18,
        DiamondAnchor = 19,
        ArrowAnchor = 20,
        AnchorMask = 240,
        Custom = 255
    }

    public enum LineJoin
    {
        Miter = 0,
        Bevel = 1,
        Round = 2,
        MiterClipped = 3
    }

    public enum StringAlignment
    {
        Near = 0,
        Center = 1,
        Far = 2
    }
}
