using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.system;
using gView.Framework.Geometry;
using gView.Framework.UI.Dialogs;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.MSAccess;

namespace gView.DataSources.Fdb.UI
{
    public class FDBImport
    {
        public enum TreeVersion { BinaryTree, BinaryTree2 };
        private TreeVersion _treeVersion = TreeVersion.BinaryTree2;

        public delegate void ReportActionEvent(FDBImport sender, string action);
        public delegate void ReportProgressEvent(FDBImport sender, int progress);
        public delegate void ReportRequestEvent(FDBImport sender, RequestArgs args);
        public event ReportActionEvent ReportAction = null;
        public event ReportProgressEvent ReportProgress = null;
        public event ReportRequestEvent ReportRequest = null;
        private string _errMsg = "";
        private ICancelTracker _cancelTracker;
        private IGeometricTransformer _transformer = null;
        private bool _schemaOnly = false;

        public FDBImport(int featureBufferSize = 1000)
        {
            _cancelTracker = new CancelTracker();
            ((CancelTracker)_cancelTracker).Reset();

            this.FeatureBufferSize = featureBufferSize > 0 ? featureBufferSize : 1000;
        }

        public FDBImport(ICancelTracker cancelTracker, int featureBufferSize = 1000)
        {
            _cancelTracker = cancelTracker;

            this.FeatureBufferSize = featureBufferSize > 0 ? featureBufferSize : 1000;
        }

        public ICancelTracker CancelTracker
        {
            get { return _cancelTracker; }
        }

        private int FeatureBufferSize { get; set; }

        public string lastErrorMsg { get { return _errMsg; } }

        public bool SchemaOnly
        {
            get { return _schemaOnly; }
            set { _schemaOnly = value; }
        }

        public bool ImportToNewFeatureclass(IFeatureDatabase fdb, string dsname, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project)
        {
            return ImportToNewFeatureclass(fdb, dsname, fcname, sourceFC, fieldTranslation, true, null);
        }
        public bool ImportToNewFeatureclass(IFeatureDatabase fdb, string dsname, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, List<IQueryFilter> filters)
        {
            return ImportToNewFeatureclass(fdb, dsname, fcname, sourceFC, fieldTranslation, true, filters, null);
        }
        public bool ImportToNewFeatureclass(IFeatureDatabase fdb, string dsname, string fcname, IFeatureClass sourceFC, FieldTranslation fieldTranslation, bool project, List<IQueryFilter> filters, ISpatialIndexDef sIndexDef, geometryType? sourceGeometryType = null)
        {
            if (!_cancelTracker.Continue) return true;

            if (fdb is AccessFDB)
            {
                ISpatialIndexDef dsSpatialIndexDef = ((AccessFDB)fdb).SpatialIndexDef(dsname);
                if (sIndexDef == null)
                {
                    sIndexDef = dsSpatialIndexDef;
                }
                else if (sIndexDef.GeometryType != dsSpatialIndexDef.GeometryType)
                {
                    _errMsg = "Spatial-Index-Definition-GeometryTypes are not compatible!";
                    return false;
                }
            }
            if (sIndexDef == null) sIndexDef = new gViewSpatialIndexDef();

            bool msSpatial = false;
            if (fdb is SqlFDB &&
                (sIndexDef.GeometryType == GeometryFieldType.MsGeography ||
                 sIndexDef.GeometryType == GeometryFieldType.MsGeometry))
            {
                msSpatial = true;
            }
            else
            {
                int maxAllowedLevel = ((fdb is SqlFDB || fdb is pgFDB) ? 62 : 30);
                if (sIndexDef.Levels > maxAllowedLevel)
                {
                    ISpatialReference defSRef = sIndexDef.SpatialReference;
                    sIndexDef = new gViewSpatialIndexDef(
                        sIndexDef.SpatialIndexBounds,
                        Math.Min(sIndexDef.Levels, maxAllowedLevel),
                        sIndexDef.MaxPerNode,
                        sIndexDef.SplitRatio);
                    ((gViewSpatialIndexDef)sIndexDef).SpatialReference = defSRef;
                }
            }

            try
            {
                fcname = fcname.Replace(".", "_");

                IFeatureDataset destDS = fdb[dsname];
                if (destDS == null)
                {
                    _errMsg = fdb.lastErrorMsg;
                    return false;
                }

                IDatasetElement destLayer = destDS[fcname] as IDatasetElement;
                if (destLayer != null)
                {
                    if (ReportRequest != null)
                    {
                        RequestArgs args = new RequestArgs(
                            "Featureclass " + fcname + " already exists in " + dsname + "\nDo want to replace it?",
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

                GeometryDef geomDef = new GeometryDef(sourceFC);
                if (geomDef.GeometryType == geometryType.Unknown && sourceGeometryType != null)
                    geomDef.GeometryType = sourceGeometryType.Value;

                int fcID = -1;
                if (destLayer != null)
                {
                    if (fdb is AccessFDB)
                    {
                        fcID = ((AccessFDB)fdb).ReplaceFeatureClass(destDS.DatasetName,
                                                      fcname,
                                                      geomDef,
                                                      (fieldTranslation == null) ?
                                                      ((sourceFC.Fields != null) ? (IFields)sourceFC.Fields.Clone() : new Fields()) :
                                                      fieldTranslation.DestinationFields);
                        if (fcID < 0)
                        {
                            _errMsg = "Can't replace featureclass " + fcname + "...\r\n" + fdb.lastErrorMsg;
                            destDS.Dispose();
                            return false;
                        }
                    }
                    else
                    {
                        fdb.DeleteFeatureClass(fcname);
                    }
                }
                if (fcID < 0)
                {
                    fcID = fdb.CreateFeatureClass(destDS.DatasetName,
                                                      fcname,
                                                      geomDef,
                                                      (fieldTranslation == null) ?
                                                      ((sourceFC.Fields != null) ? (IFields)sourceFC.Fields.Clone() : new Fields()) :
                                                      fieldTranslation.DestinationFields);
                }
                if (fcID < 0)
                {
                    _errMsg = "Can't create featureclass " + fcname + "...\r\n" + fdb.lastErrorMsg;
                    destDS.Dispose();
                    return false;
                }

                destLayer = destDS[fcname] as IDatasetElement;

                if (destLayer == null || !(destLayer.Class is IFeatureClass))
                {
                    _errMsg = "Can't load featureclass " + fcname + "...\r\n" + destDS.lastErrorMsg;
                    destDS.Dispose();
                    return false;
                }
                IFeatureClass destFC = destLayer.Class as IFeatureClass;

                if (project && destFC.SpatialReference != null && !destFC.SpatialReference.Equals(sourceFC.SpatialReference))
                {
                    _transformer = new GeometricTransformer();
                    //_transformer.FromSpatialReference = sourceFC.SpatialReference;
                    //_transformer.ToSpatialReference = destFC.SpatialReference;
                    _transformer.SetSpatialReferences(sourceFC.SpatialReference, destFC.SpatialReference);
                }

                if (!Envelope.IsNull(sIndexDef.SpatialIndexBounds) &&
                    sIndexDef.SpatialReference != null && !sIndexDef.SpatialReference.Equals(destFC.SpatialReference))
                {
                    if (!sIndexDef.ProjectTo(destFC.SpatialReference))
                    {
                        _errMsg = "Can't project SpatialIndex Boundaries...";
                        destDS.Dispose();
                        return false;
                    }
                }

                DualTree tree = null;
                BinaryTree2Builder tree2 = null;

                if (msSpatial)
                {
                    ((SqlFDB)fdb).SetMSSpatialIndex((MSSpatialIndex)sIndexDef, destFC.Name);
                    ((SqlFDB)fdb).SetFeatureclassExtent(destFC.Name, sIndexDef.SpatialIndexBounds);
                }
                else
                {
                    if (_treeVersion == TreeVersion.BinaryTree)
                    {
                        tree = SpatialIndex(sourceFC, sIndexDef.MaxPerNode, filters);
                        if (tree == null) return false;
                    }
                    else if (_treeVersion == TreeVersion.BinaryTree2)
                    {
                        if (_schemaOnly && sourceFC.Dataset.Database is IImplementsBinarayTreeDef)
                        {
                            BinaryTreeDef tDef = ((IImplementsBinarayTreeDef)sourceFC.Dataset.Database).BinaryTreeDef(sourceFC.Name);
                            tree2 = new BinaryTree2Builder(tDef.Bounds, tDef.MaxLevel, tDef.MaxPerNode, tDef.SplitRatio);
                        }
                        else
                        {
                            tree2 = SpatialIndex2(fdb, sourceFC, sIndexDef, filters);
                            if (tree2 == null) return false;
                        }
                    }

                    // Vorab einmal alle "Bounds" festlegen, damit auch
                    // ein aufzubauender Layer geviewt werden kann
                    if (_treeVersion == TreeVersion.BinaryTree2 && fdb is AccessFDB)
                    {
                        if (ReportAction != null) ReportAction(this, "Insert spatial index nodes");
                        List<long> nids = new List<long>();
                        foreach (BinaryTree2BuilderNode node in tree2.Nodes)
                        {
                            nids.Add(node.Number);
                        }
                        ((AccessFDB)fdb).ShrinkSpatialIndex(fcname, nids);

                        if (ReportAction != null) ReportAction(this, "Set spatial index bounds");
                        //((AccessFDB)fdb).SetSpatialIndexBounds(fcname, "BinaryTree2", tree2.Bounds, sIndexDef.SplitRatio, sIndexDef.MaxPerNode, tree2.maxLevels);
                        ((AccessFDB)fdb).SetSpatialIndexBounds(fcname, "BinaryTree2", tree2.Bounds, tree2.SplitRatio, tree2.MaxPerNode, tree2.maxLevels);
                        ((AccessFDB)fdb).SetFeatureclassExtent(fcname, tree2.Bounds);
                    }
                }
                if (_cancelTracker.Continue)
                {
                    bool result = true;
                    if (!_schemaOnly)
                    {
                        if (msSpatial)
                        {
                            result = CopyFeatures(sourceFC, fdb, destFC, fieldTranslation, filters);
                        }
                        else if (_treeVersion == TreeVersion.BinaryTree)
                        {
                            if (String.IsNullOrEmpty(sourceFC.IDFieldName)) // SDE Views haben keine ID -> Tree enthält keine Features
                                result = CopyFeatures(sourceFC, fdb, destFC, fieldTranslation, filters);
                            else
                                result = CopyFeatures(sourceFC, fdb, destFC, fieldTranslation, tree);
                        }
                        else if (_treeVersion == TreeVersion.BinaryTree2)
                        {
                            if (String.IsNullOrEmpty(sourceFC.IDFieldName)) // SDE Views haben keine ID -> Tree enthält keine Features
                                result = CopyFeatures(sourceFC, fdb, destFC, fieldTranslation, filters);
                            else
                                result = CopyFeatures2(sourceFC, fdb, destFC, fieldTranslation, tree2);
                        }
                        if (!result)
                        {
                            fdb.DeleteFeatureClass(fcname);
                            destDS.Dispose();
                            return false;
                        }
                    }
                }

                destDS.Dispose();

                if (_cancelTracker.Continue && fdb is AccessFDB)
                {
                    if (ReportAction != null) ReportAction(this, "Calculate extent");
                    ((AccessFDB)fdb).CalculateExtent(destFC);

                    if (msSpatial == false)
                    {
                        if (_treeVersion == TreeVersion.BinaryTree)
                        {
                            if (ReportAction != null) ReportAction(this, "Set spatial index bounds");
                            ((AccessFDB)fdb).SetSpatialIndexBounds(fcname, "BinaryTree", tree.Bounds, sIndexDef.SplitRatio, sIndexDef.MaxPerNode, 0);

                            if (ReportAction != null) ReportAction(this, "Insert spatial index nodes");
                            ((AccessFDB)fdb).__intInsertSpatialIndexNodes2(fcname, tree.Nodes);
                        }
                    }
                    return true;
                }
                else
                {
                    fdb.DeleteFeatureClass(fcname);
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

        private DualTree SpatialIndex(IFeatureClass fc, int maxPerNode, List<IQueryFilter> filters)
        {
            if (fc == null || fc.Envelope == null) return null;

            if (filters == null)
            {
                QueryFilter filter = new QueryFilter();
                filter.AddField(fc.ShapeFieldName);
                filters = new List<IQueryFilter>();
                filters.Add(filter);
            }

            DualTree dualTree = new DualTree(maxPerNode);

            foreach (IQueryFilter filter in filters)
            {
                IFeatureCursor fCursor = fc.GetFeatures(filter);
                if (fCursor == null)
                {
                    _errMsg = "Fatal error: sourcedb query failed...";
                    return null;
                }

                if (ReportAction != null) ReportAction(this, "Calculate spatial index");
                IEnvelope fcEnvelope = fc.Envelope;
                if (_transformer != null)
                {
                    IGeometry geom = _transformer.Transform2D(fcEnvelope) as IGeometry;
                    if (geom == null)
                    {
                        _errMsg = "SpatialIndex: Can't project featureclass extent!";
                        return null;
                    }
                    fcEnvelope = geom.Envelope;
                }
                dualTree.CreateTree(fcEnvelope);  // hier projezieren

                int counter = 0;
                IFeature feat;
                while ((feat = fCursor.NextFeature) != null)
                {
                    if (!_cancelTracker.Continue) break;

                    SHPObject shpObj;

                    IGeometry shape = feat.Shape;
                    if (_transformer != null)
                    {
                        shape = _transformer.Transform2D(shape) as IGeometry;
                    }

                    if (shape != null)
                        shpObj = new SHPObject((int)((uint)feat.OID), shape.Envelope);
                    else
                        shpObj = new SHPObject((int)((uint)feat.OID), null);

                    dualTree.AddShape(shpObj);
                    if ((counter % 1000) == 0 && ReportProgress != null)
                    {
                        ReportProgress(this, counter);
                    }
                    counter++;
                }
                dualTree.FinishIt();
                fCursor.Dispose();

                if (ReportProgress != null)
                {
                    ReportProgress(this, counter);
                }
            }
            return dualTree;
        }

        private BinaryTree2Builder SpatialIndex2(IFeatureDatabase fdb, IFeatureClass fc, ISpatialIndexDef def, List<IQueryFilter> filters)
        {
            if (fc == null) return null;

            IEnvelope bounds = null;
            if (fc.Envelope != null)
                bounds = fc.Envelope;
            else if (fc.Dataset is IFeatureDataset && ((IFeatureDataset)fc.Dataset).Envelope != null)
                bounds = ((IFeatureDataset)fc.Dataset).Envelope;
            if (bounds == null) return null;
            if (_transformer != null)
            {
                IGeometry transBounds = _transformer.Transform2D(bounds) as IGeometry;
                if (transBounds != null)
                    bounds = transBounds.Envelope;
            }
            int maxAllowedLevel = ((fdb is SqlFDB) ? 62 : 30);
            BinaryTree2Builder treeBuilder =
                ((Envelope.IsNull(def.SpatialIndexBounds)) ?
                new BinaryTree2Builder(bounds,
                                ((def.Levels != 0) ? def.Levels : maxAllowedLevel),
                                ((def.MaxPerNode != 0) ? def.MaxPerNode : 500),
                                ((def.SplitRatio != 0.0) ? def.SplitRatio : 0.55)) :
                new BinaryTree2Builder2(def.SpatialIndexBounds, def.Levels, def.MaxPerNode, def.SplitRatio));

            if (filters == null)
            {
                QueryFilter filter = new QueryFilter();
                filter.AddField(fc.ShapeFieldName);
                filters = new List<IQueryFilter>();
                filters.Add(filter);
            }
            foreach (IQueryFilter filter in filters)
            {
                using (IFeatureCursor fCursor = fc.GetFeatures(filter))
                {
                    if (fCursor == null)
                    {
                        _errMsg = "Fatal error: sourcedb query failed...";
                        return null;
                    }

                    if (ReportAction != null) ReportAction(this, "Calculate spatial index");

                    IEnvelope fcEnvelope = bounds;
                    if (_transformer != null)
                    {
                        IGeometry geom = _transformer.Transform2D(fcEnvelope) as IGeometry;
                        if (geom == null)
                        {
                            _errMsg = "SpatialIndex: Can't project featureclass extent!";
                            return null;
                        }
                        fcEnvelope = geom.Envelope;
                    }
                    //treeBuilder = new BinaryTree2Builder(fcEnvelope, maxLevels, maxPerNode);

                    int counter = 0;
                    IFeature feat;
                    while ((feat = fCursor.NextFeature) != null)
                    {
                        if (!_cancelTracker.Continue) break;

                        IGeometry shape = feat.Shape;
                        if (_transformer != null)
                        {
                            shape = _transformer.Transform2D(shape) as IGeometry;
                        }

                        feat.Shape = shape;
                        treeBuilder.AddFeature(feat);

                        if ((counter % 1000) == 0 && ReportProgress != null)
                        {
                            ReportProgress(this, counter);
                        }
                        counter++;
                    }
                    fCursor.Dispose();
                }
            }

            treeBuilder.Trim();
            return treeBuilder;
        }

        private bool CopyFeatures(IFeatureClass source, IFeatureUpdater fdb, IFeatureClass dest, FieldTranslation fTrans, DualTree tree)
        {
            if (tree == null || tree.Nodes == null)
            {
                _errMsg = "Spatial Index is not defined...";
                return false;
            }

            if (ReportAction != null)
            {
                ReportAction(this, "Copy Features (" + dest.Name + ")");
            }
            int featcounter = 0;
            foreach (SpatialIndexNode node in tree.Nodes)
            {
                if (!_cancelTracker.Continue) break;

                RowIDFilter filter = new RowIDFilter(source.IDFieldName);
                filter.IDs = node.IDs;
                filter.SubFields = "*";

                using (IFeatureCursor fCursor = source.GetFeatures(filter))
                {
                    if (fCursor == null)
                    {
                        _errMsg = "Fatal error: sourcedb query failed...";
                        return false;
                    }
                    if (!CopyFeatures(fCursor, node.NID, fdb, dest, fTrans, ref featcounter))
                    {
                        fCursor.Dispose();
                        return false;
                    }
                    fCursor.Dispose();
                }
            }
            if (ReportProgress != null)
            {
                ReportProgress(this, featcounter);
            }
            return true;
        }

        private bool CopyFeatures2(IFeatureClass source, IFeatureUpdater fdb, IFeatureClass dest, FieldTranslation fTrans, BinaryTree2Builder treeBuilder)
        {
            List<BinaryTree2BuilderNode> nodes;

            if (treeBuilder == null || (nodes = treeBuilder.Nodes) == null)
            {
                _errMsg = "Spatial Index is not defined...";
                return false;
            }

            if (ReportAction != null)
            {
                ReportAction(this, "Copy Features (" + dest.Name + ")");
            }
            int featcounter = 0;
            foreach (BinaryTree2BuilderNode node in nodes)
            {
                if (!_cancelTracker.Continue) break;

                RowIDFilter filter = new RowIDFilter(source.IDFieldName);
                filter.IDs = node.OIDs;
                filter.SubFields = "*";

                using (IFeatureCursor fCursor = source.GetFeatures(filter))
                {
                    if (fCursor == null)
                    {
                        _errMsg = "Fatal error: sourcedb query failed...";
                        return false;
                    }
                    if (!CopyFeatures(fCursor, node.Number, fdb, dest, fTrans, ref featcounter))
                    {
                        fCursor.Dispose();
                        return false;
                    }
                    fCursor.Dispose();
                }
            }
            if (ReportProgress != null)
            {
                ReportProgress(this, featcounter);
            }
            return true;
        }

        private bool CopyFeatures(IFeatureCursor fCursor, long NID, IFeatureUpdater fdb, IFeatureClass dest, FieldTranslation fTrans, ref int featcounter)
        {
            IFeature feat;

            List<IFeature> features = new List<IFeature>();

            while ((feat = fCursor.NextFeature) != null)
            {
                if (!_cancelTracker.Continue) break;

                feat.Fields.Add(new FieldValue("$FDB_NID", NID));

                if (_transformer != null)
                {
                    feat.Shape = _transformer.Transform2D(feat.Shape) as IGeometry;
                }

                if (fTrans != null) fTrans.RenameFields(feat);
                features.Add(feat);
                if (features.Count >= this.FeatureBufferSize)
                {
                    if (!fdb.Insert(dest, features))
                    {
                        _errMsg = "Fatal error: destdb insert failed...\n" + fdb.lastErrorMsg;
                        return false;
                    }
                    features.Clear();
                }
                featcounter++;
                if ((featcounter % this.FeatureBufferSize) == 0 && ReportProgress != null)
                {
                    ReportProgress(this, featcounter);
                }
            }

            if (features.Count > 0 && _cancelTracker.Continue)
            {
                if (!fdb.Insert(dest, features))
                {
                    _errMsg = "Fatal error: destdb insert failed...\n" + fdb.lastErrorMsg;
                    return false;
                }
                features.Clear();
            }

            return true;
        }

        private bool CopyFeatures(IFeatureClass source, IFeatureUpdater fdb, IFeatureClass dest, FieldTranslation fTrans, List<IQueryFilter> filters)
        {
            if (ReportAction != null)
            {
                ReportAction(this, "Copy Features (" + dest.Name + ")");
            }
            int featcounter = 0;

            List<IFeature> features = new List<IFeature>();

            if (filters == null || filters.Count == 0)
            {
                QueryFilter filter = new QueryFilter();
                filter.SubFields = "*";
                filters = new List<IQueryFilter>();
                filters.Add(filter);
            }
            foreach (IQueryFilter filter in filters)
            {
                using (IFeatureCursor fCursor = source.GetFeatures(filter))
                {
                    IFeature feature;
                    while ((feature = fCursor.NextFeature) != null)
                    {
                        if (!_cancelTracker.Continue) break;

                        if (_transformer != null)
                        {
                            feature.Shape = _transformer.Transform2D(feature.Shape) as IGeometry;
                        }

                        if (fTrans != null) fTrans.RenameFields(feature);
                        features.Add(feature);
                        if (features.Count >= this.FeatureBufferSize)
                        {
                            if (!fdb.Insert(dest, features))
                            {
                                _errMsg = "Fatal error: destdb insert failed...\n" + fdb.lastErrorMsg;
                                return false;
                            }
                            features.Clear();
                        }
                        featcounter++;
                        if ((featcounter % this.FeatureBufferSize) == 0 && ReportProgress != null)
                        {
                            ReportProgress(this, featcounter);
                        }
                    }
                    if (features.Count > 0 && _cancelTracker.Continue)
                    {
                        if (!fdb.Insert(dest, features))
                        {
                            _errMsg = "Fatal error: destdb insert failed...\n" + fdb.lastErrorMsg;
                            return false;
                        }
                        features.Clear();
                    }
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
