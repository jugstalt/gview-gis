using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Data;
using System.Collections.Generic;
using System;
using gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer;
using gView.GeoJsonService.DTOs;
using gView.Framework.Editor.Core;
using gView.Framework.Core.Exceptions;

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

    static public IFeatureClass GetFeatureClass(this IServiceMap serviceMap, int id)
    {
        string filterQuery;

        var tableClasses = FindTableClass(serviceMap, id, out filterQuery);
        if (tableClasses.Count > 1)
        {
            throw new Exception("FeatureService can't be used with aggregated feature classes");
        }
        if (tableClasses.Count == 0 || !(tableClasses[0] is IFeatureClass))
        {
            throw new Exception("FeatureService can only used with feature classes");
        }

        var featureClass = (IFeatureClass)tableClasses[0];

        return featureClass;
    }

    static public IServiceMap CheckEditableStatement(this IServiceMap serviceMap, int id, EditStatements statement)
    {
        var editModule = serviceMap.GetModule<gView.Plugins.Modules.EditorModule>();
        if (editModule == null)
        {
            throw new MapServerException("No editor module available for service");
        }

        var editLayer = editModule.GetEditLayer(id);
        if (editLayer == null)
        {
            throw new MapServerException($"No editable layer found with id={id}");
        }

        if (!editLayer.Statements.HasFlag(statement))
        {
            throw new MapServerException("Editoperation {statement} not allowed for layer with id={id}");
        }

        return serviceMap;
    }
}
