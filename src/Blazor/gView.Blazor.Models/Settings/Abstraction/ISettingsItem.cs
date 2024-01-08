using System;

namespace gView.Blazor.Models.Settings.Abstraction;
public interface ISettingsItem
{
    LiteDB.ObjectId ItemId { get; set; }
}
