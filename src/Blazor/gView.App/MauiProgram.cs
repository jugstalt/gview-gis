﻿using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using gView.Blazor.Core.Extensions.DependencyInjection;
using gView.Carto.Plugins.Extensions.DependencyInjection;
using gView.DataExplorer.Plugins.Extensions.DependencyInjection;
using gView.Razor.Extensions.DependencyInjection;
using gView.Razor.Leaflet.Extensions.DependencyInjection;

namespace gView.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

                config.SnackbarConfiguration.PreventDuplicates = true;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            builder.Services.AddApplicationScopeFactory();

            builder.Services.AddCartoDesktopApplicationService();
            builder.Services.AddCartoApplicationScopeService(config =>
            {
                config.ConfigRootPath = Path.Combine("C:\\temp", "gview-explorer");
            });

            builder.Services.AddExplorerDesktopApplicationService();
            builder.Services.AddExplorerApplicationScopeService(config =>
            {
                config.ConfigRootPath = Path.Combine("C:\\temp", "gview-explorer");
            });

            builder.Services.AddKnownExplorerDialogsServices();
            builder.Services.AddFrameworkServices();
            builder.Services.AddIconService();
            builder.Services.AddLeafletService();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
