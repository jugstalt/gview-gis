using gView.Blazor.Models.Settings.Abstraction;
using LiteDB;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace gView.Blazor.Core.Services;
public class PersistentSettingsService
{
    private readonly SettingsServiceOptions _options;

    public PersistentSettingsService(IOptions<SettingsServiceOptions> options)
    {
        _options = options.Value;
    }

    public bool Insert<T>(T item, string collectionName)
        where T : ISettingsItem
    {
        try
        {
            using (var db = new LiteDatabase(_options.Path))
            {
                var collection = db.GetCollection<T>(collectionName);

                collection.Insert(item);

                return true;
            }
        }
        catch (Exception /*ex*/)
        {
            //var msg = ex.Message;

            return false;
        }
    }

    public bool Update<T>(T item, string collectionName)
         where T : ISettingsItem
    {
        using (var db = new LiteDatabase(_options.Path))
        {
            var collection = GetCollection<T>(db, collectionName);

            collection.Update(item);

            return true;
        }
    }

    public bool Delete<T>(T item, string collectionName)
         where T : ISettingsItem
    {
        using (var db = new LiteDatabase(_options.Path))
        {
            var collection = GetCollection<T>(db, collectionName);

            collection.Delete(item.ItemId);

            return true;
        }
    }

    public IEnumerable<T> GetItems<T>(string collectionName, Expression<Func<T, bool>> expression)
        where T : ISettingsItem
    {
        using (var db = new LiteDatabase(_options.Path))
        {
            var collection = GetCollection<T>(db, collectionName);
            return collection.Query()
                             .Where(expression)?
                             .ToArray() ?? [];
        }
    }

    #region Helper

    private ILiteCollection<T> GetCollection<T>(LiteDatabase db, string collectionName)
        where T : ISettingsItem
    {
        var collection = db.GetCollection<T>(collectionName);

        collection.EnsureIndex(x => x.ItemId, true);

        return collection;
    }

    #endregion
}
