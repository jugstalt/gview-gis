namespace gView.Razor.Leaflet.Services;

public class LeafletService
{
    private readonly LeafletInteropService _leafletInterop;

    public LeafletService(LeafletInteropService leafletInterop)
    {
        _leafletInterop = leafletInterop;
    }

    public Map CreateMap()
    {
        return new Map(_leafletInterop);
    }
}
