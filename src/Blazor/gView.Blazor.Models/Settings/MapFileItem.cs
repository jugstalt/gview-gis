using gView.Blazor.Models.Settings.Abstraction;
using LiteDB;
using System;

namespace gView.Blazor.Models.Settings;
public class MapFileItem : ISettingsItem
{
    [BsonId]
    public LiteDB.ObjectId ItemId { get; set; } = LiteDB.ObjectId.Empty;

    public string Path { get; set; } = "";

    public DateTimeOffset LastAccess { get; set; }
}
