using System;
using Color = System.Drawing.Color;
using gView.Framework.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using gView.Framework.Web;
using System.Text;

namespace gView.Framework.OGC.WMS
{
    public enum WMSRequestType
    {
        GetCapabilities,
        GetMap,
        GetFeatureInfo,
        DescriptTiles,
        GetTile,

        GenerateTiles
    }

    public enum WMSExceptionType
    {
        se_xml,
        se_blank,
        se_in_image
    }

    public enum WMSInfoFormat
    {
        gml,
        html,
        text,
        xml,
        xsl
    }

    public enum WMSImageFormat
    {
        png,
        gif,
        jpeg,
        bmp,
        kml
    }


    public class WMSParameterDescriptor
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        #region request parameters
        private int miWidth = 200;
        private int miHeight = 200;
        private int miSRS = 4326;
        private int miFeatureInfoX = 0;
        private int miFeatureInfoY = 0;
        private int miFeatureInfoMaxRows = 0;

        private WMSRequestType meRequest = WMSRequestType.GetCapabilities;
        private WMSImageFormat meFormat = WMSImageFormat.png;
        private WMSExceptionType meExceptions = WMSExceptionType.se_xml;
        private WMSInfoFormat meInfoFormat = WMSInfoFormat.html;
        private string meInfoFormatXsl = String.Empty;
        private IEnvelope moBox = null;
        private string[] msLayers = new string[0];
        private string[] msQueryLayers = new string[0];
        private string msVersion = "1.1.1";
        private bool mbTransparent = false;
        private Color moBgColor = Color.White;
        private string sldString = String.Empty;

        private string msLayer = String.Empty, msStyle = String.Empty;
        private double miScale = 0.0;
        private int miTileRow = 0, miTileCol = 0;

        private int _bboxsrs = 0, _zoomlevel = 0;
        private string _requestKey = String.Empty;

        public double dpi = 96.0;
        #endregion

        public WMSParameterDescriptor()
        {

        }

        #region Parse
        public bool ParseParameters(string[] parameters)
        {
            ParseParameters(new Parameters(parameters));
            return true;
        }
        private void ParseParameters(Parameters Request)
        {


            //check for exception format
            if (Request["EXCEPTIONS"] != null)
            {
                if (Request["EXCEPTIONS"] != "application/vnd.ogc.se_inimage" &&
                    Request["EXCEPTIONS"] != "application/vnd.ogc.se_blank" &&
                    Request["EXCEPTIONS"] != "application/vnd.ogc.se_xml")
                    WriteError("Invalid exception format " + Request["EXCEPTIONS"] + " must be 'application/vnd.ogc.se_inimage', 'application/vnd.ogc.se_blank' or 'application/vnd.ogc.se_xml'.");
                else
                {
                    switch (Request["EXCEPTIONS"])
                    {
                        case "application/vnd.ogc.se_inimage":
                            this.Exceptions = WMSExceptionType.se_in_image;
                            break;
                        case "application/vnd.ogc.se_blank":
                            this.Exceptions = WMSExceptionType.se_blank;
                            break;
                        /* SE_XML is already the default value
                    default: 
                         * this.Exceptions = WMSExceptionType.SE_XML;
                         * break;
                         */

                    }
                }
            }
            if (Request["REQUEST"] == null)
            {
                WriteError("mandatory REQUEST parameter is missing");
            }
            if (Request["REQUEST"].ToUpper().IndexOf("MAP") != -1)
            {
                this.Request = WMSRequestType.GetMap;
            }
            else if (Request["REQUEST"].ToUpper().IndexOf("CAPABILITIES") != -1)
            {
                this.Request = WMSRequestType.GetCapabilities;
            }
            else if (Request["REQUEST"].ToUpper().IndexOf("FEATUREINFO") != -1)
            {
                this.Request = WMSRequestType.GetFeatureInfo;
            }
            else if (Request["REQUEST"].ToUpper().IndexOf("DESCRIPETILES") != -1)
            {
                this.Request = WMSRequestType.DescriptTiles;
            }
            else if (Request["REQUEST"].ToUpper().IndexOf("GETTILE") != -1)
            {
                this.Request = WMSRequestType.GetTile;
            }
            else if (Request["REQUEST"].ToUpper().IndexOf("GENERATETILES") != -1)
            {
                this.Request = WMSRequestType.GenerateTiles;
            }
            else
                WriteError("REQUEST parameter is either missing, erroneous, or not supported. Supported values are: 'GetMap', 'map', 'GetCapabilities','capabilities'");
            if (Request["VERSION"] == null && Request["WMTVER"] == null)
            {
                if (this.Request != WMSRequestType.GetCapabilities)
                    WriteError("mandatory VERSION parameter is either missing or erronous. Must be: 'WMTVER=1.0.0' or 'VERSION=1.x.x'");
                else
                    this.Version = "1.1.1";//default version

            }
            else
            {
                if (Request["WMTVER"] != null && Request["VERSION"] == null)
                    this.Version = Request["WMTVER"];
                else if (Request["WMTVER"] == null && Request["VERSION"] != null)
                    this.Version = Request["VERSION"];
            }

            #region Nicht Standard (für GenerateTiles Request: WMS2Tiles für Bing Map Controll!!)
            if (this.Request == WMSRequestType.GenerateTiles)
            {
                if (Request["BBOXSRS"] != null)
                {
                    if (Request["BBOXSRS"].IndexOf("EPSG:") == -1)
                        WriteError("only EPSG based coordinate systems are supported!", "InvalidBBOXSRS");

                    string[] srsid = Request["BBOXSRS"].Split(':');
                    this.BBoxSRS = int.Parse(srsid[srsid.Length - 1]);
                }
                if (Request["ZOOMLEVEL"] != null)
                {
                    this.ZoomLevel = int.Parse(Request["ZOOMLEVEL"]);
                }
                if (Request["REQUESTKEY"] != null)
                {
                    _requestKey = Request["REQUESTKEY"];
                }
                if (Request["TILEROW"] == null)
                    WriteError("mandatory TILEROW parameter is missing.");
                else
                    this.TileRow = int.Parse(Request["TILEROW"]);

                if (Request["TILECOL"] == null)
                    WriteError("mandatory TILECOL parameter is missing.");
                else
                    this.TileCol = int.Parse(Request["TILECOL"]);
            }
            #endregion

            if (this.Request != WMSRequestType.GetCapabilities && this.Request != WMSRequestType.DescriptTiles)
            {
                if (Request["SRS"] == null && Request["CRS"] == null)
                {
                    WriteError("mandatory SRS, CRS parameter is missing.");
                }
                else
                {
                    if (Request["SRS"] != null)
                    {
                        if (Request["SRS"].IndexOf("EPSG:") == -1)
                            WriteError("only EPSG based coordinate systems are supported!", "InvalidSRS");

                        string[] srsid = Request["SRS"].Split(':');
                        this.SRS = int.Parse(srsid[srsid.Length - 1]);
                    }
                    else if (Request["CRS"] != null)
                    {
                        if (Request["CRS"].IndexOf("EPSG:") == -1)
                            WriteError("only EPSG based coordinate systems are supported!", "InvalidCRS");

                        string[] srsid = Request["CRS"].Split(':');
                        this.SRS = int.Parse(srsid[srsid.Length - 1]);
                    }
                }

                if (this.Request == WMSRequestType.GetMap || this.Request == WMSRequestType.GetTile)
                {
                    if (Request["FORMAT"] == null)
                    {
                        WriteError("mandatory FORMAT parameter is missing.");
                    }
                    else
                    {
                        switch (Request["FORMAT"].ToLower())
                        {
                            case "image/gif":
                                this.Format = WMSImageFormat.gif; break;
                            case "image/bmp":
                                this.Format = WMSImageFormat.bmp; break;
                            case "image/jpg":
                            case "image/jpeg":
                                this.Format = WMSImageFormat.jpeg; break;
                            case "image/png":
                                this.Format = WMSImageFormat.png; break;
                            case "gif":
                                this.Format = WMSImageFormat.gif; break;
                            case "png":
                                this.Format = WMSImageFormat.png; break;
                            case "jpg":
                                this.Format = WMSImageFormat.jpeg; break;
                            case "application/vnd.google-earth.kml+xml":
                                this.Format = WMSImageFormat.kml; break;
                            default:
                                WriteError("Format " + Request["FORMAT"] + " is not supported.", "InvalidFormat");
                                break;
                        }
                    }
                }

                if (this.Request == WMSRequestType.GetTile)
                {
                    if (Request["LAYER"] == null)
                        WriteError("mandatory LAYER parameter is missing.");
                    else
                        this.Layer = Request["LAYER"];

                    if (Request["STYLE"] == null)
                        WriteError("mandatory STYLE parameter is missing.");
                    else
                        this.Style = Request["STYLE"];

                    if (Request["SCALE"] == null)
                        WriteError("mandatory SCALE parameter is missing.");
                    else
                        this.Scale = double.Parse(Request["SCALE"].Replace(",", "."), _nhi);

                    if (Request["TILEROW"] == null)
                        WriteError("mandatory TILEROW parameter is missing.");
                    else
                        this.TileRow = int.Parse(Request["TILEROW"]);

                    if (Request["TILECOL"] == null)
                        WriteError("mandatory TILECOL parameter is missing.");
                    else
                        this.TileCol = int.Parse(Request["TILECOL"]);
                }
                else
                {
                    if (this.Request == WMSRequestType.GetMap || this.Request == WMSRequestType.GetFeatureInfo)
                    {
                        if (Request["HEIGHT"] == null)
                        {
                            WriteError("mandatory HEIGHT parameter is missing.");
                        }
                        else
                            this.Height = int.Parse(Request["HEIGHT"]);
                        if (Request["WIDTH"] == null)
                        {
                            WriteError("mandatory WIDTH parameter is missing.");
                        }
                        else
                            this.Width = int.Parse(Request["WIDTH"]);
                    }

                    if (Request["BBOX"] == null)
                    {
                        WriteError("mandatory BBOX parameter is missing.");
                    }
                    string[] bbox = Request["BBOX"].Split(",".ToCharArray());
                    if (bbox.Length != 4)
                        WriteError("Invalid BBOX parameter. Must consist of 4 elements of type double or integer");

                    double MinX = double.Parse(bbox[0], _nhi);
                    double MinY = double.Parse(bbox[1], _nhi);
                    double MaxX = double.Parse(bbox[2], _nhi);
                    double MaxY = double.Parse(bbox[3], _nhi);
                    Envelope box = new Envelope(MinX, MinY, MaxX, MaxY);
                    if (box.minx >= box.maxx ||
                        box.miny >= box.maxy)
                        WriteError("Invalid BBOX parameter. MinX must not be greater than MaxX, and MinY must not be greater than MaxY");
                    this.BBOX = box;

                    string LayerTag = "LAYERS";
                    if (this.Request == WMSRequestType.GetFeatureInfo) LayerTag = "QUERY_LAYERS";

                    if (Request[LayerTag] == null)
                    {
                        WriteError("mandatory LAYERS parameter is missing.");
                    }
                    else
                    {
                        String sLayers = Request[LayerTag];
                        if (LayerTag == "LAYERS")
                            this.Layers = sLayers.Split(",".ToCharArray());
                        else if (LayerTag == "QUERY_LAYERS")
                            this.QueryLayers = sLayers.Split(",".ToCharArray());
                    }
                    if (Request["TRANSPARENT"] != null && Request["TRANSPARENT"].ToUpper() == "TRUE")
                    {
                        this.Transparent = true;
                    }
                    if (Request["BGCOLOR"] != null)
                    {
                        this.BgColor = ColorTranslator.FromHtml(Request["BGCOLOR"]);
                    }
                    if (Request["STYLES"] != null)
                    {
                        String[] styles = Request["STYLES"].Split(",".ToCharArray());
                        for (int i = 0; i < styles.Length; i++)
                            if (styles[i] != null && styles[i] != String.Empty)
                                WriteError("The monoGIS does not support named styles!.", "StyleNotDefined");
                    }
                    if (Request["DPI"] != null)
                    {
                        this.dpi = double.Parse(Request["DPI"], _nhi);
                    }
                    if (this.Request == WMSRequestType.GetMap)
                    {
                        if (Request["SLD"] != null)
                        {
                            sldString = WebFunctions.DownloadXml(Request["SLD"]);
                            if (sldString == null) sldString = String.Empty;
                        }
                        else if (Request["SLD_BODY"] != null)
                        {
                            sldString = Request["SLD_BODY"];
                        }
                    }
                }
            }
            if (this.Request == WMSRequestType.GetFeatureInfo)
            {
                if (Request["QUERYLAYERS"] == null)
                {
                    WriteError("mandatory QUERYLAYERS parameter is missing.");
                }
                else
                {
                    String sQueryLayers = Request["QUERYLAYERS"];
                    this.QueryLayers = sQueryLayers.Split(",".ToCharArray());
                }
                if (Request["X"] == null)
                {
                    WriteError("mandatory X parameter is missing.");
                }
                else
                {
                    this.FeatureInfoX = Convert.ToInt32(Request["X"]);
                    if (this.FeatureInfoX > this.Width || this.FeatureInfoX < 0)
                        WriteError("invalid X parameter, must be greater than 0 and lower than Width parameter.");
                }
                if (Request["Y"] == null)
                {
                    WriteError("mandatory Y parameter is missing.");
                }
                else
                {
                    this.FeatureInfoY = Convert.ToInt32(Request["Y"]);
                    if (this.FeatureInfoY > this.Height || this.FeatureInfoY < 0)
                        WriteError("invalid Y parameter, must be greater than 0 and lower than HEIGHT parameter.");
                }
                if (Request["FEATURECOUNT"] != null)
                {
                    this.FeatureInfoMaxRows = Convert.ToInt32(Request["FEATURECOUNT"]);
                    if (this.FeatureInfoMaxRows <= 0)
                        WriteError("invalid FEATURECOUNT parameter, must be greater than 0.");
                }
                if (Request["INFOFORMAT"] != null)
                {
                    switch (Request["INFOFORMAT"].ToLower())
                    {
                        case "gml":
                        case "application/vnd.ogc.gml": this.InfoFormat = WMSInfoFormat.gml;
                            break;
                        case "text/html": this.InfoFormat = WMSInfoFormat.html;
                            break;
                        case "text/xml": this.InfoFormat = WMSInfoFormat.xml;
                            break;
                        case "text/plain": this.InfoFormat = WMSInfoFormat.text;
                            break;
                        default:
                            if (Request["INFOFORMAT"].ToLower().StartsWith("xsl/"))
                            {
                                this.InfoFormat = WMSInfoFormat.xsl;
                                this.InfoFormatXsl = Request["INFOFORMAT"].ToLower();
                            }
                            else
                            {
                                WriteError("invalid INFORMAT parameter, may be: text/html, GML or application/vnd.ogc.gml.");
                            }
                            break;
                    }
                }
                else if (Request["INFO_FORMAT"] != null)
                {
                    switch (Request["INFO_FORMAT"].ToLower())
                    {
                        case "gml":
                        case "application/vnd.ogc.gml": this.InfoFormat = WMSInfoFormat.gml;
                            break;
                        case "text/html": this.InfoFormat = WMSInfoFormat.html;
                            break;
                        case "text/xml": this.InfoFormat = WMSInfoFormat.xml;
                            break;
                        case "text/plain": this.InfoFormat = WMSInfoFormat.text;
                            break;
                        default:
                            if (Request["INFO_FORMAT"].ToLower().StartsWith("xsl/"))
                            {
                                this.InfoFormat = WMSInfoFormat.xsl;
                                this.InfoFormatXsl = Request["INFO_FORMAT"].ToLower();
                            }
                            else
                            {
                                WriteError("invalid INFORMAT parameter, may be: text/html, GML or application/vnd.ogc.gml.");
                            }
                            break;
                    }
                }
            }
        }
        #endregion

        #region ErrorReport
        private void WriteError(String msg)
        {
            WriteError(msg, null);
        }

        private void WriteError(String msg, String code)
        {
            if (this.Format == WMSImageFormat.kml)
                WriteKMLError(msg, code);
            if (this.Exceptions == WMSExceptionType.se_xml)
            {
                String sMsg = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
<!DOCTYPE ServiceExceptionReport SYSTEM
 ""http://www.digitalearth.gov/wmt/xml/exception_1_1_0.dtd"">
<ServiceExceptionReport version=""1.1.0"">
  <ServiceException>
    message.
  </ServiceException>     
</ServiceExceptionReport>";
                sMsg = sMsg.Replace("message.", msg);
                if (code != null)
                {
                    sMsg = sMsg.Replace("<ServiceException>", "<ServiceException code=\"" + code + "\">");
                }
                //Response.ContentType = "application/vnd.ogc.se_xml";
                //Response.Write(sMsg);
                //Response.End();
            }
            else if (this.Exceptions == WMSExceptionType.se_in_image)
            {
                Bitmap bt = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bt);
                if (!this.Transparent)
                    g.Clear(this.BgColor);
                else
                {
                    bt.MakeTransparent(this.BgColor);
                    g.Clear(this.BgColor);
                }
                Font f = new Font("Tahoma", 12, FontStyle.Regular);
                SizeF sz = g.MeasureString(msg, f);
                RectangleF rect = new RectangleF(5f, 5f, sz.Width, sz.Height);
                g.DrawString(msg, f, new SolidBrush(Color.Black), rect);
                g.Save();
                if (this.Format == WMSImageFormat.gif)
                    bt = this.CreateTransparentGif(bt, bt.Palette);
                MemoryStream oStr = new MemoryStream();
                bt.Save(oStr, GetImageFormat());
                //Response.ContentType = this.MimeType;
                //byte[] img = oStr.ToArray();
                //Response.OutputStream.Write(img, 0, img.Length);
                //Response.End();
            }
            else
            {
                Bitmap bt = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bt);
                if (!this.Transparent)
                    g.Clear(this.BgColor);
                else
                {
                    bt.MakeTransparent(this.BgColor);
                    g.Clear(this.BgColor);
                }
                g.Save();
                if (this.Format == WMSImageFormat.gif)
                    bt = this.CreateTransparentGif(bt, bt.Palette);
                MemoryStream oStr = new MemoryStream();
                bt.Save(oStr, GetImageFormat());
                //Response.ContentType = this.MimeType;
                //byte[] img = oStr.ToArray();
                //Response.OutputStream.Write(img, 0, img.Length);
                //Response.End();
            }

        }

        protected void WriteKMLError(string msg, string code)
        {
            //Response.ClearContent();
            //Response.ClearHeaders();
            //Response.Clear();
            //Response.Buffer = true;

            //Response.ContentType = "application/vnd.google-earth.kml+xml";
            //Response.ContentEncoding = System.Text.Encoding.UTF8;

            String sRet = String.Empty;

            sRet = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            sRet += "<kml xmlns=\"http://earth.google.com/kml/2.0\">";
            sRet += "<Folder>";
            sRet += "<name>ERROR</name>";
            sRet += "<description>" + msg + "</description>";
            sRet += "</Folder>";
            sRet += "</kml>";

            //Response.Write(sRet);
            //Response.Flush();
            //Response.End();
        }
        #endregion

        #region Helper
        private Bitmap CreateTransparentGif(Bitmap gif, ColorPalette cp)
        {
            //please note: the following code has been adopted from the following page:
            //GDI+ FAQ, http://www.bobpowell.net/giftransparency.htm
            //Creates a new GIF image with a modified colour palette
            if (cp != null)
            {
                //Create a new 8 bit per pixel image
                Bitmap bm = new Bitmap(gif.Width, gif.Height, PixelFormat.Format8bppIndexed);
                //get it's palette
                ColorPalette ncp = bm.Palette;
                //copy all the entries from the old palette removing any transparency
                int n = 0;
                foreach (Color c in cp.Entries)
                    ncp.Entries[n++] = Color.FromArgb(255, c);
                //Set the second entry as transparent color
                ncp.Entries[ncp.Entries.Length - 1] = Color.FromArgb(0, ncp.Entries[ncp.Entries.Length - 1]);

                //re-insert the palette
                bm.Palette = ncp;
                //now to copy the actual bitmap data
                //lock the source and destination bits
                BitmapData src = ((Bitmap)gif).LockBits(new Rectangle(0, 0, gif.Width, gif.Height), ImageLockMode.ReadOnly, gif.PixelFormat);
                BitmapData dst = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.WriteOnly, bm.PixelFormat);

                //uses pointers so we need unsafe code.
                //the project is also compiled with /unsafe
                unsafe
                {
                    //steps through each pixel
                    for (int y = 0; y < gif.Height; y++)
                        for (int x = 0; x < gif.Width; x++)
                        {
                            //transferring the bytes
                            ((byte*)dst.Scan0.ToPointer())[(dst.Stride * y) + x] = ((byte*)src.Scan0.ToPointer())[(src.Stride * y) + x];
                        }
                }

                //all done, unlock the bitmaps
                ((Bitmap)gif).UnlockBits(src);
                bm.UnlockBits(dst);
                return bm;
            }
            else
                return null;
        }

        public ImageFormat GetImageFormat()
        {
            switch (this.Format)
            {
                case WMSImageFormat.gif:
                    return ImageFormat.Gif;
                case WMSImageFormat.bmp:
                    return ImageFormat.Bmp;
                case WMSImageFormat.jpeg:
                    return ImageFormat.Jpeg;
                case WMSImageFormat.png:
                    return ImageFormat.Png;
                default: return ImageFormat.Png;
            }
        }
        #endregion

        public int FeatureInfoX
        {
            get { return miFeatureInfoX; }
            set { miFeatureInfoX = value; }
        }

        public int FeatureInfoY
        {
            get { return miFeatureInfoY; }
            set { miFeatureInfoY = value; }
        }

        public int FeatureInfoMaxRows
        {
            get { return miFeatureInfoMaxRows; }
            set { miFeatureInfoMaxRows = value; }
        }

        public int Width
        {
            get { return miWidth; }
            set { miWidth = value; }
        }

        public int Height
        {
            get { return miHeight; }
            set { miHeight = value; }
        }

        public int SRS
        {
            get { return miSRS; }
            set { miSRS = value; }
        }

        public int BBoxSRS
        {
            get { return _bboxsrs; }
            set { _bboxsrs = value; }
        }

        public int ZoomLevel
        {
            get { return _zoomlevel; }
            set { _zoomlevel = value; }
        }

        public string RequestKey
        {
            get { return _requestKey; }
        }

        public WMSRequestType Request
        {
            get { return meRequest; }
            set { meRequest = value; }
        }

        public WMSExceptionType Exceptions
        {
            get { return meExceptions; }
            set { meExceptions = value; }
        }

        public WMSInfoFormat InfoFormat
        {
            get { return meInfoFormat; }
            set { meInfoFormat = value; }
        }

        public string InfoFormatXsl
        {
            get { return meInfoFormatXsl; }
            set { meInfoFormatXsl = value; }
        }

        public string Version
        {
            get { return msVersion; }
            set { msVersion = value; }
        }

        public IEnvelope BBOX
        {
            get { return moBox; }
            set { moBox = value; }
        }

        public WMSImageFormat Format
        {
            get { return meFormat; }
            set { meFormat = value; }
        }

        public string[] Layers
        {
            get { return msLayers; }
            set { msLayers = value; }
        }

        public string[] QueryLayers
        {
            get { return msQueryLayers; }
            set { msQueryLayers = value; }
        }

        public Color BgColor
        {
            get { return moBgColor; }
            set { moBgColor = value; }
        }

        public bool Transparent
        {
            get { return mbTransparent; }
            set { mbTransparent = value; }
        }

        public string MimeType
        {
            get
            {
                switch (Format)
                {
                    case WMSImageFormat.bmp:
                        return "image/bmp";
                    case WMSImageFormat.gif:
                        return "image/gif";
                    case WMSImageFormat.jpeg:
                        return "image/jpeg";
                    case WMSImageFormat.png:
                        return "image/png";
                    case WMSImageFormat.kml:
                        return "application/vnd.google-earth.kml+xml";

                    default:
                        return "image/png";
                }
            }
        }

        public string Layer
        {
            get { return msLayer; }
            set { msLayer = value; }
        }
        public string Style
        {
            get { return msStyle; }
            set { msStyle = value; }
        }
        public double Scale
        {
            get { return miScale; }
            set { miScale = value; }
        }
        public int TileRow
        {
            get { return miTileRow; }
            set { miTileRow = value; }
        }
        public int TileCol
        {
            get { return miTileCol; }
            set { miTileCol = value; }
        }

        private class Parameters
        {
            private Dictionary<string, string> _parameters = new Dictionary<string, string>();

            public Parameters(string[] list)
            {
                if (list == null) return;

                foreach (string l in list)
                {
                    string[] p = l.Split('=');

                    string p1 = p[0].Trim().ToUpper(), pp;
                    StringBuilder p2 = new StringBuilder();
                    for (int i = 1; i < p.Length; i++)
                    {
                        if (p2.Length > 0) p2.Append("=");
                        p2.Append(p[i]);
                    }
                    if (_parameters.TryGetValue(p1, out pp)) continue;

                    _parameters.Add(p1, p2.ToString());
                }
            }

            public string this[string parameter]
            {
                get
                {
                    string o;
                    if (!_parameters.TryGetValue(parameter.ToUpper(), out o))
                        return null;

                    return o;
                }
            }
        }

        public string SLD_BODY
        {
            get { return sldString; }
        }
    }
}
