using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.IO;
using gView.Framework.Core.UI;
using gView.Framework.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Cartography.UI;

internal class TocElement : ITocElement
{
    private string _name;
    private TocElementType _type;
    ITocElement _parent;
    List<IDatasetElement> _layers = new List<IDatasetElement>();
    private Toc _toc;
    private bool _showLegend = true;
    private bool _visible = false;
    private bool _locked = false;

    public TocElement(Toc parentTOC)
    {
        _toc = parentTOC;
    }
    public TocElement(ILayer layer, string name, ITocElement parent, Toc parentTOC)
    {
        _layers.Add(layer);
        _name = name;
        _type = TocElementType.Layer;
        _parent = parent;
        _toc = parentTOC;
        if (layer is ILayer)
        {
            _visible = layer.Visible;
        }
    }
    public TocElement(ILayer layer, string name, ITocElement parent, Toc parentTOC, TocElementType type)
        : this(layer, name, parent, parentTOC)
    {
        _type = type;
    }
    public TocElement(string name, Toc parentTOC)
    {
        _name = name;
        _type = TocElementType.Layer;
        _parent = null;
        _toc = parentTOC;
    }
    public TocElement(string name, ITocElement parent, Toc parentTOC)
    {
        _name = name;
        _type = TocElementType.Layer;
        _parent = parent;
        _toc = parentTOC;
    }
    public TocElement(string name, ITocElement parent, TocElementType type, Toc parentTOC)
    {
        _name = name;
        _type = type;
        _parent = parent;
        _toc = parentTOC;
    }

    internal TocElement Copy(Toc toc, ITocElement parent)
    {
        TocElement elem = new TocElement(toc);
        elem._name = _name;
        elem._type = _type;
        elem._parent = parent;
        elem._layers = ListOperations<IDatasetElement>.Clone(_layers);
        elem._showLegend = _showLegend;
        elem._visible = LayerVisible;
        elem._locked = _locked;

        return elem;
    }

    private static string RecursiveName(TocElement element)
    {
        string name = "";
        RecursiveName(element, ref name);
        return name;
    }
    private static void RecursiveName(TocElement element, ref string name)
    {
        if (element == null)
        {
            return;
        }

        name = name != "" ? element._name + "|" + name : element._name;
        RecursiveName(element.ParentGroup as TocElement, ref name);
    }

    internal static bool layerVisible(ILayer layer)
    {
        if (layer is Layer)
        {
            // Sollte nicht Recursive über Grouplayer bestimmt werden
            IGroupLayer gLayer = layer.GroupLayer;
            ((Layer)layer).GroupLayer = null;
            bool visible = layer.Visible;
            ((Layer)layer).GroupLayer = gLayer;

            return visible;
        }
        else if (layer != null)
        {
            return layer.Visible;
        }
        return false;
    }
    #region ITOCElement Member

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            if (value == null || value == "" || _toc == null)
            {
                return;
            }

            _toc.RenameElement(this, value);
        }
    }

    public TocElementType ElementType
    {
        get
        {
            return _type;
        }
    }

    public List<ILayer> Layers
    {
        get
        {
            List<ILayer> e = new List<ILayer>();
            foreach (ILayer layer in _layers)
            {
                e.Add(layer);
            }
            return e;
        }
    }
    public void RemoveLayer(ILayer layer)
    {
        if (!_layers.Contains(layer))
        {
            return;
        }

        _layers.Remove(layer);
    }
    public void AddLayer(ILayer layer)
    {
        if (_layers.Contains(layer))
        {
            return;
        }

        _layers.Add(layer);
    }
    public ITocElement ParentGroup
    {
        get
        {
            return _parent;
        }
        set
        {
            if (_parent == value)
            {
                return;
            }

            _parent = value;

            if (_layers != null)
            {
                foreach (ILayer layer in _layers)
                {
                    if (layer is Layer l)
                    {
                        if (layer.GroupLayer is GroupLayer prevGroupLayer)
                        {
                            prevGroupLayer.Remove(layer as Layer);
                        }

                        l.GroupLayer =
                            _parent != null && _parent.Layers.Count == 1
                                ? _parent.Layers[0] as IGroupLayer
                                : null;

                        if(layer.GroupLayer is GroupLayer newGroupLayer)
                        {
                            newGroupLayer.Add(l);
                        }
                    }
                }
            }
        }
    }

    public bool LayerVisible
    {
        get
        {
            if (_toc.Modifier == TocModifier.Public)
            {
                if (_type == TocElementType.Layer)
                {
                    foreach (ILayer layer in _layers)
                    {
                        if (layer == null)
                        {
                            continue;
                        }

                        if (layerVisible(layer))
                        {
                            return _visible = true;
                        }
                    }
                    return _visible = false;
                }
                if (_type == TocElementType.OpenedGroup ||
                    _type == TocElementType.ClosedGroup)
                {
                    //return _visible = (_toc.CountVisibleGroupLayers(this, true) > 0);
                    if (_layers.Count == 1 && _layers[0] is IGroupLayer)
                    {
                        return _visible = layerVisible(_layers[0] as ILayer);
                    }
                    else if (_layers.Count == 1 && _layers[0] is IWebServiceLayer)
                    {
                        return _visible = layerVisible(_layers[0] as ILayer);
                    }
                }
                return _visible = false;
            }
            else
            {
                return _visible;
            }
        }
        set
        {
            if (_type == TocElementType.Layer)
            {
                if (_toc.Modifier == TocModifier.Public)
                {
                    foreach (ILayer layer in _layers)
                    {
                        if (layer == null)
                        {
                            continue;
                        }

                        layer.Visible = _visible = value;
                    }
                }
                else
                {
                    _visible = value;
                }
            }
            if (_type == TocElementType.OpenedGroup ||
                _type == TocElementType.ClosedGroup)
            {
                if (_toc.Modifier == TocModifier.Public)
                {
                    if (_layers.Count == 1 && _layers[0] is IGroupLayer)
                    {
                        _visible = ((IGroupLayer)_layers[0]).Visible = value;
                    }
                    else if (_layers.Count == 1 && _layers[0] is IWebServiceLayer)
                    {
                        _visible = ((IWebServiceLayer)_layers[0]).Visible = value;
                    }
                }
                else
                {
                    _visible = value;
                }
                //if(_toc is TOC) 
                //{
                //    ((TOC)_toc).SetGroupVisibility(this,value);
                //}
            }
        }
    }

    public bool LayerLocked
    {
        get { return _locked; }
        set { _locked = value; }
    }
    public bool LegendVisible
    {
        get { return _showLegend; }
        set { _showLegend = value; }
    }

    public void OpenCloseGroup(bool open)
    {
        if (_type == TocElementType.OpenedGroup ||
            _type == TocElementType.ClosedGroup)
        {
            if (open)
            {
                _type = TocElementType.OpenedGroup;
            }
            else
            {
                _type = TocElementType.ClosedGroup;
            }
        }
    }

    public IToc TOC
    {
        get { return _toc; }
    }
    #endregion

    public void rename(string newName)
    {
        _name = newName;
        foreach (ILayer layer in _layers)
        {
            if (layer is IGroupLayer)
            {
                ((IGroupLayer)layer).Title = newName;
            }
        }
    }
    public List<IDatasetElement> LayersList
    {
        get { return _layers; }
    }

    #region IPersistable Member

    async public Task<bool> LoadAsync(IPersistStream stream)
    {
        _name = (string)stream.Load("Name");
        _type = (TocElementType)stream.Load("Type");
        _showLegend = (bool)stream.Load("legend", false);
        _locked = (bool)stream.Load("locked", false);

        if ((_type == TocElementType.ClosedGroup || _type == TocElementType.OpenedGroup) &&
            _name.IndexOf("|") != -1)
        {
            int pos = _name.LastIndexOf("|");
            _name = _name.Substring(pos + 1, _name.Length - pos - 1);
        }

        if (_toc.Modifier == TocModifier.Private)
        {
            _visible = (bool)stream.Load("visible", false);
        }

        string parentName = (string)stream.Load("Parent");
        if (parentName != null)
        {
            foreach (ITocElement group in _toc.GroupElements)
            {
                if (RecursiveName(group as TocElement) == parentName)
                {
                    _parent = group;
                    break;
                }
            }
        }

        _layers.Clear();
        if (_type == TocElementType.ClosedGroup || _type == TocElementType.OpenedGroup)
        {
            PersistLayer pElement = null;

            pElement = await stream.LoadAsync("DatasetElement", new PersistLayer(_toc._map));

            if (pElement != null && pElement.DatasetElement != null)
            {
                ILayer gLayer = null;
                foreach (IDatasetElement dsElement in _toc._map.MapElements)
                {
                    if (dsElement is IGroupLayer &&
                        dsElement.ID == pElement.DatasetElement.ID/*RecursiveName(this)*/)
                    {
                        gLayer = dsElement as IGroupLayer;
                        break;
                    }
                }

                if (gLayer == null)
                {
                    // wenn Gruppe ein WebServiceLayer ist....
                    gLayer = pElement.DatasetElement as ILayer;

                    //
                    // Für alte Versionen: Suchen, ob Grouplayer in Karte vorhanden
                    // Wenn nein: einfügen
                    //
                    if (gLayer == null)
                    {
                        _toc._map.AddLayer(gLayer = new GroupLayer(RecursiveName(this)));
                    }
                }

                if (gLayer != null)
                {
                    _layers.Add(gLayer);
                    if (_parent != null && _parent.Layers.Count == 1 && _parent.Layers[0] is GroupLayer)
                    {
                        ((GroupLayer)_parent.Layers[0]).Add(gLayer as Layer);
                    }
                }
            }
        }
        else
        {
            PersistLayer pElement = null;

            while ((pElement = await stream.LoadAsync("DatasetElement", new PersistLayer(_toc._map))) != null)
            {
                _layers.Add(pElement.DatasetElement);  // DatasetElement kann auch null sein, wenn (vorübergehend) nicht mehr im Dataset...
                if (_parent != null && _parent.Layers.Count == 1 && _parent.Layers[0] is GroupLayer)
                {
                    ((GroupLayer)_parent.Layers[0]).Add(pElement.DatasetElement as Layer);
                }
            }
        }

        return true;
    }

    public void Save(IPersistStream stream)
    {
        stream.Save("Name", _type == TocElementType.OpenedGroup || _type == TocElementType.ClosedGroup ? RecursiveName(this) : _name);
        stream.Save("Type", (int)_type);
        stream.Save("legend", _showLegend);
        stream.Save("locked", _locked);

        if (_toc.Modifier == TocModifier.Private)
        {
            stream.Save("visible", _visible);
        }
        if (_parent != null)
        {
            stream.Save("Parent", RecursiveName(_parent as TocElement));
        }
        foreach (ILayer layer in _layers)
        {
            PersistLayer pElement = new PersistLayer(_toc._map, layer);
            stream.Save("DatasetElement", pElement);
        }
    }

    #endregion
}
