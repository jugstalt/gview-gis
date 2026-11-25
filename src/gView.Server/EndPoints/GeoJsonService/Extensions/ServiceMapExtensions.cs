#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Data;
using gView.Framework.Editor.Core;
using System;
using System.Collections.Generic;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

static public class ServiceMapExtensions
{
    private const bool UseTOC = true;

    static public List<ITableClass>? FindTableClass(this IServiceMap map, int id, out string filterQuery)
    {
        filterQuery = String.Empty;
        if (map == null)
        {
            return null;
        }

        List<ITableClass> classes = new List<ITableClass>();

        foreach (ILayer element in MapServerHelper.FindMapLayers(map, UseTOC, id.ToString()))
        {
            if (element.Class is ITableClass tableClass)
            {
                classes.Add(tableClass);
            }

            if (element is IFeatureLayer)
            {
                if (((IFeatureLayer)element).FilterQuery != null)
                {
                    string fquery = ((IFeatureLayer)element).FilterQuery.WhereClause;
                    if (String.IsNullOrWhiteSpace(filterQuery))
                    {
                        filterQuery = fquery;
                    }
                    else if (filterQuery != fquery)
                    {
                        filterQuery = $"({filterQuery}) AND ({fquery})";
                    }
                }
            }
        }
        return classes;
    }
}
