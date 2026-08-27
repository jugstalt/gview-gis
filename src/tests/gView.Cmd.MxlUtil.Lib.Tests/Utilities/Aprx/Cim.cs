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
        CimEnvelope? defaultExtent = null,
        double? referenceScale = null,
        CimGeneralPlacementProperties? generalPlacementProperties = null) => new()
    {
        Name = name,
        LayerDefinitions = layers,
        SpatialReference = spatialReference,
        MapExtent = mapExtent,
        DefaultExtent = defaultExtent,
        ReferenceScale = referenceScale,
        GeneralPlacementProperties = generalPlacementProperties
    };

    public static CimGeneralPlacementProperties GeneralPlacementProperties(bool maplex) => new()
    {
        Type = maplex ? "CIMMaplexGeneralPlacementProperties" : "CIMStandardGeneralPlacementProperties"
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
        List<CimLabelClass>? labelClasses = null,
        double transparency = 0,
        bool scaleSymbols = true,
        CimSymbolReference? selectionSymbol = null) => new()
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
        LabelClasses = labelClasses,
        Transparency = transparency,
        ScaleSymbols = scaleSymbols,
        SelectionSymbol = selectionSymbol
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
        int serviceLayerId = 0,
        List<CimBaseLayer>? children = null) => new()
    {
        Name = name,
        Visibility = visibility,
        MinScale = minScale,
        MaxScale = maxScale,
        ServiceLayerId = serviceLayerId,
        LayerDefinitions = children
    };

    public static CimLabelClass LabelClass(
        string? expression = null,
        List<string>? fieldNames = null,
        CimSymbolReference? textSymbol = null,
        CimStandardLabelPlacementProperties? standardLabelPlacementProperties = null,
        CimMaplexLabelPlacementProperties? maplexLabelPlacementProperties = null) => new()
    {
        Expression = expression,
        FieldNames = fieldNames,
        TextSymbol = textSymbol,
        StandardLabelPlacementProperties = standardLabelPlacementProperties,
        MaplexLabelPlacementProperties = maplexLabelPlacementProperties
    };

    // Defaults to int.MaxValue (worst priority), not 0 - 0 is a legitimate "best" priority in
    // real CIM data, so leaving unspecified zones at 0 would make them accidentally tie for
    // first place instead of behaving like "not ranked".
    public static CimPointZonePriorities PointZonePriorities(
        int aboveLeft = int.MaxValue, int aboveCenter = int.MaxValue, int aboveRight = int.MaxValue,
        int centerLeft = int.MaxValue, int centerRight = int.MaxValue,
        int belowLeft = int.MaxValue, int belowCenter = int.MaxValue, int belowRight = int.MaxValue) => new()
    {
        AboveLeft = aboveLeft,
        AboveCenter = aboveCenter,
        AboveRight = aboveRight,
        CenterLeft = centerLeft,
        CenterRight = centerRight,
        BelowLeft = belowLeft,
        BelowCenter = belowCenter,
        BelowRight = belowRight
    };

    public static CimStandardLabelPlacementProperties StandardLabelPlacementProperties(
        string? numLabelsOption = null,
        string? pointPlacementMethod = "AroundPoint",
        CimPointZonePriorities? pointPlacementPriorities = null,
        CimStandardLineLabelPosition? lineLabelPosition = null,
        string? rotationField = null,
        string? rotationType = null,
        bool allowOverlappingLabels = false) => new()
    {
        NumLabelsOption = numLabelsOption,
        PointPlacementMethod = pointPlacementMethod,
        PointPlacementPriorities = pointPlacementPriorities,
        LineLabelPosition = lineLabelPosition,
        RotationField = rotationField,
        RotationType = rotationType,
        AllowOverlappingLabels = allowOverlappingLabels
    };

    public static CimStandardLineLabelPosition LineLabelPosition(
        bool above = false, bool inLine = false, bool below = false, bool parallel = true) => new()
    {
        Above = above,
        InLine = inLine,
        Below = below,
        Parallel = parallel
    };

    public static CimMaplexLabelPlacementProperties MaplexLabelPlacementProperties(
        string? pointPlacementMethod = "AroundPoint",
        CimPointZonePriorities? pointExternalZonePriorities = null) => new()
    {
        PointPlacementMethod = pointPlacementMethod,
        PointExternalZonePriorities = pointExternalZonePriorities
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
        bool enable = true,
        CimPoint2D? anchorPoint = null,
        string? anchorPointUnits = null,
        double offsetX = 0,
        double offsetY = 0) => new()
    {
        CharacterIndex = characterIndex,
        Size = size,
        FontFamilyName = fontFamilyName,
        Color = color,
        Rotation = rotation,
        Enable = enable,
        AnchorPoint = anchorPoint,
        AnchorPointUnits = anchorPointUnits,
        OffsetX = offsetX,
        OffsetY = offsetY
    };

    public static CimPoint2D Point2D(double x, double y) => new() { X = x, Y = y };

    public static CimGeometricEffectDashes Dashes(params double[] template) => new() { DashTemplate = [.. template] };

    public static CimTextSymbol TextSymbol(
        double height = 10,
        string? fontFamilyName = null,
        string? fontStyleName = null,
        CimSymbol? textFillSymbol = null,
        CimSymbol? haloSymbol = null,
        double haloSize = 0,
        CimCallout? callout = null) => new()
    {
        Height = height,
        FontFamilyName = fontFamilyName,
        FontStyleName = fontStyleName,
        TextFillSymbol = textFillSymbol,
        HaloSymbol = haloSymbol,
        HaloSize = haloSize,
        Callout = callout
    };

    public static CimCallout BalloonCallout(CimSymbol? backgroundSymbol, CimMargin? margin = null) => new()
    {
        Type = "CIMBalloonCallout",
        BackgroundSymbol = backgroundSymbol,
        Margin = margin
    };

    public static CimMargin Margin(double left, double right, double top, double bottom) => new()
    { Left = left, Right = right, Top = top, Bottom = bottom };

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
