using gView.Blazor.Models.MapServer;
using System.Collections.Generic;

namespace gView.Blazor.Core.Services;

public class PublishMapServiceOptions
{
    public IEnumerable<ServerInstanceModel> Services { get; set; } = [];
}
