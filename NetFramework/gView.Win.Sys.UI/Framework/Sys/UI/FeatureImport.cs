using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.system.UI
{
    public class FeatureImport
    {
        public delegate void ReportActionEvent(FeatureImport sender, string action);
        public delegate void ReportProgressEvent(FeatureImport sender, int progress);
        public delegate void ReportRequestEvent(FeatureImport sender, RequestArgs args);
        public event ReportActionEvent ReportAction = null;
        public event ReportProgressEvent ReportProgress = null;
        public event ReportRequestEvent ReportRequest = null;
        private string _errMsg = "";
        private ICancelTracker _cancelTracker;
        private IGeometricTransformer _transformer = null;
        private bool _schemaOnly = false;

        public FeatureImport(int featureBufferSize = 50)
        {
            _cancelTracker = new CancelTracker();
            ((CancelTracker)_cancelTracker).Reset();

            this.FeatureBufferSize = featureBufferSize > 0 ? featureBufferSize : 50;
        }

        public FeatureImport(ICancelTracker cancelTracker, int featureBufferSize = 50)
        {
            _cancelTracker = cancelTracker;

            this.FeatureBufferSize = featureBufferSize > 0 ? featureBufferSize : 50;
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

        public Task<bool> ImportToNewFeatureclass(IFeatureDataset destDS, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, geometryType? sourceGeometryType = null)
        {
            return ImportToNewFeatureclass(destDS, fcname, sourceFC, fieldTranslation, project, null, sourceGeometryType);
        }

        async public Task<bool> ImportToNewFeatureclass(IFeatureDataset destDS, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, List<IQueryFilter> filters, geometryType? sourceGeometryType = null)
        {
            if (destDS == null)
            {
                return false;
            }

            DatasetNameCase nameCase = DatasetNameCase.ignore;
            foreach (System.Attribute attribute in System.Attribute.GetCustomAttributes(destDS.GetType()))
            {
                if (attribute is UseDatasetNameCase)
                {
                    nameCase = ((UseDatasetNameCase)attribute).Value;
                }
            }
            return await ImportToNewFeatureclass(destDS, fcname, sourceFC, fieldTranslation, project, filters, nameCase, sourceGeometryType);
        }

        async private Task<bool> ImportToNewFeatureclass(IFeatureDataset destDS, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, List<IQueryFilter> filters, DatasetNameCase namecase, geometryType? sourceGeometryType = null)
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
                fcname = fcname.Replace(".", "_");

                if (destDS == null)
                {
                    _errMsg = "Argument Exception";
                    return false;
                }
                IFeatureDatabase fdb = destDS.Database as IFeatureDatabase;
                if (!(fdb is IFeatureUpdater))
                {
                    _errMsg = "Database don't implement IFeatureUpdater...";
                    return false;
                }

                IDatasetElement destLayer = await destDS.Element(fcname);
                if (destLayer != null)
                {
                    if (ReportRequest != null)
                    {
                        RequestArgs args = new RequestArgs(
                            "Featureclass " + fcname + " already exists in database\nDo want to replace it?",
                            MessageBoxButtons.YesNoCancel,
                            DialogResult.Cancel);
                        ReportRequest(this, args);
                        switch (args.Result)
                        {
                            case DialogResult.No:
                                return true;
                            case DialogResult.Cancel:
                                _errMsg = "Import is canceled by the user...";
                                return false;
                        }
                    }
                }

                if (destLayer != null)
                {
                    await fdb.DeleteFeatureClass(fcname);
                }

                GeometryDef geomDef = new GeometryDef(sourceFC);
                if (geomDef.GeometryType == geometryType.Unknown && sourceGeometryType != null)
                {
                    geomDef.GeometryType = sourceGeometryType.Value;
                }

                int fcID = await fdb.CreateFeatureClass(destDS.DatasetName,
                                                  fcname,
                                                  geomDef,
                                                  (fieldTranslation == null) ?
                                                  ((sourceFC.Fields != null) ? (IFields)sourceFC.Fields.Clone() : new Fields()) :
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
                IFeatureClass destFC = destLayer.Class as IFeatureClass;

                if (project && destFC.SpatialReference != null && !destFC.SpatialReference.Equals(sourceFC.SpatialReference))
                {
                    _transformer = GeometricTransformerFactory.Create();
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

        async private Task<bool> CopyFeatures(IFeatureClass source, IFeatureUpdater fdb, IFeatureClass dest, FieldTranslation fTrans, List<IQueryFilter> filters)
        {
            if (filters == null)
            {
                QueryFilter filter = new QueryFilter();
                filter.SubFields = "*";

                filters = new List<IQueryFilter>();
                filters.Add(filter);
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

    public class RequestArgs
    {
        private string _request;
        private MessageBoxButtons _buttons;
        private DialogResult _result;

        public RequestArgs(string request, MessageBoxButtons buttons, DialogResult result)
        {
            _request = request;
            _buttons = buttons;
            _result = result;
        }

        public MessageBoxButtons Buttons
        {
            get
            {
                return _buttons;
            }
        }

        public string Request
        {
            get { return _request; }
        }

        public DialogResult Result
        {
            get { return _result; }
            set { _result = value; }
        }
    }
}
