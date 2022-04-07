using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.Framework.Carto;
using System.Xml;
using gView.Framework.FDB;
using gView.Explorer.UI;
using gView.Framework.GeoProcessing;
using System.Threading.Tasks;

namespace gView.Framework.UI.Controls.Filter
{
    public class ExplorerDialogFilter
    {
        public bool BrowseAll = false;

        private List<Guid> _guids = new List<Guid>();
        private string _name;

        public ExplorerDialogFilter(string name)
        {
            _name = name;
        }

        public List<Guid> ExplorerObjectGUIDs { get { return _guids; } }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual Task<bool> Match(IExplorerObject exObject)
        {
            if (exObject == null) return Task.FromResult(false);

            bool found = false;
            foreach (Guid guid in ExplorerObjectGUIDs)
            {
                if (PlugInManager.PlugInID(exObject) == guid)
                {
                    found = true;
                    break;
                }
            }
            return Task.FromResult(found);
        }

        public virtual object FilterObject
        {
            get { return null; }
        }
    }

    public class ExplorerOpenDialogFilter : ExplorerDialogFilter
    {
        public ExplorerOpenDialogFilter(string name)
            : base(name)
        {
        }
        private List<Type> _types = new List<Type>();
        
        public List<Type> ObjectTypes { get { return _types; } }

        async public override Task<bool> Match(IExplorerObject exObject)
        {
            Type objType = exObject.ObjectType;
            if (exObject == null) return false;

            //object obj = exObject.Object;
            
            bool found = false;
            if (objType != null)
            {
                foreach (Type type in ObjectTypes)
                {
                    if (objType == type)
                    {
                        found = true;
                        break;
                    }
                    foreach (Type inter in objType.GetInterfaces())
                    {
                        if (inter == type)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }
            if (!found) found = await base.Match(exObject);
            return found;
        }
    }

    //public class ExplorerSaveDialogFilter : ExplorerDialogFilter
    //{
    //    private List<Guid> _parentGuids = new List<Guid>();

    //    public ExplorerSaveDialogFilter(string name)
    //        : base(name)
    //    {
    //    }

    //    public List<Guid> ParentExplorerObjectGUIDs { get { return _parentGuids; } }

    //    public bool MatchParent(IExplorerObject parentObject)
    //    {
    //        if (parentObject == null) return false;

    //        bool found = false;
    //        foreach (Guid guid in ParentExplorerObjectGUIDs)
    //        {
    //            if (parentObject.GUID == guid)
    //            {
    //                found = true;
    //                break;
    //            }
    //        }
    //        return found;
    //    }
    //}

    public class OpenFeatureDatasetFilter : ExplorerOpenDialogFilter 
    {
        public OpenFeatureDatasetFilter() : base ("Feature Dataset")
        {
            this.ObjectTypes.Add(typeof(IFeatureDataset));
        }
    }

    public class OpenFeatureDatasetOrFolder : ExplorerOpenDialogFilter
    {
        public OpenFeatureDatasetOrFolder()
            : base("Feature dataset or folder")
        {
            this.ObjectTypes.Add(typeof(IFeatureDataset));
            this.ExplorerObjectGUIDs.Add(new Guid("458E62A0-4A93-45cf-B14D-2F958D67E522"));
            //this.ExplorerObjectGUIDs.Add(new Guid("A610B342-E911-4c52-8E35-72A69B52440A"));
        } 
    }

    public class OpenImageDatasetFiler : ExplorerOpenDialogFilter
    {
        public OpenImageDatasetFiler()
            : base("Image Dataset")
        {
            this.ObjectTypes.Add(typeof(IImageDataset));
        }
    }

    public class OpenRasterDatasetFiler : ExplorerOpenDialogFilter
    {
        public OpenRasterDatasetFiler()
            : base("Raster Dataset")
        {
            this.ObjectTypes.Add(typeof(IRasterDataset));
        }
    }

    public class OpenFeatureclassFilter : ExplorerOpenDialogFilter
    {
        public OpenFeatureclassFilter()
            : base("Feature Class")
        {
            this.ObjectTypes.Add(typeof(IFeatureClass));
        }
    }

    public class OpenShapeFilter : ExplorerOpenDialogFilter
    {
        public OpenShapeFilter()
            : base("Shapefile")
        {
            this.ExplorerObjectGUIDs.Add(new Guid("665E0CD5-B3DF-436c-91B4-D4C0B3ECA5B9"));  
        }
    }

    public class OpenFDBFeatureclassFilter : ExplorerOpenDialogFilter
    {
        public OpenFDBFeatureclassFilter()
            : base("Shapefile")
        {
            this.ExplorerObjectGUIDs.Add(new Guid("FE6E1EA7-1300-400c-8674-68465859E991"));
            this.ExplorerObjectGUIDs.Add(new Guid("A610B342-E911-4c52-8E35-72A69B52440A"));
        }
    }

    /*
    public class OpenFDBFeatureclassFilter : ExplorerDialogFilter
    {
        public OpenFDBFeatureclassFilter()
            : base("FDB Featureclass")
        {
            ComponentManager compMan = new ComponentManager();
            IExplorerObject exObject = (IExplorerObject)compMan.getComponent(new Guid("4151D4A7-D2BA-4df2-B9BA-733DA2992997"));  // AccessFDBFeatureclassObject
            if (exObject != null)
            {
                this.ObjectTypes.Add(exObject.GetType());
            }
            exObject = (IExplorerObject)compMan.getComponent(new Guid("CBD57219-4138-484c-B6BA-B71CAA191DF5"));  // SqlFDBFeatureclass
            if (exObject != null)
            {
                this.ObjectTypes.Add(exObject.GetType());
            }
        }
    }
    */
    public class OpenFDBDatasetFilter : ExplorerOpenDialogFilter
    {
        public OpenFDBDatasetFilter()
            : base("FDB Dataset")
        {
            this.ExplorerObjectGUIDs.Add(new Guid("06676F47-AA31-4c2f-B703-223FE56F9B1D"));  // AccessFDBDataset
            this.ExplorerObjectGUIDs.Add(new Guid("231E8933-5AD4-4fe3-9DA3-CF806A098902"));  // SqlFDBDataset
        }
    }

    public class OpenDataFilter : ExplorerOpenDialogFilter
    {
        public OpenDataFilter()
            : base("Data")
        {
            this.ObjectTypes.Add(typeof(IFeatureClass));
            this.ObjectTypes.Add(typeof(IFeatureDataset));
            this.ObjectTypes.Add(typeof(IDataset));
            this.ObjectTypes.Add(typeof(IRasterDataset));
            //this.ObjectTypes.Add(typeof(IRasterFile));
            //this.ObjectTypes.Add(typeof(IFeatureLayer));
            //this.ObjectTypes.Add(typeof(IRasterLayer));
            this.ObjectTypes.Add(typeof(IImageDataset));
            this.ObjectTypes.Add(typeof(ILayer));
            this.ObjectTypes.Add(typeof(IRasterClass));
            this.ObjectTypes.Add(typeof(IWebServiceClass));

            this.BrowseAll = true;
        }
    }

    public class OpenFeatureLayerFilter : ExplorerOpenDialogFilter
    {
        private GeometryType _geomType = GeometryType.Unknown;

        public OpenFeatureLayerFilter()
            : base("Map Feature Layer")
        {
            ObjectTypes.Add(typeof(IFeatureLayer));
        }
        public OpenFeatureLayerFilter(GeometryType geomType)
            : this()
        {
            _geomType = geomType;
        }

        async public override Task<bool> Match(IExplorerObject exObject)
        {
            bool match = await base.Match(exObject);

            var instatnce = await exObject.GetInstanceAsync();
            if (match && _geomType!=GeometryType.Unknown && instatnce is IFeatureLayer && ((IFeatureLayer)instatnce).FeatureClass != null)
            {
                return ((IFeatureLayer)instatnce).FeatureClass.GeometryType == _geomType;
            }
            
            return match;
        }
    }

    //public class SaveFDBFeatureclassFilter : ExplorerSaveDialogFilter
    //{
    //    public SaveFDBFeatureclassFilter()
    //        : base("FDB Featureclass")
    //    {
    //        this.ParentExplorerObjectGUIDs.Add(new Guid("06676F47-AA31-4c2f-B703-223FE56F9B1D"));  // AccessFDBDataset
    //        this.ParentExplorerObjectGUIDs.Add(new Guid("231E8933-5AD4-4fe3-9DA3-CF806A098902"));  // SqlFDBDataset

    //        this.ExplorerObjectGUIDs.Add(new Guid("4151D4A7-D2BA-4df2-B9BA-733DA2992997"));  // AccessFDBFeatureclassObject
    //        this.ExplorerObjectGUIDs.Add(new Guid("CBD57219-4138-484c-B6BA-B71CAA191DF5"));  // SqlFDBFeatureclass
    //    }
    //}

    //public class SaveShapefileFilter : ExplorerSaveDialogFilter
    //{
    //    public SaveShapefileFilter()
    //        : base("Shapefile")
    //    {
    //        this.ParentExplorerObjectGUIDs.Add(KnownExplorerObjectIDs.Directory);  // AccessFDBDataset

    //        this.ExplorerObjectGUIDs.Add(new Guid("665E0CD5-B3DF-436c-91B4-D4C0B3ECA5B9"));  // Shapefile
    //    }
    //}

    public class OpenSpatialReferenceFilter : ExplorerOpenDialogFilter
    {
        public OpenSpatialReferenceFilter()
            : base("Spatialreference")
        {
            this.ObjectTypes.Add(typeof(ISpatialReference));
            
            this.BrowseAll = true;
        }
    }

    public class OpenMapFilter : ExplorerOpenDialogFilter
    {
        public OpenMapFilter()
            : base("Map")
        {
            this.ObjectTypes.Add(typeof(IMap));
            
            this.BrowseAll = true;
        }
    }

    public class SaveFeatureClassFilters
    {
        public static List<ExplorerDialogFilter> AllFilters
        {
            get
            {
                List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();

                PlugInManager compMan = new PlugInManager();
                bool dsAdded = false;
                foreach (var dsType in compMan.GetPlugins(Plugins.Type.IDataset))
                {
                    var ds = compMan.CreateInstance<IDataset>(dsType);
                    if (ds == null || 
                        ds.Database == null) continue;

                    if (ds.Database is IFileFeatureDatabase)
                    {
                        filters.Add(new SaveFileFeatureClass(ds));   
                    }
                    else if (ds is IFeatureUpdater)
                    {
                        if (!dsAdded)
                        {
                            filters.Insert(0, new SaveFeatureClass());
                            dsAdded = true;
                        }
                    }
                }

                return filters;
            }
        }

        public static List<ExplorerDialogFilter> DatabaseFilters
        {
            get
            {
                List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();

                PlugInManager compMan = new PlugInManager();
                bool dsAdded = false;
                foreach (var dsType in compMan.GetPlugins(Plugins.Type.IDataset))
                {
                    var ds = compMan.CreateInstance<IDataset>(dsType);
                    if (ds == null ||
                        ds.Database == null) continue;

                    if (ds.Database is IFileFeatureDatabase)
                    {
                        continue;
                    }
                    else if (ds.Database is IFeatureUpdater)
                    {
                        if (!dsAdded)
                        {
                            filters.Insert(0, new SaveFeatureClass());
                            dsAdded = true;
                        }
                    }
                }

                return filters;
            }
        }

        private class SaveFeatureClass : ExplorerDialogFilter
        {
            public SaveFeatureClass()
                : base("Feature Class")
            {
            }

            async public override Task<bool> Match(IExplorerObject exObject)
            {
                if (exObject == null) return false;

                var instance = await exObject.GetInstanceAsync();
                if (instance is IDataset &&
                   ((IDataset)instance).Database is IFeatureUpdater)
                    return true;

                return false;
            }
        }

        private class SaveFileFeatureClass : ExplorerDialogFilter
        {
            IDataset _ds;
            public SaveFileFeatureClass(IDataset ds)
                : base(((IFileFeatureDatabase)ds.Database).DatabaseName)
            {
                _ds = ds;
            }

            public override Task<bool> Match(IExplorerObject exObject)
            {
                return Task.FromResult(exObject is DirectoryObject);
            }

            public override object FilterObject
            {
                get
                {
                    return _ds;
                }
            }
        }
    }

    public class OpenActivityDataFilter : ExplorerOpenDialogFilter 
    {
        private IActivityData _aData;

        public OpenActivityDataFilter(IActivityData aData)
            : base(aData.DisplayName)
        {
            _aData = aData;
        }

        async public override Task<bool> Match(IExplorerObject exObject)
        {
            if (exObject == null) return false;

            var instatnce = await exObject.GetInstanceAsync();
            if (instatnce is IDatasetElement)
                return _aData.ProcessAble((IDatasetElement)instatnce);

            if (instatnce is IClass)
            {
                DatasetElement element = new DatasetElement((IClass)instatnce);
                return _aData.ProcessAble(element);
            }

            return false;
        }
    }
}
