using gView.Blazor.Core.Extensions;
using gView.Blazor.Core.Services.Abstraction;
using gView.Blazor.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Blazor.Core.Services;
public class SettingsService
{
    private readonly PersistentSettingsService _persistentSettings;
    private readonly IAppIdentityProvider _identityProvider;

    public SettingsService(IAppIdentityProvider identityProvider,
                           PersistentSettingsService persistentSettings)
    {
        _identityProvider = identityProvider;
        _persistentSettings = persistentSettings;
    }

    public Task<bool> StoreMapDocumentLastAccess(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return Task.FromResult(false);
        }

        try
        {
            string collectionName = UserCollection("mapfiles");

            var existingMapFileItem = _persistentSettings.GetItems<MapFileItem>(UserCollection("mapfiles"), m => m.Path == path)
                                                         .FirstOrDefault();

            if (existingMapFileItem is not null)
            {
                existingMapFileItem.LastAccess = DateTimeOffset.UtcNow;

                _persistentSettings.Update(existingMapFileItem, collectionName);
            }
            else
            {
                _persistentSettings.Insert(new MapFileItem()
                {
                    Path = path,
                    LastAccess = DateTimeOffset.UtcNow
                }, collectionName);
            }

            return Task.FromResult(true);
        }
        catch (Exception /*ex*/)
        {
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<MapFileItem>> GetLastAccessedDocuments(int count = 10)
    {
        try
        {
            return Task.FromResult<IEnumerable<MapFileItem>>(_persistentSettings.GetItems<MapFileItem>(UserCollection("mapfiles"), m => true)
                .OrderByDescending(m => m.LastAccess)
                .Take(count)
                .ToArray());
        }
        catch (Exception /*ex*/)
        {
            return Task.FromResult<IEnumerable<MapFileItem>>([]);
        }
    }

    #region Helper

    private string UserCollection(string collectionName) => $"{collectionName}_{_identityProvider.Identity.UsernameWithoutSpatialSigns('_')}";

    #endregion
}
