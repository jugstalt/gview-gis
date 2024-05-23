using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Data;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.UI;

internal class PersistLayer : IPersistableLoadAsync
{
    private IMap _map = null;
    private IDatasetElement _element = new NullLayer();

    public PersistLayer(IMap map)
    {
        _map = map;
    }
    public PersistLayer(IMap map, IDatasetElement element) : this(map)
    {
        _element = element;
    }

    public IDatasetElement DatasetElement
    {
        get { return _element; }
    }

    #region IPersistable Member

    async public Task<bool> LoadAsync(IPersistStream stream)
    {
        if (_map == null)
        {
            return true;
        }

        int datasetIndex = (int)stream.Load("DatasetIndex", -1);

        IDataset dataset = _map[datasetIndex];
        // dataset ist bei Grouplayern immer null, darum kein abbruch
        //if(dataset==null) return;

        bool isWebTheme = (bool)stream.Load("IsWebTheme", false);

        string webThemeId = string.Empty;
        string webClassName = string.Empty;

        if (isWebTheme && dataset != null)
        {
            webThemeId = (string)stream.Load("ID", "");
            webClassName = (string)stream.Load("ClassName", "");

            IDatasetElement wElement = await dataset.Element(webClassName);
            if (wElement == null || !(wElement.Class is IWebServiceClass))
            {
                return true;
            }

            IWebServiceClass wc = wElement.Class as IWebServiceClass;
            if (wc == null || wc.Themes == null)
            {
                return true;
            }

            foreach (IWebServiceTheme theme in wc.Themes)
            {
                if (theme.LayerID == webThemeId)
                {
                    _element = theme;
                    return true;
                }
            }

            return true;
        }

        string name = (string)stream.Load("Name", "");
        int _id_ = (int)stream.Load("_ID_", -1);

        if (_id_ == -1)  // Old Version
        {
            foreach (IDatasetElement layer in _map.MapElements)
            {
                if (layer.Class != null && layer.Class.Dataset == dataset && layer.Title == name)
                {
                    _element = layer;
                    return true;
                }
            }
        }
        else
        {
            foreach (IDatasetElement layer in _map.MapElements)
            {
                // Grouplayer
                if (dataset == null &&
                    layer is IGroupLayer &&
                    layer.ID == _id_ &&
                    layer.Title == name)
                {
                    _element = layer;
                    return true;
                }
                // Layer
                if (layer.Class != null && layer.Class.Dataset == dataset &&
                    layer.Title == name && layer.ID == _id_)
                {
                    _element = layer;
                    return true;
                }
            }
        }

        // für ein späters speichern des projektes die werte merken
        if (_element is NullLayer nullLayer)
        {
            nullLayer.PersistLayerID = _id_;
            nullLayer.PersistDatasetID = datasetIndex;
            nullLayer.PersistIsWebTheme = isWebTheme;
            nullLayer.PersistWebThemeID = webThemeId;
            nullLayer.PersistClassName = webClassName;
            nullLayer.Title = name;
            nullLayer.LastErrorMessage = dataset.LastErrorMessage;
        }

        return true;
    }

    public void Save(IPersistStream stream)
    {
        if (_element == null || _map == null)
        {
            return;
        }

        if (_element is NullLayer nullLayer)
        {
            stream.Save("DatasetIndex", nullLayer.PersistDatasetID);

            if (nullLayer.PersistIsWebTheme)
            {
                stream.Save("ID", nullLayer.PersistWebThemeID);
                stream.Save("IsWebTheme", true);
                stream.Save("ClassName", nullLayer.PersistClassName);
            }
            else
            {
                stream.Save("_ID_", nullLayer.PersistLayerID);
                stream.Save("Name", nullLayer.Title);
            }
        }
        else
        {
            //IDataset dataset = _map[_element.DatasetID];

            stream.Save("DatasetIndex", _element.DatasetID);

            if (_element is IWebServiceTheme)
            {
                stream.Save("ID", ((IWebServiceTheme)_element).LayerID);
                stream.Save("IsWebTheme", true);

                //if (dataset.Elements.Count==1 && dataset.Elements[0].Class != null)
                if (((IWebServiceTheme)_element).ServiceClass != null)
                {
                    //stream.Save("ClassName", dataset.Elements[0].Class.Name);
                    stream.Save("ClassName", ((IWebServiceTheme)_element).ServiceClass.Name);
                }
            }
            else
            {
                stream.Save("_ID_", _element.ID);
                stream.Save("Name", _element.Title);
            }
        }

        return;
    }

    #endregion
}
