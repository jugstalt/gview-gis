using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.UI;
using gView.Framework.UI.Events;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.Carto;
using gView.Framework.UI.Dialogs;
using gView.Framework.system;
using gView.Framework.Globalisation;
using gView.Framework.UI.Controls.Filter;

namespace gView.Plugins.MapTools
{
    [gView.Framework.system.RegisterPlugIn("11BA5E40-A537-4651-B475-5C7C2D65F36E")]
    public class AddData : ITool, IMapContextMenuItem
    {
        IMapDocument _doc = null;

        #region ITool Member

        public bool Enabled
        {
            get
            {
                if (_doc == null || _doc.Application == null) return false;

                if (_doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true) return false;

                //LicenseTypes lt = _doc.Application.ComponentLicenseType("gview.desktop;gview.map");
                //return (lt == LicenseTypes.Licensed || lt == LicenseTypes.Express);
                return true;
            }
        }

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.AddData", "Add Data..."); }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent)) return;
            IMap map = ((MapEvent)MapEvent).Map;

            bool firstDataset = (map[0] == null);

            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenDataFilter());

            ExplorerDialog dlg = new ExplorerDialog("Add data...", filters, true);

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<IDataset> datasets = dlg.Datasets;


                FormDatasetProperties datasetProps = new FormDatasetProperties(datasets);
                try
                {
                    if (((MapEvent)MapEvent).UserData == null)
                    {
                        if (datasetProps.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                            return;
                    }
                }
                catch  // Kann ObjectDisposed Exception werfen...
                {
                    return;
                }


                Envelope env = null;
                foreach (ILayer layer in datasetProps.Layers)
                {
                    ISpatialReference sRef = null;
                    IEnvelope classEnv = null;
                    if (layer is IFeatureLayer && ((IFeatureLayer)layer).FeatureClass != null && ((IFeatureLayer)layer).FeatureClass.Envelope != null)
                    {
                        sRef = ((IFeatureLayer)layer).FeatureClass.SpatialReference;
                        classEnv = ((IFeatureLayer)layer).FeatureClass.Envelope;
                    }
                    else if (layer is IRasterLayer && ((IRasterLayer)layer).RasterClass != null && ((IRasterLayer)layer).RasterClass.Polygon != null && ((IRasterLayer)layer).RasterClass.Polygon.Envelope != null)
                    {
                        sRef = ((IRasterLayer)layer).RasterClass.SpatialReference;
                        classEnv = ((IRasterLayer)layer).RasterClass.Polygon.Envelope;
                    }
                    else if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null && ((IWebServiceLayer)layer).WebServiceClass.Envelope != null)
                    {
                        sRef = ((IWebServiceLayer)layer).WebServiceClass.SpatialReference;
                        classEnv = ((IWebServiceLayer)layer).WebServiceClass.Envelope;
                    }

                    if (classEnv != null)
                    {
                        if (sRef != null && !sRef.Equals(map.Display.SpatialReference))
                        {
                            bool found = false;
                            foreach (string p in map.Display.SpatialReference.Parameters)
                            {
                                if (p.ToLower().Trim() == "+nadgrids=@null")
                                    found = false;
                            }
                            if (found)
                            {
                                classEnv = null;
                            }
                            else
                            {
                                IGeometry geom = GeometricTransformer.Transform2D(classEnv.ToPolygon(0), sRef, map.Display.SpatialReference);
                                if (geom != null)
                                    classEnv = geom.Envelope;
                                else
                                    classEnv = null;
                            }
                        }
                        if (classEnv != null)
                        {
                            if (env == null)
                                env = new Envelope(classEnv);
                            else
                                env.Union(classEnv);
                        }
                    }

                    map.AddLayer(layer);
                }
                //map.AddDataset(dataset, 0);


                if (env != null && map.Display != null)
                {
                    if (firstDataset)
                    {
                        map.Display.Limit = env;
                        map.Display.ZoomTo(env);
                    }
                    else
                    {
                        IEnvelope limit = map.Display.Limit;
                        limit.Union(env);
                        map.Display.Limit = limit;
                    }
                }
                ((MapEvent)MapEvent).drawPhase = DrawPhase.All;
                ((MapEvent)MapEvent).refreshMap = true;
            }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.add_data; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        #endregion

        #region IContextMenuTool Member


        public bool Enable(object element)
        {
            return true;
        }

        public bool Visible(object element)
        {
            return true;
        }

        public void OnEvent(object element, object parent)
        {
            if (element is IMap)
            {
                MapEvent mapEvent = new MapEvent((IMap)element);
                this.OnEvent(mapEvent);

                if (mapEvent.refreshMap && _doc != null && _doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).RefreshActiveMap(mapEvent.drawPhase);
                }
            }
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 55; }
        }

        #endregion
    }
}
