using gView.Framework.Data;
using gView.Framework.Data.Metadata;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.DataSources.Shape
{
    internal class CreateSpatialIndexTree
    {
        private SHPFile _file;
        private DualTree _tree;
        private IEnvelope _bounds;

        public CreateSpatialIndexTree(SHPFile file, DualTree tree, IEnvelope bounds)
        {
            _file = file;
            _tree = tree;
            _bounds = bounds;
        }

        public Task Create()
        {
            return Task.Run(() =>
            {
                _tree.CreateTree(_bounds);

                for (uint i = 0; i < _file.Entities; i++)
                {
                    IEnvelope env = _file.ReadEnvelope(i);
                    //if (env == null)
                    //    continue;
                    SHPObject obj = new SHPObject((int)i, env);
                    //bool inserted=_tree.SHPTreeAddShapeId(obj);
                    bool inserted = _tree.AddShape(obj);
                    inserted = false;
                }
                _tree.FinishIt();

                _tree.writeIDXIndex(_file.IDX_Filename);
            });
        }
    }

    /// <summary>
    /// Zusammenfassung für ShapeDataset.
    /// </summary>
    [UseDatasetNameCase(DatasetNameCase.fieldNamesUpper)]
    [MaximumFieldnameLength(10)]
    [gView.Framework.system.RegisterPlugIn("80F48262-D412-41fb-BF43-2D611A2ABF42")]
    public class ShapeDataset : DatasetMetadata, IFeatureDataset
    {
        private string _connectionString = "", _errMsg = "";
        private IntPtr _hTree = (IntPtr)null;
        private double _minX = 0.0, _minY = 0.0, _maxX = 0.0, _maxY = 0.0;
        private List<IDatasetElement> _elements;
        private bool _useGUI = gView.Framework.system.gViewEnvironment.UserInteractive;
        private DatasetState _state = DatasetState.unknown;
        ShapeDatabase _database = new ShapeDatabase();

        public ShapeDataset()
        {
            _elements = new List<IDatasetElement>();
        }

        public void Dispose()
        {
            foreach (IDatasetElement element in _elements)
            {
                if (element.Class is IDisposable)
                {
                    ((IDisposable)element.Class).Dispose();
                }
            }
            _elements.Clear();
        }

        public bool useGUI
        {
            set { _useGUI = value; }
            get { return _useGUI; }
        }

        #region IFeatureDataset Member

        public bool renderImage(gView.Framework.Carto.IDisplay display)
        {
            return false;
        }

        public System.Drawing.Image Bitmap
        {
            get
            {
                return null;
            }
        }

        public bool canRenderImage
        {
            get
            {
                return false;
            }
        }

        public Task<IEnvelope> Envelope()
        {

            return Task.FromResult<IEnvelope>(new Envelope(_minX, _minY, _maxX, _maxY));
        }

        public bool renderLayer(gView.Framework.Carto.IDisplay display, ILayer layer)
        {
            return false;
        }

        public Task<ISpatialReference> GetSpatialReference()
        {
            return Task.FromResult<ISpatialReference>(null);
        }
        public void SetSpatialReference(ISpatialReference sRef) { }

        #endregion

        #region IDataset Member

        public string Query_FieldPostfix
        {
            get
            {
                return "";
            }
        }

        public string Query_FieldPrefix
        {
            get
            {
                return "";
            }
        }

        public IDatasetEnum DatasetEnum
        {
            get
            {
                return /*new FormAddShapefile()*/ null;
            }
        }

        public string DatasetName
        {
            get
            {
                return "ESRI Shapefile";
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }
        public Task<bool> SetConnectionString(string value)
        {
            _connectionString = value;
            _database.DirectoryName = _connectionString;

            return Task.FromResult(true);
        }


        /*
        public bool DeleteSpatialIndex()
        {
            if (_file == null)
            {
                SHPFile file = new SHPFile(ConnectionString);
                bool ret = file.DeleteIDX();
                file.Close();
                return ret;
            }
            else
            {
                return _file.DeleteIDX();
            }
        }
        */

        public DatasetState State
        {
            get { return _state; }
        }

        public Task<bool> Open()
        {
            /*
			_file=new SHPFile(_connectionString);
            
			_minX=_file.Header.Xmin;
			_minY=_file.Header.Ymin;
			_maxX=_file.Header.Xmax;
			_maxY=_file.Header.Ymax;

			// Tree bauen fehlt noch...
			double [] minBounds=new double[4];
			double [] maxBounds=new double[4];
			minBounds[0]=_minX;
			minBounds[1]=_minY;
			maxBounds[0]=_maxX;
			maxBounds[1]=_maxY;
			
			//QuadTree _tree=null; //new QuadTree((int)_file.Entities);
			//_tree.SHPCreateTree(2,10,new Envelope(_minX,_minY,_maxX,_maxY));
			
			if(!_file.IDX_Exists && _file.IDX_Filename!="") 
			{
				DualTree tree=new DualTree(500);

                CreateSpatialIndexTree creator = new CreateSpatialIndexTree(_file, tree, (IEnvelope)(new Envelope(_minX, _minY, _maxX, _maxY)));

                if (_useGUI)
                {
                    Thread thread = new Thread(new ThreadStart(creator.Create));
                    gView.Framework.UI.Dialogs.FormProgress frmProgress = new gView.Framework.UI.Dialogs.FormProgress(tree, thread);
                    frmProgress.Text = "Create Spatial Index...";

                    frmProgress.ShowDialog();
                }
                else
                {
                    creator.Create();
                }      
			}

			gView.Framework.FDB.IIndexTree iTree=null;
			if(_file.IDX_Exists) 
			{
                iTree = new IDXIndexTree(_file.IDX_Filename);
				//iTree=new gView.Framework.FDB.FDBDualTree(_file.IDX_Filename,_file.Title);
			}

			ShapeDatasetElement layer=new ShapeDatasetElement(_file,this,iTree);

			_layers=new List<IDatasetElement>();
			_layers.Add(layer);
            */

            _state = DatasetState.opened;
            return Task.FromResult(true);
        }

        public string LastErrorMessage
        {
            get
            {
                return _errMsg;
            }
            set { _errMsg = value; }
        }

        public Task<List<IDatasetElement>> Elements()
        {
            return Task.FromResult(_elements);
        }


        public string DatasetGroupName
        {
            get
            {
                return "Shape";
            }
        }

        public string ProviderName
        {
            get
            {
                return "Shapefile";
            }
        }

        public gView.Framework.FDB.IDatabase Database
        {
            get
            {
                return _database;
            }
        }

        async public Task<IDatasetElement> Element(string title)
        {
            foreach (IDatasetElement element in _elements)
            {
                if (element == null)
                {
                    continue;
                }

                if (element.Title == title)
                {
                    return element;
                }
            }
            try
            {
                if (title.ToLower().EndsWith(".shp"))
                {
                    title = title.Substring(0, title.Length - 4);
                }
                DirectoryInfo di = new DirectoryInfo(_connectionString);
                FileInfo[] fi = di.GetFiles(title + ".shp");
                if (fi.Length == 0)
                {
                    _errMsg = "Can't find shapefile...";
                    return null;
                }
                SHPFile shpFile = new SHPFile(fi[0].FullName);

                FileInfo idx = new FileInfo(shpFile.IDX_Filename);
                if (!idx.Exists ||
                    idx.LastWriteTime < shpFile.LastWriteTime)
                {
                    DualTree tree = new DualTree(500);

                    CreateSpatialIndexTree creator = new CreateSpatialIndexTree(shpFile, tree, (IEnvelope)(new Envelope(shpFile.Header.Xmin, shpFile.Header.Ymin, shpFile.Header.Xmax, shpFile.Header.Ymax)));

                    if (_useGUI)
                    {
                        IProgressTaskDialog progress = ProgressDialog.CreateProgressDialogInstance();
                        if (progress != null && progress.UserInteractive)
                        {
                            progress.Text = "Create Spatial Index...";
                            progress.ShowProgressDialog(tree, creator.Create());
                        }
                        else
                        {
                            await creator.Create();
                        }
                    }
                    else
                    {
                        await creator.Create();
                    }
                }

                gView.Framework.FDB.IIndexTree iTree = null;
                if (shpFile.IDX_Exists)
                {
                    iTree = new IDXIndexTree(shpFile.IDX_Filename);
                }

                ShapeDatasetElement element = new ShapeDatasetElement(shpFile, this, iTree);
                _elements.Add(element);
                return element;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
            }
            return null;
        }

        public Task RefreshClasses()
        {
            return Task.CompletedTask;
        }
        #endregion

        #region IPersistableLoadAsync Member

        async public Task<bool> LoadAsync(IPersistStream stream)
        {
            _connectionString = (string)stream.Load("connectionstring", "");
            return await this.Open();
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("connectionstring", _connectionString);
        }

        #endregion

        public bool Delete(string shpName)
        {
            foreach (ShapeDatasetElement element in _elements)
            {
                if (element.Title.ToLower() == shpName.ToLower())
                {
                    return element.Delete();
                }
            }
            return false;
        }

        public bool Rename(string shpName, string newName)
        {
            foreach (ShapeDatasetElement element in _elements)
            {
                if (element.Title.ToLower() == shpName.ToLower())
                {
                    return element.Rename(newName);
                }
            }
            return false;
        }
    }
}
