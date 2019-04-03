using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.Symbology;
using gView.MapServer;
using gView.Framework.FDB;
using gView.Framework.IO;

namespace gView.Framework.system.UI
{
    /// <summary>
    /// Zusammenfassung für AssemblyExplorer.
    /// </summary>
    public class AssemblyExplorer
    {
        public delegate void AddPluginEvent(string Name, Type type);
        public event AddPluginEvent AddPlugin = null;
        public delegate void AddPluginExceptionEvent(object sender, AddPluginExceptionEventArgs args);
        public event AddPluginExceptionEvent AddPluginException = null;

        public List<Type> _types;

        public AssemblyExplorer()
        {
            _types = new List<Type>()
            {
                typeof(IDataset),
                typeof(ITool),
                typeof(IToolbar),
                typeof(IExTool),
                typeof(IExToolbar),
                typeof(IDatasetElementContextMenuItem),
                typeof(IMapContextMenuItem),
                typeof(IFeatureRenderer),
                typeof(ILabelRenderer),
                typeof(ISymbol),
                typeof(IDockableWindowContainer),
                typeof(IExplorerObject),
                typeof(IExplorerCommand),
                typeof(IExplorerTabPage),
                typeof(IServiceRequestInterpreter),
                typeof(IMapOptionPage),
                typeof(IExplorerOptionPage),
                typeof(ILayerPropertyPage),
                typeof(IGraphicElement2),
                typeof(IAutoField),
                typeof(IFieldDomain),
                typeof(IServiceableDataset),
                typeof(IFileFeatureDatabase),
                typeof(IMetadataProvider),
                typeof(IPersistable),
                typeof(IMapApplicationModule),
                typeof(gView.Framework.GeoProcessing.IActivity),
                typeof(gView.Framework.system.ISimpleNumberCalculation),
                typeof(gView.Framework.Network.INetworkTracer),
                typeof(IFeatureLayerJoin),
                typeof(IFeatureDatabase),
                typeof(ICartoRibbonTab),
                typeof(IExplorerRibbonTab)
            };
        }



        public string Explore(string path)
        {
            try
            {
                //string output=@"C:\temp\assembly.xml";
                StringBuilder xml = new StringBuilder();

                Assembly assembly = Assembly.LoadFrom(path);

                foreach (Module module in assembly.GetModules())
                {
                    foreach (Type type in module.GetTypes())
                    {
                        if (type.IsAbstract || type.IsInterface) continue;

                        RegisterPlugIn plugin = (RegisterPlugIn)Attribute.GetCustomAttribute(type, typeof(RegisterPlugIn));
                        if (plugin == null) continue;

                        try
                        {
                            object wObject = assembly.CreateInstance(type.FullName, false);

                            //object wObject=assembly.CreateInstance(type.FullName,false);
                            //wObject=Activator.CreateInstance(type);

                            string attr = " guid=\"" + plugin.Value + "\"";
                            attr += " fullname=\"" + type.FullName + "\"";
                            attr += " assembly=\"" + ToAssemblyPath(path) + "\"";

                            if (wObject is IPlugInDependencies)
                            {
                                try
                                {
                                    if (((IPlugInDependencies)wObject).HasUnsolvedDependencies() == true)
                                        continue;
                                }
                                catch (Exception dependenciesEx)
                                {
                                    // GDAL kann jammern, weil auch die Referenzierten Assemblies gdal_sharp.dll, ...
                                    // nicht im Pojekt sind!
                                    continue;
                                }
                            }

                            foreach (Type wObjectInterface in wObject.GetType().GetInterfaces())
                            {
                                if (_types.Contains(wObjectInterface))
                                {
                                    xml.Append("<plugin interface=\"" + wObjectInterface.ToString() + "\"" + attr + " />\n");
                                }
                            }
                            #region old method
                            /*
                            if (wObject is IDataset)
                            {
                                if (AddPlugin!=null) AddPlugin(type.Name, typeof(IDataset));
                                xml.Append("<IDataset" + attr + " />\n");
                            }
                            if (wObject is ITool || wObject is IToolMenu)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(ITool));
                                xml.Append("<ITool" + attr + " />\n");
                            }
                            if (wObject is IToolbar)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IToolbar));
                                xml.Append("<IToolbar" + attr + " />\n");
                            }
                            if (wObject is IExTool || wObject is IExToolMenu)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IExTool));
                                xml.Append("<IExTool" + attr + " />\n");
                            }
                            if (wObject is IExToolbar)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IExToolbar));
                                xml.Append("<IExToolbar" + attr + " />\n");
                            }
                            if (wObject is IDatasetElementContextMenuItem)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IDatasetElementContextMenuItem));
                                xml.Append("<IDatasetElementContextMenuItem" + attr + " />\n");
                            }
                            if (wObject is IMapContextMenuItem)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IMapContextMenuItem));
                                xml.Append("<IMapContextMenuItem" + attr + " />\n");
                            }
                            if (wObject is IFeatureRenderer)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IFeatureRenderer));
                                xml.Append("<IFeatureRenderer" + attr + " />\n");
                            }
                            if (wObject is ILabelRenderer)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(ILabelRenderer));
                                xml.Append("<ILabelRenderer" + attr + " />\n");
                            }
                            if (wObject is ISymbol)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(ISymbol));
                                xml.Append("<ISymbol" + attr + " />\n");
                            }
                            if (wObject is IDockableWindowContainer)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IDockableWindowContainer));
                                xml.Append("<IDockableWindowContainer" + attr + " />\n");
                            }
                            if (wObject is IExplorerObject)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IExplorerObject));
                                xml.Append("<IExplorerObject" + attr + " />\n");
                            }
                            if (wObject is IExplorerCommand)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IExplorerCommand));
                                xml.Append("<IExplorerCommand" + attr + " />\n");
                            }
                            if (wObject is IExplorerTabPage)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IExplorerTabPage));
                                xml.Append("<IExplorerTabPage" + attr + " />\n");
                            }
                            if (wObject is IServiceRequestInterpreter)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IServiceRequestInterpreter));
                                xml.Append("<IServiceRequestInterpreter" + attr + " />\n");
                            }
                            if (wObject is IMapOptionPage)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IMapOptionPage));
                                xml.Append("<IMapOptionPage" + attr + " />\n");
                            }
                            if (wObject is IExplorerOptionPage)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IExplorerOptionPage));
                                xml.Append("<IExplorerOptionPage" + attr + " />\n");
                            }
                            if (wObject is ILayerPropertyPage)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(ILayerPropertyPage));
                                xml.Append("<ILayerPropertyPage" + attr + " />\n");
                            }
                            if (wObject is IGraphicElement2)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IGraphicElement2));
                                xml.Append("<IGraphicElement2" + attr + " />\n");
                            }
                            if (wObject is IAutoField)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IAutoField));
                                xml.Append("<IAutoField" + attr + " />\n");
                            }
                            if (wObject is IFieldDomain)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IFieldDomain));
                                xml.Append("<IFieldDomain" + attr + " />\n");
                            }
                            if (wObject is IServiceableDataset)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IServiceableDataset));
                                xml.Append("<IServiceableDataset" + attr + " />\n");
                            }
                            if (wObject is IFileFeatureDatabase)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IFileFeatureDatabase));
                                xml.Append("<IFileFeatureDatabase" + attr + " />\n");
                            }
                            if (wObject is IMetadataProvider)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IMetadataProvider));
                                xml.Append("<IMetadataProvider" + attr + " />\n");
                            }
                            if (wObject is IPersistable)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IPersistable));
                                xml.Append("<IPersistable" + attr + " />\n");
                            }
                            if (wObject is IMapApplicationModule)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IMapApplicationModule));
                                xml.Append("<IMapApplicationModule" + attr + " />\n");
                            }
                            if (wObject is gView.Framework.GeoProcessing.IActivity)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(gView.Framework.GeoProcessing.IActivity));
                                xml.Append("<IActivity" + attr + " />\n");
                            }
                            if (wObject is gView.Framework.system.ISimpleNumberCalculation)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(gView.Framework.system.ISimpleNumberCalculation));
                                xml.Append("<ISimpleNumberCalculation" + attr + " />\n");
                            }
                            if (wObject is gView.Framework.Network.INetworkTracer)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(gView.Framework.Network.INetworkTracer));
                                xml.Append("<INetworkTracer" + attr + " />\n");
                            }
                            if (wObject is IFeatureLayerJoin)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IFeatureLayerJoin));
                                xml.Append("<IFeatureLayerJoin" + attr + " />\n");
                            }
                            if (wObject is IFeatureDatabase)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IFeatureDatabase));
                                xml.Append("<IFeatureDatabase" + attr + " />\n");
                            }
                            if (wObject is ICartoRibbonTab)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(ICartoRibbonTab));
                                xml.Append("<ICartoRibbonTab" + attr + " />\n");
                            }
                            if (wObject is IExplorerRibbonTab)
                            {
                                if (AddPlugin != null) AddPlugin(type.Name, typeof(IExplorerRibbonTab));
                                xml.Append("<IExplorerRibbonTab" + attr + " />\n");
                            }
                             * */
                            #endregion
                        }
                        catch(Exception ex) 
                        {
                            if (AddPluginException != null)
                            {
                                AddPluginExceptionEventArgs args = new AddPluginExceptionEventArgs(type, ex);
                                AddPluginException(this, args);

                                if (args.Cancel)
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }

                return xml.ToString();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        #region Helper

        private string ToAssemblyPath(string path)
        {
            if(SystemVariables.IsPortable)
            {
                path = path.Replace(SystemVariables.ApplicationDirectory, "{APP_PATH}");
            }

            return path;
        }

        #endregion
    }

    public class AddPluginExceptionEventArgs : EventArgs
    {
        public bool Cancel = false;
        public Type Type = null;
        public Exception Exception = null;

        internal AddPluginExceptionEventArgs(Type type, Exception ex)
        {
            Type = type;
            Exception = ex;
        }
    }
}
