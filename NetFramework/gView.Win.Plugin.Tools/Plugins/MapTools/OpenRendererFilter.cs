using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.UI;
using gView.Framework.Geometry;
using gView.Framework.UI.Controls.Filter;

namespace gView.Plugins.MapTools
{
    internal class OpenRendererFilter : ExplorerOpenDialogFilter
    {
        private geometryType _geomType = geometryType.Unknown;

        public OpenRendererFilter()
            : base("Map Feature Layer")
        {
            ObjectTypes.Add(typeof(ITOCElement));
        }
        public OpenRendererFilter(geometryType geomType)
            : this()
        {
            _geomType = geomType;
        }

        public override bool Match(IExplorerObject exObject)
        {
            bool match = base.Match(exObject);

            // Grouplayer nicht...
            if (exObject is IExplorerParentObject) return false;

            if (match && exObject != null && exObject.Object is ITOCElement && _geomType != geometryType.Unknown)
            {
                ITOCElement tocElement = exObject.Object as ITOCElement;
                if (tocElement.Layers != null)
                {
                    foreach (ILayer layer in tocElement.Layers)
                    {
                        if (layer is IFeatureLayer && ((IFeatureLayer)layer).FeatureClass != null)
                        {
                            return ((IFeatureLayer)layer).FeatureClass.GeometryType == _geomType;
                        }
                    }
                }
            }
            //if (match && _geomType != geometryType.Unknown && exObject.Object is IFeatureLayer && ((IFeatureLayer)exObject.Object).FeatureClass != null)
            //{
            //    return ((IFeatureLayer)exObject.Object).FeatureClass.GeometryType == _geomType;
            //}
            
            return match;
        }
    }
}
