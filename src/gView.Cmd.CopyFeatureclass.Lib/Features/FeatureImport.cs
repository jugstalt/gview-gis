﻿using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace gView.Cmd.CopyFeatureclass.Lib.Features;
public class FeatureImport
{
    public delegate void ReportActionEvent(FeatureImport sender, string action);
    public delegate void ReportProgressEvent(FeatureImport sender, int progress);

    public event ReportActionEvent? ReportAction;
    public event ReportProgressEvent? ReportProgress = null;

    private string _errMsg = "";
    private ICancelTracker _cancelTracker;
    private IGeometricTransformer? _transformer = null;
    private bool _schemaOnly = false;

    public FeatureImport(ICancelTracker? cancelTracker, int featureBufferSize = 100)
    {
        _cancelTracker = cancelTracker ?? new CancelTracker();

        this.FeatureBufferSize = featureBufferSize > 0 ? featureBufferSize : 100;
    }

    public ICancelTracker CancelTracker
    {
        get { return _cancelTracker; }
    }

    public string lastErrorMsg { get { return _errMsg; } }

    public bool SchemaOnly
    {
        get { return _schemaOnly; }
        set { _schemaOnly = value; }
    }

    private int FeatureBufferSize { get; set; }

    public Task<bool> ImportToNewFeatureclass(IFeatureDataset destDS, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, IDatumTransformations? datumTransformations, GeometryType? sourceGeometryType = null)
    {
        return ImportToNewFeatureclass(destDS, fcname, sourceFC, fieldTranslation, project, datumTransformations, null, sourceGeometryType);
    }

    async public Task<bool> ImportToNewFeatureclass(IFeatureDataset destDS, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, IDatumTransformations? datumTransformations, List<IQueryFilter>? filters, GeometryType? sourceGeometryType = null)
    {
        if (destDS == null)
        {
            return false;
        }

        DatasetNameCase nameCase = DatasetNameCase.ignore;
        bool importWithSchema = false;

        foreach (System.Attribute attribute in System.Attribute.GetCustomAttributes(destDS.GetType()))
        {
            if (attribute is UseDatasetNameCaseAttribute)
            {
                nameCase = ((UseDatasetNameCaseAttribute)attribute).Value;
            }

            if (attribute is ImportFeatureClassNameWithSchemaAttribute)
            {
                importWithSchema = true;
            }
        }

        if (importWithSchema == false)
        {
            fcname = fcname.Replace(".", "_");
        }
        else
        {
            if (fcname.Contains(".") && fcname.Split('.').Length > 2)
            {
                throw new System.Exception($"Featureclass name {fcname} not allowed. Use SCHEMA.FEATURECLASS_NAME for example.");
            }
        }

        return await ImportToNewFeatureclass(destDS, fcname, sourceFC, fieldTranslation, project, datumTransformations, filters, nameCase, sourceGeometryType);
    }

    async private Task<bool> ImportToNewFeatureclass(IFeatureDataset destDS, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, IDatumTransformations? datumTransformations, List<IQueryFilter>? filters, DatasetNameCase namecase, GeometryType? sourceGeometryType = null)
    {
        if (!_cancelTracker.Continue)
        {
            return true;
        }

        switch (namecase)
        {
            case DatasetNameCase.upper:
                fcname = fcname.ToUpper();
                fieldTranslation.ToUpper();
                break;
            case DatasetNameCase.lower:
                fcname = fcname.ToLower();
                fieldTranslation.ToLower();
                break;
            case DatasetNameCase.classNameUpper:
                fcname = fcname.ToUpper();
                break;
            case DatasetNameCase.classNameLower:
                fcname = fcname.ToLower();
                break;
            case DatasetNameCase.fieldNamesUpper:
                fieldTranslation.ToUpper();
                break;
            case DatasetNameCase.fieldNamesLower:
                fieldTranslation.ToLower();
                break;
        }
        try
        {
            if (destDS == null)
            {
                _errMsg = "Argument Exception";
                return false;
            }
            IFeatureDatabase? fdb = destDS.Database as IFeatureDatabase;
            if (!(fdb is IFeatureUpdater))
            {
                _errMsg = "Database don't implement IFeatureUpdater...";
                return false;
            }

            IDatasetElement destLayer = await destDS.Element(fcname);
            if (destLayer != null)
            {
                throw new Exception($"Destination layer {destLayer.Title} already exists.");
                //if (ReportRequest != null)
                //{
                //    RequestArgs args = new RequestArgs(
                //        "Featureclass " + fcname + " already exists in database\nDo want to replace it?",
                //        MessageBoxButtons.YesNoCancel,
                //        DialogResult.Cancel);
                //    ReportRequest(this, args);
                //    switch (args.Result)
                //    {
                //        case DialogResult.No:
                //            return true;
                //        case DialogResult.Cancel:
                //            _errMsg = "Import is canceled by the user...";
                //            return false;
                //    }
                //}
            }

            if (destLayer != null)
            {
                await fdb.DeleteFeatureClass(fcname);
            }

            GeometryDef geomDef = new GeometryDef(sourceFC);
            if (geomDef.GeometryType == GeometryType.Unknown && sourceGeometryType != null)
            {
                geomDef.GeometryType = sourceGeometryType.Value;
            }

            int fcID = await fdb.CreateFeatureClass(destDS.DatasetName,
                                                    fcname,
                                                    geomDef,
                                                    (fieldTranslation == null) ?
                                                    ((sourceFC.Fields != null) ? (IFieldCollection)sourceFC.Fields.Clone() : new FieldCollection()) :
                                                    fieldTranslation.DestinationFields);
            if (fcID < 0)
            {
                _errMsg = "Can't create featureclass " + fcname + "...\r\n" + fdb.LastErrorMessage;
                destDS.Dispose();
                return false;
            }

            destLayer = await destDS.Element(fcname);

            if (destLayer == null || !(destLayer.Class is IFeatureClass))
            {
                _errMsg = "Can't load featureclass " + fcname + "...\r\n" + destDS.LastErrorMessage;
                destDS.Dispose();
                return false;
            }
            IFeatureClass destFC = (IFeatureClass)destLayer.Class;

            if (project && destFC.SpatialReference != null && !destFC.SpatialReference.Equals(sourceFC.SpatialReference))
            {
                _transformer = GeometricTransformerFactory.Create(datumTransformations);
                //_transformer.FromSpatialReference = sourceFC.SpatialReference;
                //_transformer.ToSpatialReference = destFC.SpatialReference;
                _transformer.SetSpatialReferences(sourceFC.SpatialReference, destFC.SpatialReference);
            }

            var importBufferSizeAttribute = destFC.GetType().GetCustomAttribute<ImportFeaturesBufferSizeAttribute>();
            if (importBufferSizeAttribute != null && importBufferSizeAttribute.BufferSize > 0)
            {
                this.FeatureBufferSize = importBufferSizeAttribute.BufferSize;
            }

            if (_cancelTracker.Continue)
            {
                bool result = true;

                if (fdb is IFeatureImportEvents)
                {
                    ((IFeatureImportEvents)fdb).BeforeInsertFeaturesEvent(sourceFC, destFC);
                }

                if (!_schemaOnly)
                {
                    result = await CopyFeatures(sourceFC, fdb, destFC, fieldTranslation, filters);
                }
                if (!result)
                {
                    await fdb.DeleteFeatureClass(fcname);
                    destDS.Dispose();
                    return false;
                }

                if (fdb is IFeatureImportEvents)
                {
                    ((IFeatureImportEvents)fdb).AfterInsertFeaturesEvent(sourceFC, destFC);
                }
            }

            if (fdb is IFileFeatureDatabase)
            {
                if (!((IFileFeatureDatabase)fdb).Flush(destFC))
                {
                    _errMsg = "Error executing flush for file database..." + fdb.LastErrorMessage;
                    return false;
                }
            }
            destDS.Dispose();

            ReportAction?.Invoke(this, "Import finished");

            if (_cancelTracker.Continue)
            {
                return true;
            }
            else
            {
                await fdb.DeleteFeatureClass(fcname);
                _errMsg = "Import is canceled by the user...";
                return false;
            }
        }
        finally
        {
            if (_transformer != null)
            {
                _transformer.Release();
                _transformer = null;
            }
        }
    }

    async private Task<bool> CopyFeatures(IFeatureClass source, IFeatureUpdater fdb, IFeatureClass dest, FieldTranslation? fTrans, List<IQueryFilter>? filters)
    {
        if (filters == null)
        {
            QueryFilter filter = new QueryFilter();
            filter.SubFields = "*";

            filters = new List<IQueryFilter>
            {
                filter
            };
        }

        int counter = 0;
        List<IFeature> features = new List<IFeature>();
        foreach (IQueryFilter filter in filters)
        {
            using (IFeatureCursor cursor = await source.GetFeatures(filter))
            {
                IFeature feature;
                while ((feature = await cursor.NextFeature()) != null)
                {
                    if (fTrans != null)
                    {
                        fTrans.RenameFields(feature);
                    }

                    if (_transformer != null)
                    {
                        feature.Shape = _transformer.Transform2D(feature.Shape) as IGeometry;
                    }
                    features.Add(feature);
                    counter++;

                    if (!_cancelTracker.Continue)
                    {
                        break;
                    }

                    if ((counter % this.FeatureBufferSize) == 0)
                    {
                        if (!await fdb.Insert(dest, features))
                        {
                            _errMsg = "Fatal error: destdb insert failed...\n" + fdb.LastErrorMessage;
                            return false;
                        }
                        features.Clear();
                        if (ReportProgress != null)
                        {
                            ReportProgress(this, counter);
                        }
                    }
                }
            }
        }
        if (features.Count > 0 && _cancelTracker.Continue)
        {
            if (!await fdb.Insert(dest, features))
            {
                _errMsg = "Fatal error: destdb insert failed...";
                return false;
            }
            features.Clear();
            if (ReportProgress != null)
            {
                ReportProgress(this, counter);
            }
        }
        return true;
    }
}
