using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Editor.Core;
using gView.Framework.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Interoperability.Extensions.Editing;

/// <summary>
/// The edit rules of a feature layer, shared by GeoServices REST (FeatureServer) and OGC API - Features (Part 4):
/// allowed edit statements of the layer (editor module), validation of inserts/updates,
/// projection into the feature class CRS, geometry cleaning and the (optional) edit session.
/// </summary>
public class FeatureEditService
{
    private readonly int _layerId;
    private readonly string _statementsError;
    private readonly IFeatureUpdater _database;
    private readonly GeometryType _geometryType;
    private readonly ISpatialReference _defaultSpatialReference;
    private readonly IDatumTransformations _datumTransformations;

    public FeatureEditService(
                int layerId,
                IFeatureClass featureClass,
                IFeatureUpdater database,
                EditStatements allowedStatements,
                GeometryType geometryType = GeometryType.Unknown,
                ISpatialReference defaultSpatialReference = null,
                IDatumTransformations datumTransformations = null,
                string statementsError = null)
    {
        _layerId = layerId;
        FeatureClass = featureClass;
        _database = database;
        AllowedStatements = allowedStatements;
        _geometryType = geometryType != GeometryType.Unknown ? geometryType : featureClass?.GeometryType ?? GeometryType.Unknown;
        _defaultSpatialReference = defaultSpatialReference;
        _datumTransformations = datumTransformations;
        _statementsError = statementsError;
    }

    /// <summary>
    /// Resolves the feature class of a layer and the edit statements configured in the editor module
    /// </summary>
    public static FeatureEditService Create(IServiceMap serviceMap, int layerId, bool useTOC = true)
    {
        var featureClass = FindFeatureClass(serviceMap, layerId, useTOC);
        var (statements, statementsError) = EditLayerStatements(serviceMap, layerId);

        return new FeatureEditService(
            layerId,
            featureClass,
            featureClass.Dataset?.Database as IFeatureUpdater,
            statements,
            GetGeometryType(serviceMap, featureClass),
            serviceMap.LayerDefaultSpatialReference,
            serviceMap.Display?.DatumTransformations,
            statementsError);
    }

    public IFeatureClass FeatureClass { get; }

    public IFeatureUpdater Database => _database ?? throw new MapServerException("Featureclass is not editable");

    public bool IsEditable => _database != null && _statementsError == null && AllowedStatements != EditStatements.NONE;

    public EditStatements AllowedStatements { get; }

    #region Statements

    /// <summary>
    /// The edit statements (INSERT, UPDATE, DELETE) configured for a layer in the editor module
    /// </summary>
    public static (EditStatements statements, string error) EditLayerStatements(IServiceMap serviceMap, int layerId)
    {
        var editModule = serviceMap.GetModule<gView.Plugins.Modules.EditorModule>();
        if (editModule == null)
        {
            return (EditStatements.NONE, "No editor module available for service");
        }

        var editLayer = editModule.GetEditLayer(layerId);
        if (editLayer == null)
        {
            return (EditStatements.NONE, $"No editable layer found with id={layerId}");
        }

        return (editLayer.Statements, null);
    }

    public static void CheckEditableStatement(IServiceMap serviceMap, int layerId, EditStatements statement)
    {
        var (statements, error) = EditLayerStatements(serviceMap, layerId);
        CheckStatement(layerId, statements, error, statement);
    }

    public void CheckStatement(EditStatements statement)
        => CheckStatement(_layerId, AllowedStatements, _statementsError, statement);

    private static void CheckStatement(int layerId, EditStatements statements, string error, EditStatements statement)
    {
        if (error != null)
        {
            throw new MapServerException(error);
        }

        if (!statements.HasFlag(statement))
        {
            throw new MapServerException($"Editoperation {statement} not allowed for layer with id={layerId}");
        }
    }

    #endregion

    #region Prepare

    /// <summary>
    /// Validates new features (no object id, with geometry), projects and cleans their geometries
    /// </summary>
    public void PrepareInserts(IList<IFeature> features)
    {
        if (features.Any(f => f.OID > 0))
        {
            throw new MapServerException("Can't insert features with existing ObjectId");
        }
        if (features.Any(f => f.Shape is null))
        {
            throw new MapServerException("Insert features without geometry are not allowed");
        }

        ProjectToFeatureClass(features);
        GeometryMakeValid(features);
    }

    /// <summary>
    /// Validates changed features (with object id), projects and cleans their geometries
    /// </summary>
    public void PrepareUpdates(IList<IFeature> features)
    {
        if (features.Any(f => f.OID <= 0))
        {
            throw new MapServerException("Can't update features without existing ObjectId");
        }

        ProjectToFeatureClass(features);
        GeometryMakeValid(features);
    }

    /// <summary>
    /// Transforms geometries with a spatial reference (Shape.Srs) other than the feature class one
    /// </summary>
    public void ProjectToFeatureClass(IEnumerable<IFeature> features)
    {
        // feature classes without spatial reference use the layer default spatial reference of the map
        var targetSRef = FeatureClass?.SpatialReference ?? _defaultSpatialReference;
        int? fcSrs = targetSRef?.EpsgCode > 0 ? targetSRef.EpsgCode : null;

        if (fcSrs == null)
        {
            return;
        }

        foreach (var feature in features)
        {
            if (feature.Shape != null && feature.Shape.Srs.HasValue && !feature.Shape.Srs.Equals(fcSrs))
            {
                using (var transformer = GeometricTransformerFactory.Create(_datumTransformations))
                {
                    var fromSrs = SpatialReference.FromID("epsg:" + feature.Shape.Srs.ToString());
                    var toSrs = SpatialReference.FromID("epsg:" + fcSrs.ToString());

                    transformer.SetSpatialReferences(fromSrs, toSrs);

                    var transformedShape = transformer.Transform2D(feature.Shape) as IGeometry;
                    transformedShape.Srs = fcSrs;

                    feature.Shape = transformedShape;
                }
            }
        }
    }

    public void GeometryMakeValid(IEnumerable<IFeature> features)
    {
        if (_geometryType != GeometryType.Polygon)
        {
            return;
        }

        foreach (var feature in features)
        {
            var polygon = feature.Shape as IPolygon;

            if (polygon == null)
            {
                continue;
            }

            var sRef = polygon.Srs.HasValue && !polygon.Srs.Equals(FeatureClass.SpatialReference?.EpsgCode) ?
                SpatialReference.FromID($"epsg:{polygon.Srs.Value}") :
                FeatureClass.SpatialReference ?? _defaultSpatialReference;

            if (sRef != null)
            {
                feature.Shape.Clean(CleanGemetryMethods.IdentNeighbors | CleanGemetryMethods.ZeroParts);
            }
        }
    }

    #endregion

    #region Execute

    /// <summary>
    /// Starts an edit session (one transaction), if the database supports it. Otherwise null.
    /// </summary>
    async public Task<IFeatureEditSession> BeginEditSession()
        => Database is ISupportsFeatureEditSession sessionFactory
            ? await sessionFactory.BeginEditSession()
            : null;

    /// <summary>
    /// Inserts the features. The database writes the new object ids back to feature.OID.
    /// </summary>
    public Task<bool> Insert(List<IFeature> features, IFeatureEditSession session = null)
        => session != null
            ? session.Insert(FeatureClass, features, returnIds: true)
            : Database.Insert(FeatureClass, features, returnIds: true);

    public Task<bool> Update(List<IFeature> features, IFeatureEditSession session = null)
        => session != null
            ? session.Update(FeatureClass, features)
            : Database.Update(FeatureClass, features);

    public Task<bool> Delete(int objectId, IFeatureEditSession session = null)
        => session != null
            ? session.Delete(FeatureClass, objectId)
            : Database.Delete(FeatureClass, objectId);

    public string LastErrorMessage(IFeatureEditSession session = null)
        => session != null ? session.LastErrorMessage : _database?.LastErrorMessage;

    #endregion

    #region Helper

    public static IFeatureClass FindFeatureClass(IServiceMap serviceMap, int layerId, bool useTOC = true)
    {
        var tableClasses = MapServerHelper.FindMapLayers(serviceMap, useTOC, layerId.ToString())
            .Where(l => l.Class is ITableClass)
            .Select(l => (ITableClass)l.Class)
            .ToList();

        if (tableClasses.Count > 1)
        {
            throw new MapServerException("FeatureService can't be used with aggregated feature classes");
        }
        if (tableClasses.Count == 0 || !(tableClasses[0] is IFeatureClass))
        {
            throw new MapServerException("FeatureService can only used with feature classes");
        }

        return (IFeatureClass)tableClasses[0];
    }

    public static GeometryType GetGeometryType(IServiceMap map, IFeatureClass featureClass)
    {
        if (featureClass.GeometryType != GeometryType.Unknown)
        {
            return featureClass.GeometryType;
        }

        var layer = map.MapElements.Where(e => e.Class == featureClass).FirstOrDefault() as IFeatureLayer;

        return layer?.LayerGeometryType ?? GeometryType.Unknown;
    }

    #endregion
}
