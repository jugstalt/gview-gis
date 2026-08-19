using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Small factory helpers for building minimal CIM model object graphs to feed into
/// <c>AprxMapConverter.Convert</c>, without needing an actual .aprx file.
/// </summary>
internal static class Cim
{
    public static CimMap Map(
        string? name = null,
        List<CimBaseLayer>? layers = null,
        CimSpatialReference? spatialReference = null,
        CimEnvelope? mapExtent = null,
        CimEnvelope? defaultExtent = null) => new()
    {
        Name = name,
        LayerDefinitions = layers,
        SpatialReference = spatialReference,
        MapExtent = mapExtent,
        DefaultExtent = defaultExtent
    };

    public static CimSpatialReference SpatialReference(int wkid, int latestWkid = 0) => new()
    {
        Wkid = wkid,
        LatestWkid = latestWkid
    };

    public static CimEnvelope Envelope(double xmin, double ymin, double xmax, double ymax) => new()
    {
        XMin = xmin,
        YMin = ymin,
        XMax = xmax,
        YMax = ymax
    };

    public static CimFeatureLayer FeatureLayer(
        string? name = null,
        bool visibility = true,
        double minScale = 0,
        double maxScale = 0,
        int serviceLayerId = 0,
        string? definitionExpression = null,
        CimFeatureTable? featureTable = null,
        CimRenderer? renderer = null,
        bool labelVisibility = false,
        List<CimLabelClass>? labelClasses = null) => new()
    {
        Name = name,
        Visibility = visibility,
        MinScale = minScale,
        MaxScale = maxScale,
        ServiceLayerId = serviceLayerId,
        DefinitionExpression = definitionExpression,
        FeatureTable = featureTable,
        Renderer = renderer,
        LabelVisibility = labelVisibility,
        LabelClasses = labelClasses
    };

    public static CimFeatureTable FeatureTable(
        string? dataset = "MyTable",
        string? definitionExpression = null,
        List<CimFieldDescription>? fieldDescriptions = null,
        string? workspaceConnectionString = null) => new()
    {
        DataConnection = new CimDataConnection
        {
            Dataset = dataset,
            WorkspaceConnectionString = workspaceConnectionString
        },
        DefinitionExpression = definitionExpression,
        FieldDescriptions = fieldDescriptions
    };

    public static CimGroupLayer GroupLayer(
        string? name = null,
        bool visibility = true,
        double minScale = 0,
        double maxScale = 0,
        List<CimBaseLayer>? children = null) => new()
    {
        Name = name,
        Visibility = visibility,
        MinScale = minScale,
        MaxScale = maxScale,
        LayerDefinitions = children
    };

    public static CimLabelClass LabelClass(
        string? expression = null,
        List<string>? fieldNames = null,
        CimSymbolReference? textSymbol = null) => new()
    {
        Expression = expression,
        FieldNames = fieldNames,
        TextSymbol = textSymbol
    };

    public static CimSymbolReference SymbolRef(CimSymbol symbol) => new() { Symbol = symbol };

    // --- Colors ---

    public static CimRgbColor Rgb(double r, double g, double b, double alpha = 100) => new()
    { R = r, G = g, B = b, Alpha = alpha };

    public static CimCmykColor Cmyk(double c, double m, double y, double k, double alpha = 100) => new()
    { C = c, M = m, Y = y, K = k, Alpha = alpha };

    public static CimGrayColor Gray(double level, double alpha = 100) => new() { Level = level, Alpha = alpha };

    public static CimHsvColor Hsv(double h, double s, double v, double alpha = 100) => new()
    { H = h, S = s, V = v, Alpha = alpha };

    // --- Symbols ---

    public static CimPointSymbol PointSymbol(params CimSymbolLayer[] layers) => new() { SymbolLayers = [.. layers] };
    public static CimLineSymbol LineSymbol(params CimSymbolLayer[] layers) => new() { SymbolLayers = [.. layers] };
    public static CimPolygonSymbol PolygonSymbol(params CimSymbolLayer[] layers) => new() { SymbolLayers = [.. layers] };

    public static CimSolidFill SolidFill(CimColor? color, bool enable = true) => new() { Color = color, Enable = enable };

    public static CimSolidStroke SolidStroke(
        CimColor? color = null,
        double width = 1,
        List<CimGeometricEffect>? effects = null,
        bool enable = true) => new()
    { Color = color, Width = width, Effects = effects, Enable = enable };

    public static CimHatchFill HatchFill(double rotation, CimLineSymbol? lineSymbol = null, bool enable = true) => new()
    { Rotation = rotation, LineSymbol = lineSymbol, Enable = enable };

    public static CimPictureFill PictureFill(string? url = null, bool enable = true) => new() { Url = url, Enable = enable };

    public static CimCharacterMarker CharacterMarker(
        int characterIndex,
        double size = 10,
        string fontFamilyName = "ESRI Default Marker",
        CimColor? color = null,
        double rotation = 0,
        bool enable = true) => new()
    {
        CharacterIndex = characterIndex,
        Size = size,
        FontFamilyName = fontFamilyName,
        Color = color,
        Rotation = rotation,
        Enable = enable
    };

    public static CimGeometricEffectDashes Dashes(params double[] template) => new() { DashTemplate = [.. template] };

    public static CimTextSymbol TextSymbol(
        double height = 10,
        string? fontFamilyName = null,
        string? fontStyleName = null,
        CimSymbolReference? textFillSymbol = null,
        CimSymbol? haloSymbol = null,
        double haloSize = 0) => new()
    {
        Height = height,
        FontFamilyName = fontFamilyName,
        FontStyleName = fontStyleName,
        TextFillSymbol = textFillSymbol,
        HaloSymbol = haloSymbol,
        HaloSize = haloSize
    };

    // --- Renderers ---

    public static CimSimpleRenderer SimpleRenderer(
        CimSymbol symbol,
        string? label = null,
        List<CimVisualVariable>? visualVariables = null) => new()
    {
        Symbol = SymbolRef(symbol),
        Label = label,
        VisualVariables = visualVariables
    };

    public static CimUniqueValueRenderer UniqueValueRenderer(
        List<string> fields,
        List<CimUniqueValueGroup>? groups = null,
        CimSymbolReference? defaultSymbol = null,
        string? defaultLabel = null,
        bool useDefaultSymbol = false) => new()
    {
        Fields = fields,
        Groups = groups,
        DefaultSymbol = defaultSymbol,
        DefaultLabel = defaultLabel,
        UseDefaultSymbol = useDefaultSymbol
    };

    public static CimUniqueValueGroup UniqueValueGroup(params CimUniqueValueClass[] classes) => new()
    { Classes = [.. classes] };

    public static CimUniqueValueClass UniqueValueClass(
        CimSymbol symbol,
        string? label = null,
        bool visible = true,
        params string[] fieldValues) => new()
    {
        Label = label,
        Visible = visible,
        Symbol = SymbolRef(symbol),
        Values = [new CimUniqueValue { FieldValues = [.. fieldValues] }]
    };

    public static CimClassBreaksRenderer ClassBreaksRenderer(params CimClassBreak[] breaks) => new()
    { Breaks = [.. breaks] };

    public static CimClassBreak ClassBreak(double upperBound, CimSymbol symbol, string? label = null) => new()
    { UpperBound = upperBound, Symbol = SymbolRef(symbol), Label = label };

    public static CimRotationVisualVariable Rotation(string expression, string? rotationTypeZ = null) => new()
    {
        VisualVariableInfoZ = new CimVisualVariableInfo { Expression = expression },
        RotationTypeZ = rotationTypeZ
    };
}
