using gView.Blazor.Models.MapServer;

namespace gView.WebApps.Model;

public class PublishModel
{
    public IEnumerable<ServerInstanceModel>? Servers { get; set; }
}
