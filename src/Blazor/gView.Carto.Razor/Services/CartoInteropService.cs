using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace gView.Carto.Razor.Services;
internal class CartoInteropService
{
    private const string _carto = "window.cartoInterops";

    private readonly IJSRuntime _jsRuntime;

    public CartoInteropService(IJSRuntime jSRuntime)
    {
        _jsRuntime = jSRuntime;
    }

    internal ValueTask ShowDataFrame(int minSize = 0) =>
            _jsRuntime.InvokeVoidAsync($"{_carto}.showDataFrame", minSize);

    internal ValueTask HideDataFrame() =>
            _jsRuntime.InvokeVoidAsync($"{_carto}.hideDataFrame");

    internal ValueTask FloatDataFrame(ElementReference frame, ElementReference titlebar) =>
            _jsRuntime.InvokeVoidAsync($"{_carto}.floatDataFrame", frame, titlebar);

    internal ValueTask DockDataFrame(ElementReference frame) =>
            _jsRuntime.InvokeVoidAsync($"{_carto}.dockDataFrame", frame);
}
