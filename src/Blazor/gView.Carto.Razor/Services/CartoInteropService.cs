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
}
