using gView.Blazor.Core.Exceptions;
using gView.Blazor.Core.Extensions;
using gView.Blazor.Core.Models;
using gView.Blazor.Core.Services;
using gView.Blazor.Core.Services.Abstraction;
using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Models;
using gView.Carto.Core.Models.ToolEvents;
using gView.Carto.Core.Models.Tree;
using gView.Carto.Core.Services;
using gView.Carto.Core.Services.Abstraction;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using gView.Framework.Blazor.Services;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.IO;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;

namespace gView.Carto.Plugins.Services;
public class CartoApplicationScopeService : ApplictionBusyHandlerAndCache, ICartoApplicationScopeService
{
    private readonly IDialogService _dialogService;
    private readonly IEnumerable<IKnownDialogService> _knownDialogs;
    private readonly CartoEventBusService _eventBus;
    private readonly CartoDisplayService _displayService;
    private readonly CartoDataTableService _dataTables;
    private readonly IJSRuntime _jsRuntime;
    private readonly ISnackbar _snackbar;
    private readonly CartoApplicationScopeServiceOptions _options;
    private readonly GeoTransformerService _geoTransformer;
    private readonly ICartoInteractiveToolService _toolService;
    private readonly SettingsService _settings;
    private readonly IAppIdentityProvider _identityProvider;
    private readonly PluginManagerService _pluginManager;
    private readonly ICartoRestoreService? _restoreService;
    private readonly IZoomHistory _zoomHistory;

    private ICartoDocument _cartoDocument;

    public CartoApplicationScopeService(IDialogService dialogService,
                                        IEnumerable<IKnownDialogService> knownDialogs,
                                        CartoEventBusService eventBus,
                                        CartoDisplayService displayService,
                                        CartoDataTableService dataTables,
                                        IJSRuntime jsRuntime,
                                        ISnackbar snackbar,
                                        GeoTransformerService geoTransformer,
                                        MapControlCrsService crsService,
                                        SpatialReferenceService sRefService,
                                        SettingsService settings,
                                        ICartoInteractiveToolService toolService,
                                        PluginManagerService pluginManager,
                                        IAppIdentityProvider identityProvider,
                                        IZoomHistory zoomHistory,
                                        IOptions<CartoApplicationScopeServiceOptions> options,
                                        IScopeContextService? scopeContext = null,
                                        ICartoRestoreService? restoreService = null
                                )
    {
        _dialogService = dialogService;
        _knownDialogs = knownDialogs;
        _eventBus = eventBus;
        _displayService = displayService;
        _dataTables = dataTables;
        _jsRuntime = jsRuntime;
        _snackbar = snackbar;
        _geoTransformer = geoTransformer;
        _settings = settings;
        _toolService = toolService;
        _options = options.Value;
        _pluginManager = pluginManager;
        _restoreService = restoreService;
        _zoomHistory = zoomHistory;

        _cartoDocument = this.Document = new CartoDocument(this);

        if (_cartoDocument.Map.Display.SpatialReference is null)
        {
            _cartoDocument.Map.Display.SpatialReference =
                sRefService.GetSpatialReference($"epsg:{crsService.GetDefaultOrAny().Epsg}").Result;
        }

        foreach (var tool in _toolService.ScopedTools)
        {
            tool.InitializeScope(this);
        }

        _eventBus.OnLayerSettingsChangedAsync += HandleLayerSettingsChangedAsync;
        _eventBus.OnCartoDocumentLoadedAsync += HandleCartoDocumentTouchedAsync;

        _eventBus.OnMapClickAsync += HandleMapClickAsync;
        _eventBus.OnMapBBoxAsync += HandleMapBBoxAsync;
        _eventBus.OnToolEventAsync += HandleToolEventAsync;
        _identityProvider = identityProvider;
    }

    public CartoEventBusService EventBus => _eventBus;

    public ICartoDocument Document
    {
        get
        {
            return _cartoDocument;
        }
        set
        {
            _cartoDocument = value;

            _eventBus.FireCartoDocumentLoadedAsync(value);
        }
    }

    public AppIdentity Identity => _identityProvider.Identity;

    #region Map/Carto Document

    public Task<bool> LoadCartoDocumentAsync(string mxlFilePath)
        => LoadCartoDocumentOrRestorePointAsync(mxlFilePath);

    async public Task<bool> SaveCartoDocumentAsync(string xmlFilePath, bool performEncryption)
    {
        XmlStream stream = new XmlStream("MapApplication", performEncryption);
        stream.Save("MapDocument", this.Document);

        stream.WriteStream(xmlFilePath);

        this.Document.FilePath = xmlFilePath;

        await HandleCartoDocumentTouchedAsync(Document);

        return true;
    }

    public bool SerializeCartoDocument(Stream stream)
    {
        XmlStream xmlStream = new XmlStream("MapApplication", true);
        xmlStream.Save("MapDocument", this.Document);

        xmlStream.WriteStream(stream, System.Xml.Formatting.Indented);

        return true;
    }

    async private Task<bool> LoadCartoDocumentOrRestorePointAsync(string mxlFilePath, string? restoreFilePath = null)
    {
        _zoomHistory.Clear();

        IMap? originalMap = null;

        if (Document?.Map is not null 
            && mxlFilePath.Equals(Document.FilePath))
        {
            originalMap = Document.Map;
        }
        else
        {
            try
            {
                XmlStream originalMapStream = new XmlStream("");
                originalMapStream.ReadStream(mxlFilePath);

                var originalCartoDocument = new CartoDocument(null);
                await originalMapStream.LoadAsync("MapDocument", originalCartoDocument);

                originalMap = originalCartoDocument.Map;
            }
            catch { }
        }

        XmlStream stream = new XmlStream("");
        stream.ReadStream(restoreFilePath ?? mxlFilePath);

        var cartoDocument = new CartoDocument(this)
        {
            FilePath = mxlFilePath
        };

        await stream.LoadAsync("MapDocument", cartoDocument);

        if (cartoDocument.Map?.ErrorMessages?.Any() == true)
        {
            if (await this.ShowKnownDialog(
                                KnownDialogs.WarningsDialog,
                                model: new WarningsDialogModel()
                                {
                                    Warnings = cartoDocument.Map.ErrorMessages
                                })
                is null)
            {
                return false;
            }
        }

        if (cartoDocument.Map?.Display is null) throw new Exception("Restore point do not contain a valid map");

        if (originalMap?.Display is not null)
        {
            cartoDocument.Map.Display.ImageWidth = originalMap.Display.ImageWidth;
            cartoDocument.Map.Display.ImageHeight = originalMap.Display.ImageHeight;
            cartoDocument.Map.Display.ZoomTo(originalMap.Display.Envelope);
        }

        this.Document = cartoDocument;

        return true;
    }

    #endregion

    #region Document Restore Points

    public void CreateRestorePoint(string description)
    {
        if (_restoreService is not null)
        {
            Task.Run(() => _restoreService.SetRestorePoint(this, description));
        }
    }

    public RestorePointState LatestRestorePointState(string? mxlPath = null)
    {
        mxlPath ??= Document?.FilePath;

        if (_restoreService is null || String.IsNullOrEmpty(mxlPath)) return RestorePointState.None;

        var restorePoint = _restoreService.GetRestorePoints(mxlPath, 1);
        var fileInfo = new FileInfo(mxlPath);

        return restorePoint.Count() switch
        {
            0 => RestorePointState.None,
            1 when fileInfo.LastWriteTimeUtc > restorePoint.First().timeUtc => RestorePointState.Older,
            _ => RestorePointState.Newer
        };
    }

    public IEnumerable<RestorePoint> RestorePoints(string? mxlPath = null)
        => _restoreService?
                .GetRestorePoints(mxlPath ?? Document?.FilePath ?? "")
                .Select(r => new RestorePoint()
                {
                    Hash = r.filePath.GenerateSHA1(),
                    Description = r.description,
                    TimeUtc = r.timeUtc
                }) ?? Array.Empty<RestorePoint>();

    public Task<bool> LoadRestorePointAsync(string mxlPath, string restorePointHash)
    {
        var restorePoint = _restoreService?
                .GetRestorePoints(mxlPath ?? Document?.FilePath ?? "")
                .Where(r => r.filePath.GenerateSHA1() == restorePointHash)
                .FirstOrDefault();

        if (restorePoint is null) throw new Exception($"Unknown restore point {restorePointHash}");

        return LoadCartoDocumentOrRestorePointAsync(mxlPath!, restorePoint.Value.filePath);
    }

    public RestoreResult RemoveRestorePoints(string mxlPath)
        => _restoreService?.RemoveRestorePoints(mxlPath) ?? RestoreResult.Success;

    #endregion

    public IZoomHistory ZoomHistory => _zoomHistory;

    public TocTreeNode? SelectedTocTreeNode { get; private set; }

    public Task SetSelectedTocTreeNode(TocTreeNode? selectedTocTreeNode)
        => _eventBus.FireSelectedTocTreeNodeChangedAsync(this.SelectedTocTreeNode = selectedTocTreeNode);

    public CartoDataTableService DataTableService => _dataTables;

    public CartoDisplayService DisplayService => _displayService;

    public SettingsService Settings => _settings;

    public GeoTransformerService GeoTransformer => _geoTransformer;

    public PluginManagerService PluginManager => _pluginManager;

    public ICartoInteractiveToolService Tools => _toolService;

    public IEnumerable<IMapApplicationModule> Modules => Document.Modules;

    #region Event Handlers

    private Task HandleLayerSettingsChangedAsync(ILayer oldLayer, ILayer newLayer)
    {
        if (_dataTables.Layers.Contains(oldLayer))
        {
            var oldTableProperties = _dataTables.GetProperties(oldLayer);

            _dataTables.RemoveLayer(oldLayer);
            _dataTables.AddIfNotExists(newLayer, tableProperties: oldTableProperties);

            return _eventBus.FireRefreshDataTableAsync(newLayer, oldLayer);
        }

        return Task.CompletedTask;
    }

    private Task HandleCartoDocumentTouchedAsync(ICartoDocument cartoDocument)
        => _settings.StoreMapDocumentLastAccess(cartoDocument.FilePath);

    private Task HandleMapClickAsync(IPoint point)
    {
        if (Tools.CurrentTool?.ToolType.HasFlag(ToolType.Click) == true)
        {
            return Tools.CurrentTool.OnEvent(this,
                new MapClickEventArgs() { Point = point });
        }

        return Task.CompletedTask;
    }

    private Task HandleMapBBoxAsync(IEnvelope bbox)
    {
        if (Tools.CurrentTool?.ToolType.HasFlag(ToolType.BBox) == true)
        {
            return Tools.CurrentTool.OnEvent(this,
                new MapBBoxEventArgs() { BBox = bbox });
        }

        return Task.CompletedTask;
    }

    private Task HandleToolEventAsync(ToolEventArgs toolEvent)
    {
        if (Tools.CurrentTool is not null)
        {
            return Tools.CurrentTool.OnEvent(this, toolEvent);
        }

        return Task.CompletedTask;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _eventBus.OnLayerSettingsChangedAsync -= HandleLayerSettingsChangedAsync;
        _eventBus.OnCartoDocumentLoadedAsync -= HandleCartoDocumentTouchedAsync;

        _eventBus.OnMapClickAsync -= HandleMapClickAsync;
        _eventBus.OnMapBBoxAsync -= HandleMapBBoxAsync;
        _eventBus.OnToolEventAsync -= HandleToolEventAsync;
    }

    #endregion

    #region IApplicationScope

    async public Task<T?> ShowModalDialog<T>(Type razorComponent,
                                             string title,
                                             T? model = default(T),
                                             ModalDialogOptions? modalDialogOptions = null)
    {
        IDialogReference? dialog = null;
        T? result = default(T);

        var dialogParameters = new DialogParameters
        {
            { "OnDialogClose", new EventCallback<Blazor.Models.Dialogs.DialogResult>(null, OnClose) },
            //{ "OnClose", new EventCallback<Blazor.Models.Dialogs.DialogResult>(null, OnClose) }
        };

        if (model != null)
        {
            dialogParameters.Add("Model", model);
        }

        var dialogOptions = new DialogOptions()
        {
            DisableBackdropClick = true,
            CloseButton = modalDialogOptions?.ShowCloseButton ?? true,
            MaxWidth = modalDialogOptions?.Width switch
            {
                ModalDialogWidth.ExtraExtraLarge => MaxWidth.ExtraExtraLarge,
                ModalDialogWidth.Large => MaxWidth.Large,
                ModalDialogWidth.Medium => MaxWidth.Medium,
                ModalDialogWidth.Small => MaxWidth.Small,
                ModalDialogWidth.ExtraSmall => MaxWidth.ExtraSmall,
                _ => MaxWidth.ExtraLarge
            },
            CloseOnEscapeKey = modalDialogOptions?.CloseOnEscapeKey ?? false,
            FullWidth = modalDialogOptions?.FullWidth ?? false,
        };

        dialog = await _dialogService.ShowAsync(razorComponent, title, dialogParameters, dialogOptions);
        var dialogResult = await dialog.Result;

        if (dialogResult != null && !dialogResult.Canceled)
        {
            return result;
        }

        return default(T?);

        void OnClose(Blazor.Models.Dialogs.DialogResult? data)
        {
            if (data?.Result is T)
            {
                result = (T)data.Result;
            }

            if (dialog != null)
            {
                dialog.Close();
            }
        }
    }

    async public Task<T?> ShowKnownDialog<T>(KnownDialogs dialog,
                                             string? title = null,
                                             T? model = default)
    {
        var knownDialog = _knownDialogs.Where(d => d.Dialog == dialog).FirstOrDefault();

        if (knownDialog == null)
        {
            throw new GeneralException($"Dialog {dialog} is not registered as know dialog");
        }

        model = model ?? Activator.CreateInstance<T>();

        if (model == null)
        {
            throw new GeneralException($"Can't create dialog model");
        }

        return await ShowModalDialog(knownDialog.RazorType,
                                     title ?? knownDialog.Title,
                                     model,
                                     knownDialog.DialogOptions);
    }

    #endregion

    #region Busy Context

    override protected Task HandleBusyStatusChanged(bool isBussy, string message)
        => _eventBus.FireBusyStatusChangedAsync(isBussy, message);

    override async protected ValueTask SetBusyCursor()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsyncIgnoreErrors("window.gview_base.setCursor", "wait");
        }
        catch { }
    }
    override async protected ValueTask SetDefaultCursor()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsyncIgnoreErrors("window.gview_base.setCursor", "default");
        }
        catch { }
    }

    #endregion
}
