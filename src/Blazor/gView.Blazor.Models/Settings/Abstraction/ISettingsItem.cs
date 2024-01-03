using System;

namespace gView.Blazor.Models.Settings.Abstraction;
public interface ISettingsItem
{
    Guid ItemId { get; set; }
}
