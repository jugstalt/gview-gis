using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using gView.Framework.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.Carto
{
    public interface IMap : IDisposable, 
                            IClone, 
                            IMetadata, 
                            IDataCopyright, 
                            IPersistableLoadAsync
    {
        event LayerAddedEvent LayerAdded;
        event LayerRemovedEvent LayerRemoved;
        event TOCChangedEvent TOCChanged;
        event EventHandler MapRenamed;
        event UserIntefaceEvent OnUserInterface;

        void AddDataset(IDataset dataset, int order);
        void RemoveDataset(IDataset dataset);
        void RemoveAllDatasets();

        void AddLayer(ILayer layer);
        void AddLayer(ILayer layer, int pos);
        void RemoveLayer(ILayer layer);

        IDataset this[int datasetIndex]
        {
            get;
        }
        IDataset this[IDatasetElement element]
        {
            get;
        }
        IEnumerable<IDataset> Datasets { get; }
        string Name { get; set; }

        string Title { get; set; }

        Task<List<IDatasetElement>> Elements(string aliasname);
        List<IDatasetElement> MapElements { get; }
        Task<List<IDatasetElement>> ActiveLayers();

        IDatasetElement DatasetElementByClass(IClass cls);

        string ActiveLayerNames
        {
            get;
            set;
        }
        void ClearSelection();

        IToc TOC { get; }

        ISelectionEnvironment SelectionEnvironment { get; }

        IDisplay Display { get; }

        void Release();


        ISpatialReference LayerDefaultSpatialReference
        {
            get;
            set;
        }

        IEnumerable<string> ErrorMessages { get; }
        bool HasErrorMessages { get; }

        void ResetRequestExceptions();

        bool HasRequestExceptions { get; }

        IEnumerable<Exception> RequestExceptions { get; }

        void Compress();

        string GetLayerDescription(int layerId);
        void SetLayerDescription(int layerId, string description);

        string GetLayerCopyrightText(int layerId);
        void SetLayerCopyrightText(int layerId, string copyrightText);

        IResourceContainer ResourceContainer { get; }
        IMapEventHooks MapEventHooks { get; }
        IMapServiceProperties MapServiceProperties { get; }
    }
}