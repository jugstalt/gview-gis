using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Common;

namespace gView.Framework.Data
{
    public class LayerFactory
    {
        private static bool UseSelectableElement(System.Type t)
        {
            bool selectionSet = true;
            foreach (System.Attribute attr in System.Attribute.GetCustomAttributes(t))
            {
                if (attr is UseWithinSelectableDatasetElements)
                {
                    selectionSet = ((UseWithinSelectableDatasetElements)attr).Value;
                }
            }
            return selectionSet;
        }

        public static ILayer Create(IClass Class)
        {
            return Create(Class, true, null);
        }
        public static ILayer Create(IClass Class, IWebServiceClass serviceClass)
        {
            return Create(Class, true, serviceClass);
        }
        private static ILayer Create(IClass Class, bool initalize)
        {
            return Create(Class, initalize, null);
        }
        private static ILayer Create(IClass Class, bool initalize, IWebServiceClass serviceClass)
        {
            if (Class is IWebFeatureClass)
            {
                IWebServiceTheme theme;
                if (UseSelectableElement(Class.GetType()))
                {
                    theme = new WebServiceTheme2(Class, Class.Name, ((IWebFeatureClass)Class).ID, false, serviceClass);
                }
                else
                {
                    theme = new WebServiceTheme(Class, Class.Name, ((IWebFeatureClass)Class).ID, false, serviceClass);
                }

                if (initalize && theme.FeatureClass != null)
                {
                    if (theme.FeatureClass.GeometryType == GeometryType.Unknown)
                    {
                        theme.FeatureRenderer = null;
                        theme.LabelRenderer = null;
                        IFeatureRenderer renderer = PlugInManager.Create(KnownObjects.Carto_UniversalGeometryRenderer) as IFeatureRenderer;
                        theme.SelectionRenderer = renderer;
                    }
                    else
                    {
                        IFeatureRenderer2 renderer = PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer2;
                        if (renderer is ISymbolCreator && renderer.CanRender(theme, null))
                        {
                            theme.FeatureRenderer = null;
                            theme.LabelRenderer = null;
                            renderer.Symbol = ((ISymbolCreator)renderer).CreateStandardSelectionSymbol(theme.FeatureClass.GeometryType);
                            theme.SelectionRenderer = renderer;
                        }
                        else if (renderer != null)
                        {
                            renderer.Release();
                            renderer = null;
                        }
                    }
                }
                return theme;
            }
            else if (Class is IWebRasterClass)
            {
                IWebServiceTheme theme = new WebServiceTheme(Class, Class.Name, ((IWebRasterClass)Class).ID, false, serviceClass);
                return theme;
            }
            else if (Class is IWebServiceClass)
            {
                WebServiceLayer wsLayer = new WebServiceLayer(Class as IWebServiceClass);
                return wsLayer;
            }
            else if (Class is IRasterCatalogClass)
            {
                IRasterCatalogLayer layer = new RasterCatalogLayer(Class as IRasterCatalogClass);
                if (initalize)
                {
                    IFeatureRenderer2 renderer = PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer2;
                    if (renderer is ISymbolCreator && renderer.CanRender(layer, null))
                    {
                        //layer.FeatureRenderer = renderer;
                        //renderer.Symbol = ((ISymbolCreator)renderer).CreateStandardSymbol(layer.FeatureClass.GeometryType);

                        //IFeatureRenderer2 selectionRenderer = ComponentManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer2;
                        //selectionRenderer.Symbol = ((ISymbolCreator)selectionRenderer).CreateStandardSelectionSymbol(layer.FeatureClass.GeometryType);
                        //layer.SelectionRenderer = selectionRenderer;
                        renderer.Symbol = ((ISymbolCreator)renderer).CreateStandardSelectionSymbol(layer.FeatureClass.GeometryType);
                        layer.SelectionRenderer = renderer;
                    }
                    else if (renderer != null)
                    {
                        renderer.Release();
                        renderer = null;
                    }
                }

                return layer;
            }
            else if (Class is IFeatureClass)
            {
                IFeatureLayer layer;
                if (UseSelectableElement(Class.GetType()))
                {
                    layer = new FeatureLayer2(Class as IFeatureClass);
                }
                else
                {
                    layer = new FeatureLayer(Class as IFeatureClass);
                }

                if (initalize && layer.FeatureClass != null)
                {
                    if (layer.FeatureClass.GeometryType == GeometryType.Unknown)
                    {
                        layer.FeatureRenderer = PlugInManager.Create(KnownObjects.Carto_UniversalGeometryRenderer) as IFeatureRenderer; ;
                        layer.SelectionRenderer = PlugInManager.Create(KnownObjects.Carto_UniversalGeometryRenderer) as IFeatureRenderer;
                    }
                    else
                    {
                        IFeatureRenderer2 renderer = PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer2;
                        if (renderer is ISymbolCreator && renderer.CanRender(layer, null))
                        {
                            layer.FeatureRenderer = renderer;
                            renderer.Symbol = ((ISymbolCreator)renderer).CreateStandardSymbol(layer.LayerGeometryType/*layer.FeatureClass.GeometryType*/);

                            IFeatureRenderer2 selectionRenderer = PlugInManager.Create(KnownObjects.Carto_SimpleRenderer) as IFeatureRenderer2;
                            selectionRenderer.Symbol = ((ISymbolCreator)selectionRenderer).CreateStandardSelectionSymbol(layer.FeatureClass.GeometryType);
                            layer.SelectionRenderer = selectionRenderer;
                        }
                        else if (renderer != null)
                        {
                            renderer.Release();
                            renderer = null;

                            IFeatureRenderer uRenderer = PlugInManager.Create(KnownObjects.Carto_UniversalGeometryRenderer) as IFeatureRenderer;
                            layer.FeatureRenderer = uRenderer;
                        }
                    }
                }

                return layer;
            }
            else if (Class is IRasterClass)
            {
                RasterLayer layer = new RasterLayer(Class as IRasterClass);

                return layer;
            }

            return null;
        }

        public static ILayer Create(IClass Class, ILayer protoType)
        {
            return Create(Class, protoType, null);
        }
        public static ILayer Create(IClass Class, ILayer protoType, IWebServiceClass serviceClass)
        {
            if (protoType is GroupLayer && Class == null)
            {
                GroupLayer grLayer = new GroupLayer();
                grLayer.Title = protoType.Title;
                grLayer.CopyFrom(protoType);
                return grLayer;
            }

            ILayer layer = Create(Class, false, serviceClass);
            if (layer is DatasetElement)
            {
                ((DatasetElement)layer).CopyFrom(protoType);
                //((DatasetElement)layer).Class2 = Class;   
            }
            return layer;
        }
    }
}
