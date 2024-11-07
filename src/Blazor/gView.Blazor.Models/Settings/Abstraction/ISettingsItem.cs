using LiteDB;
using System;

namespace gView.Blazor.Models.Settings.Abstraction;
public interface ISettingsItem
{
    [BsonId]
    LiteDB.ObjectId ItemId { get; set; }
}
