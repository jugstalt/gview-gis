using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace gView.Framework.OGC.WFS
{
    public class CapabilitiesHelper
    {
        public static NumberFormatInfo Nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        private object _caps = null;

        private string _onlineResource;
        private string _getGFTOnlineResource, _postGFTOnlineResource;
        private string _getDFTOnlineResource, _postDFTOnlineResource;
        private List<FeatureType> _feaureTypes = new List<FeatureType>();

        public CapabilitiesHelper(object capabilities)
        {
            _caps = capabilities;

            if (_caps is Version_1_0_0.WFS_CapabilitiesType)
            {
                Version_1_0_0.WFS_CapabilitiesType caps = (Version_1_0_0.WFS_CapabilitiesType)_caps;

                #region OnlineResources
                if (caps.Service.OnlineResource != null)
                    _onlineResource = caps.Service.OnlineResource.ToString();
                foreach (object item in caps.Capability.Request.Items)
                {
                    if (item is Version_1_0_0.GetCapabilitiesType)
                    {
                        Version_1_0_0.GetCapabilitiesType gct = (Version_1_0_0.GetCapabilitiesType)item;
                    }
                    else if (item is Version_1_0_0.DescribeFeatureTypeType)
                    {
                        Version_1_0_0.DescribeFeatureTypeType dft = (Version_1_0_0.DescribeFeatureTypeType)item;
                    }
                    else if (item is Version_1_0_0.GetFeatureTypeType)
                    {
                        Version_1_0_0.GetFeatureTypeType gft = (Version_1_0_0.GetFeatureTypeType)item;
                        foreach (Version_1_0_0.DCPTypeType dtt in gft.DCPType)
                        {
                            foreach (object http in dtt.HTTP)
                            {
                                if (http is Version_1_0_0.GetType)
                                    _getGFTOnlineResource = _getGFTOnlineResource = ((Version_1_0_0.GetType)http).onlineResource;
                                if (http is Version_1_0_0.PostType)
                                    _postDFTOnlineResource = _postGFTOnlineResource = ((Version_1_0_0.PostType)http).onlineResource;
                            }
                        }
                    }
                }

                foreach (object item in caps.Filter_Capabilities.Spatial_Capabilities.Spatial_Operators.Items)
                {

                }
                #endregion

                #region FeatureTypes
                Version_1_0_0.FeatureTypeListType ftl = caps.FeatureTypeList;
                foreach (Version_1_0_0.FeatureTypeType ft in ftl.FeatureType)
                {
                    FeatureType featureType = new FeatureType(ft.Name, ft.Title, ft.SRS);
                    if (ft.LatLongBoundingBox != null && ft.LatLongBoundingBox.Length > 0)
                    {
                        featureType.LatLongBBox[0] = double.Parse(ft.LatLongBoundingBox[0].minx, Nhi);
                        featureType.LatLongBBox[1] = double.Parse(ft.LatLongBoundingBox[0].miny, Nhi);
                        featureType.LatLongBBox[2] = double.Parse(ft.LatLongBoundingBox[0].maxx, Nhi);
                        featureType.LatLongBBox[3] = double.Parse(ft.LatLongBoundingBox[0].maxy, Nhi);
                    }
                    _feaureTypes.Add(featureType);
                }
                #endregion
            }
            else if (_caps is Version_1_1_0.WFS_CapabilitiesType)
            {
                Version_1_1_0.WFS_CapabilitiesType caps = (Version_1_1_0.WFS_CapabilitiesType)_caps;

                #region OnlineResources
                foreach (Version_1_1_0.Operation operation in caps.OperationsMetadata.Operation)
                {
                    if (operation.DCP == null || operation.DCP.Length == 0)
                        continue;

                    switch (operation.name.ToLower())
                    {
                        case "getcapabilities":
                            foreach (Version_1_1_0.DCP dcp in operation.DCP)
                            {
                                Version_1_1_0.HTTP http = dcp.Item;
                                foreach (object item in http.Items)
                                {
                                    if (item is Version_1_1_0.OnlineResourceType)
                                    {
                                        Version_1_1_0.OnlineResourceType or = (Version_1_1_0.OnlineResourceType)item;
                                        _onlineResource = or.href;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "describefeaturetype":
                            foreach (Version_1_1_0.DCP dcp in operation.DCP)
                            {
                                Version_1_1_0.HTTP http = dcp.Item;
                                for (int i = 0; i < http.Items.Length; i++)
                                {
                                    if (http.Items[i] is Version_1_1_0.OnlineResourceType)
                                    {
                                        Version_1_1_0.OnlineResourceType or = (Version_1_1_0.OnlineResourceType)http.Items[i];
                                        if (http.ItemsElementName[i].ToString().ToLower() == "get")
                                            _getDFTOnlineResource = or.href;
                                        else if (http.ItemsElementName[i].ToString().ToLower() == "post")
                                            _postDFTOnlineResource = or.href;
                                    }
                                }
                            }
                            break;
                        case "getfeature":
                            foreach (Version_1_1_0.DCP dcp in operation.DCP)
                            {
                                Version_1_1_0.HTTP http = dcp.Item;
                                for (int i = 0; i < http.Items.Length; i++)
                                {
                                    if (http.Items[i] is Version_1_1_0.OnlineResourceType)
                                    {
                                        Version_1_1_0.OnlineResourceType or = (Version_1_1_0.OnlineResourceType)http.Items[i];
                                        if (http.ItemsElementName[i].ToString().ToLower() == "get")
                                            _getGFTOnlineResource = or.href;
                                        else if (http.ItemsElementName[i].ToString().ToLower() == "post")
                                            _postGFTOnlineResource = or.href;
                                    }
                                }
                            }
                            break;
                    }
                }
                #endregion

                #region FeatureTypes
                Version_1_1_0.FeatureTypeListType ftl = caps.FeatureTypeList;
                foreach (Version_1_1_0.FeatureTypeType ft in ftl.FeatureType)
                {
                    string srs = String.Empty;
                    for (int i = 0; i < ft.Items.Length; i++)
                    {
                        if (ft.ItemsElementName[i].ToString() == "DefaultSRS")
                        {
                            srs = ft.Items[i].ToString();
                            break;
                        }
                    }

                    FeatureType featureType = new FeatureType(ft.Name.Name, ft.Title, srs, ft.OutputFormats.Format);
                    if (ft.WGS84BoundingBox != null && ft.WGS84BoundingBox.Length > 0)
                    {
                        featureType.LatLongBBox[0] = double.Parse(ft.WGS84BoundingBox[0].LowerCorner.Split(' ')[0], Nhi);
                        featureType.LatLongBBox[1] = double.Parse(ft.WGS84BoundingBox[0].LowerCorner.Split(' ')[1], Nhi);
                        featureType.LatLongBBox[2] = double.Parse(ft.WGS84BoundingBox[0].UpperCorner.Split(' ')[0], Nhi);
                        featureType.LatLongBBox[3] = double.Parse(ft.WGS84BoundingBox[0].UpperCorner.Split(' ')[1], Nhi);
                    }
                    _feaureTypes.Add(featureType);
                }
                #endregion
            }
        }

        #region Properties
        public string OnlineResource
        {
            get { return _onlineResource; }
        }

        public string GetFeatureTypeOnlineResourceHttpPost
        {
            get { return _postGFTOnlineResource; }
        }

        public string GetFeatureTypeOnlineResourceHttpGet
        {
            get { return _getGFTOnlineResource; }
        }

        public string GetDescribeFeatureTypeOnlineResourceHttpPost
        {
            get { return _postDFTOnlineResource; }
        }

        public string GetDescribeFeatureTypeOnlineResourceHttpGet
        {
            get { return _getDFTOnlineResource; }
        }

        public FeatureType[] FeatureTypeList
        {
            get { return _feaureTypes.ToArray(); }
        }
        #endregion

        #region Helper Classes
        public class FeatureType
        {
            public string Name, Title, SRS;
            public double[] LatLongBBox = new double[4];
            public string[] OutputFormats;

            public FeatureType(string name, string title, string srs)
            {
                Name = name;
                Title = title;
                SRS = srs;
            }
            public FeatureType(string name, string title, string srs, string[] outputFormats)
            {
                Name = name;
                Title = title;
                SRS = srs;
                OutputFormats = outputFormats;
            }
        }
        #endregion
    }
}
