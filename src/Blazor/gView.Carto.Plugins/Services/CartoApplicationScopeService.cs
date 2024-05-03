using gView.Blazor.Core.Exceptions;
using gView.Blazor.Core.Models;
using gView.Blazor.Core.Services;
using gView.Blazor.Core.Services.Abstraction;
using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Models.ToolEvents;
using gView.Carto.Core.Models.Tree;
using gView.Carto.Core.Services;
using gView.Carto.Core.Services.Abstraction;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Blazor.Models;
using gView.Framework.Blazor.Services;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.IO;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using System.Security.Permissions;

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
                                        IOptions<CartoApplicationScopeServiceOptions> options,
                                        IScopeContextService? scopeContext = null
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

    async public Task<bool> LoadCartoDocument(string mxlFilePath)
    {
        XmlStream stream = new XmlStream("");
        stream.ReadStream(mxlFilePath);

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

        this.Document = cartoDocument;

        return true;
    }

    async public Task<bool> SaveCartoDocument(string xmlFilePath, bool performEncryption)
    {
        XmlStream stream = new XmlStream("MapApplication", performEncryption);
        stream.Save("MapDocument", this.Document);

        stream.WriteStream(xmlFilePath);

        this.Document.FilePath = xmlFilePath;

        await HandleCartoDocumentTouchedAsync(Document);

        return true;
    }

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
