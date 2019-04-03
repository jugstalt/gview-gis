using System;
using gView.Framework;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using gView.Framework.UI.Events;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.UI;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI.Dialogs;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.Globalisation;
using System.Text;
using gView.Plugins.MapTools.Dialogs;
using gView.Plugins.MapTools.Controls;
using gView.system.UI.Framework.system.UI;

namespace gView.Plugins.MapTools
{
    /// <summary>
    /// Zusammenfassung für BaseTools.
    /// </summary>
    /// 

    [RegisterPlugIn("D1A87DBA-00DB-4704-B67B-4846E6F03959")]
    public class NewDocument : gView.Framework.UI.ITool
    {
        IMapDocument _doc;

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.New", "New"); }
        }

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

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return (new Buttons()).imageList1.Images[11]; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null) return;

            foreach (IMap map in _doc.Maps)
            {
                _doc.RemoveMap(map);
            }
            IMap newmap = new Map();
            _doc.AddMap(newmap);
            _doc.FocusMap = newmap;

            if (_doc.Application is IMapApplication)
                ((IMapApplication)_doc.Application).DocumentFilename = String.Empty;
        }

        #endregion
    }

    [RegisterPlugIn("CEACE261-ECE4-4622-A892-58A5B32E5295")]
    public class LoadDocument : gView.Framework.UI.ITool, IShortCut
    {
        private MapDocument _doc = null;

        #region ITool Member

        public object Image
        {
            get
            {
                Buttons dlg = new Buttons();
                return dlg.imageList1.Images[0];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is MapDocument)
                _doc = (MapDocument)hook;
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Load", "Load...");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null) return;

            System.Windows.Forms.OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Map Files (*.mxl)|*.mxl|Reader Files (*.rdm)|*.rdm|ArcXml Files (*.axl)|*.axl";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (_doc.Application is IMapApplication)
                    ((IMapApplication)_doc.Application).LoadMapDocument(dlg.FileName);
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command; ;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.O;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+O";
            }
        }

        #endregion

    }

    [RegisterPlugIn("FCA2C303-A0B6-4f36-BD21-E1C119EB9C8E")]
    public class SaveDocument : gView.Framework.UI.ITool, IShortCut
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public object Image
        {
            get
            {
                Buttons dlg = new Buttons();
                return dlg.imageList1.Images[1];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Save", "Save");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null) return;
            if (!(_doc.Application is IMapApplication)) return;

            try
            {
                FileInfo fi = new FileInfo(((IMapApplication)_doc.Application).DocumentFilename);
                if (fi.Name.ToLower() == "newdocument.mxl" || fi.Extension.ToLower() == ".axl")
                {
                    System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "Map Files (*.mxl)|*.mxl|Reader Files (*.rdm)|*.rdm";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        ((IMapApplication)_doc.Application).SaveMapDocument(dlg.FileName, true);
                    }
                }
                else
                {
                    ((IMapApplication)_doc.Application).SaveMapDocument();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.S;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+S";
            }
        }

        #endregion

    }

    [RegisterPlugIn("17D0A3C1-5EE9-4ddd-9402-E6E9EAB1CD06")]
    public class SaveDocumentAs : gView.Framework.UI.ITool
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public object Image
        {
            get
            {
                return null;
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.SaveAs", "Save As...");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null) return;
            if (!(_doc.Application is IMapApplication)) return;

            System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Map Files (*.mxl)|*.mxl|Reader Files (*.rdm)|*.rdm";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                gView.Plugins.Tools.MapTools.Dialogs.FormSaveEncrypted saveEncDlg = new Tools.MapTools.Dialogs.FormSaveEncrypted();
                if (saveEncDlg.ShowDialog() == DialogResult.OK)
                {
                    ((IMapApplication)_doc.Application).SaveMapDocument(dlg.FileName, saveEncDlg.SaveEncrypted);
                }
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion

    }

    [RegisterPlugIn("97F8675C-E01F-451e-AAE0-DC29CD547EB5")]
    public class ExitApplication : ITool, IExTool
    {
        IMapDocument _doc = null;
        IExplorerApplication _exapp = null;

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Exit", "Exit"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get
            {
                Buttons dlg = new Buttons();
                return dlg.imageList1.Images[14];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
            else if (hook is IExplorerApplication)
                _exapp = (IExplorerApplication)hook;
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc != null && _doc.Application != null)
            {
                _doc.Application.Exit();
            }
            if (_exapp != null)
            {
                _exapp.Exit();
            }
        }

        #endregion
    }

    [RegisterPlugIn("CC299CF6-2C88-45aa-BE75-9EE3D5DCC0A8")]
    public class PublishMap : ITool, IMapContextMenuItem
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.PublishMap", "Publish Map..."); }
        }

        public bool Enabled
        {
            get
            {
                return _doc != null && _doc.FocusMap != null;
            }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.publish; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public void OnEvent(object mapEvent)
        {
            if (_doc == null || _doc.FocusMap == null)
                return;

            IMap map = _doc.FocusMap;
            if (mapEvent is MapEvent && ((MapEvent)mapEvent).Map != null)
                map = ((MapEvent)mapEvent).Map;

            FormPublishMap dlg = new FormPublishMap();
            dlg.ServiceName = map.Name;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (_doc.Application is IGUIApplication)
                        ((IGUIApplication)_doc.Application).SetCursor(Cursors.WaitCursor);

                    XmlStream stream;

                    if (dlg.Version == FormPublishMap.ServerVersion.gViewServer5)
                    {
                        var mapDocument = new MapDocument(_doc.Application as IMapApplication);
                        mapDocument.AddMap(map);

                        stream = new XmlStream("root");
                        stream.Save("MapDocument", mapDocument);

                        stream.ReduceDocument("//MapDocument");
                    }
                    else
                    {
                        stream = new XmlStream("MapDocument");
                        stream.Save("IMap", map);

                        stream.ReduceDocument("//IMap");
                    }

                    StringBuilder sb = new StringBuilder();
                    StringWriter sw = new StringWriter(sb);
                    stream.WriteStream(sw);

                    //gView.MapServer.Connector.MapServerInstanceTypeService proxy = new gView.MapServer.Connector.MapServerInstanceTypeService(
                    //    dlg.Server + ":" + dlg.Port.ToString());

                    string serverUrl = MapServer.Connector.MapServerConnection.ServerUrl(dlg.Server,dlg.Port);
                    gView.MapServer.Connector.MapServerConnection service = new MapServer.Connector.MapServerConnection(serverUrl);
                    if (!service.AddMap(dlg.ServiceName, sb.ToString(), dlg.Username, dlg.Password))
                    {
                        throw new Exception("Unable to add service..." + Environment.NewLine + service.lastErrorMsg);
                    }

                    if (_doc.Application is IGUIApplication)
                        ((IGUIApplication)_doc.Application).SetCursor(Cursors.Default);

                    MessageBox.Show("Service successfully add to server instance!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    if (_doc.Application is IGUIApplication)
                        ((IGUIApplication)_doc.Application).SetCursor(Cursors.Default);

                    FormToolException.Show("Publish Map", ex.Message);
                }


            }
        }

        #endregion

        #region IContextMenuTool Member

        public bool Enable(object element)
        {
            return element is IMap && ((IMap)element).MapElements != null &&
                ((IMap)element).MapElements.Count > 0;
        }

        public bool Visible(object element)
        {
            return true;
        }

        public void OnEvent(object element, object parent)
        {
            if (!(element is IMap))
                return;

            MapEvent mapEvent = new MapEvent(element as IMap);
            this.OnEvent(mapEvent);
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 81; }
        }

        #endregion
    }

    [RegisterPlugIn("6351BCBE-809A-43cb-81AA-6414ED3FA459")]
    public class ZoomInStatic : gView.Framework.UI.ITool
    {
        #region ITool Members

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ZoomInStatic", "Zoom In Static");
            }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get
            {
                Buttons dlg = new Buttons();
                return dlg.imageList1.Images[16];
            }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent)) return;

            MapEvent ev = (MapEvent)MapEvent;
            if (ev.Map == null) return;

            ev.Map.Display.mapScale /= 2.0;

            ev.refreshMap = true;
        }

        #endregion
    }

    [RegisterPlugIn("E1C01E9D-8ADC-477b-BCD1-6B7BBA756D44")]
    public class ZoomOutStatic : gView.Framework.UI.ITool
    {
        #region ITool Members

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ZoomOutStatic", "Zoom Out Static");
            }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get
            {
                Buttons dlg = new Buttons();
                return dlg.imageList1.Images[15];
            }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent)) return;

            MapEvent ev = (MapEvent)MapEvent;
            if (ev.Map == null) return;

            ev.Map.Display.mapScale *= 2.0;

            ev.refreshMap = true;
        }

        #endregion
    }

    [RegisterPlugIn("09007AFA-B255-4864-AC4F-965DF330BFC4")]
    public class ZoomIn : gView.Framework.UI.ITool
    {
        public ZoomIn()
        {

        }
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ZoomIn", "Zoom In");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return gView.Framework.UI.ToolType.rubberband;
            }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEventRubberband)) return;

            MapEventRubberband ev = (MapEventRubberband)MapEvent;
            if (ev.Map == null) return;

            if (!(ev.Map.Display is Display)) return;
            Display nav = (Display)ev.Map.Display;

            if (Math.Abs(ev.maxX - ev.minX) < 1e-5 ||
                Math.Abs(ev.maxY - ev.minY) < 1e-5)
            {
                nav.setScale(nav.mapScale / 2.0, ev.maxX, ev.maxY);
            }
            else
            {
                nav.ZoomTo(ev.minX, ev.minY, ev.maxX, ev.maxY);
            }
            ev.refreshMap = true;
        }
        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[4];
            }
        }
        #endregion
    }

    [RegisterPlugIn("58AE3C1D-40CD-4f61-8C5C-0A955C010CF4")]
    public class Zoom2FullExtent : ITool
    {
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ZoomToFullExtent", "Zoom To Full Extent");
            }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.map16; }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent)) return;

            MapEvent ev = (MapEvent)MapEvent;
            if (ev.Map == null || ev.Map.Display == null) return;

            ev.Map.Display.ZoomTo(ev.Map.Display.Limit);

            ev.refreshMap = true;
        }

        #endregion
    }
    [RegisterPlugIn("3E2E9F8C-24FB-48f6-B80E-1B0A54E8C309")]
    public class SmartNavigation : gView.Framework.UI.ITool, IScreenTip
    {
        public SmartNavigation()
        {

        }
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.SmartNavigation", "Zoom/Pan");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return gView.Framework.UI.ToolType.smartnavigation;
            }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            if (MapEvent is MapEventRubberband)
            {

                MapEventRubberband ev = (MapEventRubberband)MapEvent;
                if (ev.Map == null) return;

                if (!(ev.Map.Display is Display)) return;
                Display nav = (Display)ev.Map.Display;

                nav.ZoomTo(new Envelope(ev.minX, ev.minY, ev.maxX, ev.maxY));
                ev.refreshMap = true;
            }
            else if (MapEvent is MapEventPan)
            {
                MapEventPan ev = (MapEventPan)MapEvent;
                if (ev.Map == null) return;

                if (!(ev.Map.Display is Display)) return;
                Display nav = (Display)ev.Map.Display;

                nav.Pan(ev.dX, ev.dY);
                ev.refreshMap = true;
            }
        }

        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[18];
            }
        }
        #endregion

        #region IScreenTip Member

        public string ScreenTip
        {
            get { return "Use left mousebutton for panning.\nUse right mousebutton an move up an down for zooming.\nThis tool is always used, if you press and hold Crtl-Key..."; }
        }

        #endregion
    }

    [RegisterPlugIn("51D04E6F-A13E-40b6-BF28-1B8E7C24493D")]
    public class ZoomOut : gView.Framework.UI.ITool
    {
        public ZoomOut()
        {
        }
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ZoomOut", "Zoom Out");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return gView.Framework.UI.ToolType.rubberband;
            }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEventRubberband)) return;

            MapEventRubberband ev = (MapEventRubberband)MapEvent;
            if (ev.Map == null) return;

            if (!(ev.Map.Display is Display)) return;
            Display nav = (Display)ev.Map.Display;

            if (Math.Abs(ev.maxX - ev.minX) < 1e-5 ||
                Math.Abs(ev.maxY - ev.minY) < 1e-5)
            {
                nav.setScale(nav.mapScale * 2.0, ev.maxX, ev.maxY);
            }
            else
            {
                // Extent vergrößern
                Envelope env = new Envelope(ev.minX, ev.minY, ev.maxX, ev.maxY);
                Envelope mEnv = new Envelope(ev.Map.Display.Envelope);
                double area = env.Width * env.Height;
                double mArea = mEnv.Width * mEnv.Height;

                mEnv.Raise((mArea / area) * 100.0);
                nav.ZoomTo(mEnv.minx, mEnv.miny, mEnv.maxx, mEnv.maxy);
            }
            ev.refreshMap = true;
        }
        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[5];
            }
        }
        #endregion
    }

    [RegisterPlugIn("9AADD17B-CDD0-4111-BBC5-E31E060CE210")]
    public class QueryThemeText : gView.Framework.UI.ITool, gView.Framework.UI.IToolItem
    {
        #region ITool Member

        public string Name
        {
            get { return "ScaleText"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {

        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get
            {
                return new ToolStripLabel(
                    LocalizedResources.GetResString("Text.Scale", "Scale:")
                    );
            }
        }

        #endregion
    }

    [RegisterPlugIn("03058244-16EE-44dd-B185-5522281498F5")]
    public class ScaleCombo : gView.Framework.UI.ITool, gView.Framework.UI.IToolItem
    {
        private IMapDocument _doc = null;
        private ToolStripComboBox _dropDown;
        public ScaleCombo()
        {
            _dropDown = new ToolStripComboBox();
            _dropDown.Items.Add("1:1000");
            _dropDown.Items.Add("1:5000");
            _dropDown.Items.Add("1:10000");
            _dropDown.Items.Add("1:50000");
            _dropDown.Items.Add("1:100000");
            _dropDown.Items.Add("1:500000");
            _dropDown.Items.Add("1:1000000");
            _dropDown.Visible = true;
            _dropDown.Width = 100;

            _dropDown.KeyDown += new KeyEventHandler(dropDown_KeyDown);
            _dropDown.SelectedIndexChanged += new EventHandler(dropDown_SelectedIndexChanged);
        }

        private void dropDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                int scale = GetScaleFromText(_dropDown.Text);
                if (scale <= 0) return;

                int index = 0;
                foreach (string txt in _dropDown.Items)
                {
                    int s = GetScaleFromText(txt);
                    if (s > scale)
                    {
                        _dropDown.Items.Insert(index, "1:" + scale);
                        break;
                    }
                    index++;
                }
                SetMapScale(scale);
            }
        }

        private void dropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMapScale(GetScaleFromText(_dropDown.Text));
        }

        private delegate void MapScaleChangedCallback(IDisplay display);
        private void doc_MapScaleChanged(IDisplay display)
        {
            if (_dropDown == null || _dropDown.Owner == null) return;

            if (_dropDown.Owner.InvokeRequired)
            {
                MapScaleChangedCallback d = new MapScaleChangedCallback(doc_MapScaleChanged);
                _dropDown.Owner.Invoke(d, new object[] { display });
            }
            else
            {
                if (display == null)
                {
                    _dropDown.Text = "";
                    _dropDown.Enabled = false;
                    return;
                }
                else if (display.MapUnits == GeoUnits.Unknown)
                {
                    _dropDown.Text = "";
                    _dropDown.Enabled = false;
                    return;
                }
                try
                {
                    _dropDown.Enabled = true;
                    _dropDown.Text = "1:" + (int)(display.mapScale + 0.5);
                }
                catch { }
            }
        }

        private void SetMapScale(int scale)
        {
            if (_doc == null) return;
            if (_doc.FocusMap == null || _doc.Application == null) return;
            if (scale <= 0 || scale == (int)((IDisplay)_doc.FocusMap).mapScale) return;

            _doc.FocusMap.Display.mapScale = scale;
            if (_doc.Application is IMapApplication)
                ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
        }
        private int GetScaleFromText(string txt)
        {
            if (_doc == null) return 0;
            if (_doc.FocusMap == null) return 0;

            int pos = txt.IndexOf(":");
            if (pos != 0)
            {
                txt = txt.Substring(pos + 1, txt.Length - pos - 1);
            }
            try
            {
                double scale = Convert.ToDouble(txt);
                return (int)Math.Abs(scale);
            }
            catch
            {
                return 0;
                //return (int)((IDisplay)_doc.FocusMap).mapScale;
            }
        }
        #region ITool Members

        public string Name
        {
            get { return "ScaleCombo"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _doc.MapScaleChanged += new MapScaleChangedEvent(doc_MapScaleChanged);
            }
        }

        public void OnEvent(object MapEvent)
        {

        }

        #endregion

        #region IToolItem Members


        public ToolStripItem ToolItem
        {
            get
            {
                return (ToolStripItem)_dropDown;
            }
        }

        #endregion
    }

    [RegisterPlugIn("2680F0FD-31EE-48c1-B0F7-6674BAD0A688")]
    public class Pan : gView.Framework.UI.ITool
    {
        public Pan()
        {
        }
        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Pan", "Pan");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return gView.Framework.UI.ToolType.pan;
            }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEventPan)) return;

            MapEventPan ev = (MapEventPan)MapEvent;
            if (ev.Map == null) return;

            if (!(ev.Map.Display is Display)) return;
            Display nav = (Display)ev.Map.Display;

            nav.Pan(ev.dX, ev.dY);
            ev.refreshMap = true;
        }
        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[6];
            }
        }
        #endregion
    }

    internal class ZoomStack
    {
        static System.Collections.Stack stack = new Stack(20);

        static public void Push(IEnvelope envelope)
        {
            if (envelope == null) return;
            if (stack.Count > 0)
            {
                IEnvelope last = (IEnvelope)stack.Peek();
                //MessageBox.Show(last.minx.ToString() + "==" + envelope.minx.ToString() + "\n" + last.miny.ToString() + "==" + envelope.miny.ToString() + "\n" + last.maxx.ToString() + "==" + envelope.maxx.ToString() + "\n" + last.maxy.ToString() + "==" + envelope.maxy.ToString());
                if (Math.Abs(last.minx - envelope.minx) < 1e-10 &&
                    Math.Abs(last.miny - envelope.miny) < 1e-10 &&
                    Math.Abs(last.maxx - envelope.maxx) < 1e-10 &&
                    Math.Abs(last.maxy - envelope.maxy) < 1e-10)
                {
                    return;
                }
            }
            stack.Push(envelope);
            //MessageBox.Show(stack.Count.ToString());
        }
        static public IEnvelope Pop()
        {
            if (stack.Count == 0) return null;
            return (IEnvelope)stack.Pop();
        }

        static public int Count
        {
            get { return stack.Count; }
        }
    }
    internal class ZoomForwardStack
    {
        static System.Collections.Stack stack = new Stack(20);

        static public void Push(IEnvelope envelope)
        {
            if (envelope == null) return;
            stack.Push(envelope);
        }
        static public IEnvelope Pop()
        {
            if (stack.Count == 0) return null;
            return (IEnvelope)stack.Pop();
        }

        static public int Count
        {
            get { return stack.Count; }
        }
    }

    [RegisterPlugIn("82F8E9C3-7B75-4633-AB7C-8F9637C2073D")]
    public class ZoomBack : gView.Framework.UI.ITool
    {
        private IMapDocument _doc;

        private void ZoomBack_MapScaleChanged(IDisplay sender)
        {
            if (sender == null) return;

            ZoomStack.Push(sender.Envelope);
        }

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.ZoomBack", "Back"); }
        }

        public bool Enabled
        {
            get
            {
                return (ZoomStack.Count > 0);
            }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return (new Buttons()).imageList1.Images[12]; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _doc.MapScaleChanged += new MapScaleChangedEvent(ZoomBack_MapScaleChanged);

            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null) return;
            ZoomForwardStack.Push(ZoomStack.Pop());
            IEnvelope env = ZoomStack.Pop();
            if (env == null) return;

            _doc.MapScaleChanged -= new MapScaleChangedEvent(ZoomBack_MapScaleChanged);
            ((MapEvent)MapEvent).Map.Display.ZoomTo(new Envelope(env.minx, env.miny, env.maxx, env.maxy));
            _doc.MapScaleChanged += new MapScaleChangedEvent(ZoomBack_MapScaleChanged);

            ((MapEvent)MapEvent).refreshMap = true;
            ((MapEvent)MapEvent).drawPhase = DrawPhase.All;
        }

        #endregion
    }

    [RegisterPlugIn("CFE66CDF-CD95-463c-8CD1-2541574D719A")]
    public class ZoomForward : gView.Framework.UI.ITool
    {

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.ZoomForward", "Forward"); }
        }

        public bool Enabled
        {
            get
            {
                return (ZoomForwardStack.Count > 0);
            }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return (new Buttons()).imageList1.Images[13]; }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            MapEvent ev = (MapEvent)MapEvent;
            if (ev.Map == null) return;

            IEnvelope env = ZoomForwardStack.Pop();
            if (env == null) return;
            ev.Map.Display.ZoomTo(new Envelope(env.minx, env.miny, env.maxx, env.maxy));
            ev.refreshMap = true;
            ev.drawPhase = DrawPhase.All;
        }

        #endregion
    }

    [RegisterPlugIn("646860CF-4F82-424b-BF7D-822BE7A214FF")]
    public class Select : gView.Framework.UI.ITool, gView.Framework.UI.IToolWindow
    {
        private gView.Plugins.MapTools.Controls.SelectionEnvironmentControl _dlg;
        private IMapApplication _app = null;
        private SelectionGraphicsElement _element;
        private GraphicsContainer _container;

        public Select()
        {
            _element = new SelectionGraphicsElement();
            _container = new GraphicsContainer();
            _container.Elements.Add(_element);
            _relation = spatialRelation.SpatialRelationIntersects;
        }

        #region ITool Member

        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[7];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument && ((IMapDocument)hook).Application is IMapApplication)
            {
                _dlg = new gView.Plugins.MapTools.Controls.SelectionEnvironmentControl(this);
                _dlg.MapDocument = (IMapDocument)hook;
                _app = ((IMapDocument)hook).Application as IMapApplication;
            }
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Select", "Select");
            }
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public void OnEvent(object MapEvent)
        {
            MapEvent ev = MapEvent as MapEvent;
            if (ev == null || ev.Map == null || ev.Map.Display == null) return;
            if (ev.Map.SelectionEnvironment == null) return;


            if (ev is MapEventClick && _methode != gView.Plugins.MapTools.Controls.selectionMothode.Rectangle)
            {
                MapEventClick evClick = (MapEventClick)ev;

                _element.AddPoint(new Point(evClick.x, evClick.y));
                ev.Map.Display.DrawOverlay(_container, true);
                return;
                //if ((filter.Geometry = _element.Geometry) == null) return;
                //filter.SpatialReference = ev.map.Display.SpatialReference;
            }
            else
            {
                if (!(ev is MapEventRubberband)) return;
                MapEventRubberband evRubberband = (MapEventRubberband)ev;

                IEnvelope envelope = new gView.Framework.Geometry.Envelope(
                    evRubberband.minX, evRubberband.minY,
                    evRubberband.maxX, evRubberband.maxY);

                if (SelectByGeometry(ev.Map, envelope))
                {
                    ev.refreshMap = true;
                    ev.drawPhase = DrawPhase.Selection | DrawPhase.Graphics;
                }
            }
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return
                    (_methode ==gView.Plugins.MapTools.Controls.selectionMothode.Rectangle) ? gView.Framework.UI.ToolType.rubberband : gView.Framework.UI.ToolType.click;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion

        #region IToolWindow Members

        public IDockableWindow ToolWindow
        {
            get { return _dlg; }
        }

        #endregion

        private gView.Plugins.MapTools.Controls.selectionMothode _methode;
        internal gView.Plugins.MapTools.Controls.selectionMothode SelectionMethode
        {
            get { return _methode; }
            set
            {
                _methode = _element.Methode = value;
                _element.ClearPoints();
            }
        }

        private spatialRelation _relation;
        internal spatialRelation SpatialRelation
        {
            get { return _relation; }
            set { _relation = value; }
        }

        private CombinationMethod _combinationMethode = CombinationMethod.New;
        internal CombinationMethod CombinationMethode
        {
            get { return _combinationMethode; }
            set { _combinationMethode = value; }
        }

        internal void ClearSelectionFigure()
        {
            try
            {
                _dlg.MapDocument.FocusMap.Display.ClearOverlay();
                _element.ClearPoints();
            }
            catch { }
        }
        internal IGeometry SelectionGeometry
        {
            get
            {
                return _element.Geometry;
            }
        }
        internal bool SelectByGeometry(IMap map, IGeometry geometry)
        {
            if (map == null || map.Display == null || geometry == null) return false;

            gView.Framework.Data.SpatialFilter filter = new gView.Framework.Data.SpatialFilter();

            filter.Geometry = geometry;
            filter.FilterSpatialReference = map.Display.SpatialReference;
            // FeatureSpatialReference egal, es geht nur um die IDs
            //filter.FeatureSpatialReference = map.Display.SpatialReference;
            filter.SpatialRelation = _relation;

            if (_combinationMethode == CombinationMethod.New) map.ClearSelection();

            foreach (IDatasetElement layer in map.SelectionEnvironment.SelectableElements)
            {
                if (!(layer is IFeatureSelection)) continue;

                ((IFeatureSelection)layer).Select(filter, _combinationMethode);
                ((IFeatureSelection)layer).FireSelectionChangedEvent();
            }
            return true;
        }
    }

    internal class SelectionGraphicsElement : IGraphicElement
    {
        private gView.Plugins.MapTools.Controls.selectionMothode _methode;
        private List<IPoint> _points;
        private SimpleLineSymbol _lineSymbol;
        private SimpleFillSymbol _fillSymbol;
        private SimplePointSymbol _pointSymbol;

        public SelectionGraphicsElement()
        {
            _points = new List<IPoint>();

            _lineSymbol = new SimpleLineSymbol();
            _lineSymbol.Color = System.Drawing.Color.Blue;
            _lineSymbol.Width = 2;
            _pointSymbol = new SimplePointSymbol();
            _pointSymbol.Color = System.Drawing.Color.Blue;
            _pointSymbol.Size = 4;
            _fillSymbol = new SimpleFillSymbol();
            _fillSymbol.Color = System.Drawing.Color.FromArgb(100, 255, 255, 100);
            _fillSymbol.OutlineSymbol = _lineSymbol;
        }

        public gView.Plugins.MapTools.Controls.selectionMothode Methode
        {
            get { return _methode; }
            set { _methode = value; }
        }

        public void AddPoint(IPoint point)
        {
            _points.Add(point);
        }
        public void ClearPoints()
        {
            _points.Clear();
        }

        public IGeometry Geometry
        {
            get
            {
                switch (_methode)
                {
                    case gView.Plugins.MapTools.Controls.selectionMothode.Multipoint:
                        if (_points.Count == 0) return null;
                        return new MultiPoint(_points);
                    case gView.Plugins.MapTools.Controls.selectionMothode.Polyline:
                        if (_points.Count < 2) return null;
                        Polyline line = new Polyline();
                        line.AddPath(new gView.Framework.Geometry.Path(_points));
                        return line;
                    case gView.Plugins.MapTools.Controls.selectionMothode.Polygon:
                        if (_points.Count < 3) return null;
                        Polygon poly = new Polygon();
                        poly.AddRing(new Ring(_points));
                        return poly;
                }
                return null;
            }
        }
        #region IGraphicElement Member

        public void Draw(IDisplay display)
        {
            if (_points.Count == 0) return;

            IGeometry geom = Geometry;

            if (_methode == gView.Plugins.MapTools.Controls.selectionMothode.Multipoint)
            {
                if (geom != null) display.Draw(_pointSymbol, geom);
            }
            else if (_methode == gView.Plugins.MapTools.Controls.selectionMothode.Polyline)
            {
                if (geom != null) display.Draw(_lineSymbol, geom);
                display.Draw(_pointSymbol, new MultiPoint(_points));
            }
            else if (_methode == gView.Plugins.MapTools.Controls.selectionMothode.Polygon)
            {
                if (geom != null) display.Draw(_fillSymbol, geom);
                display.Draw(_pointSymbol, new MultiPoint(_points));
            }
        }

        #endregion
    }

    [RegisterPlugIn("F3DF8F45-4BAC-49ee-82E6-E10711029648")]
    public class Zoom2Selection : gView.Framework.UI.ITool
    {
        IMapDocument _doc = null;
        #region ITool Member

        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[8];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ZoomToSelection", "Zoom To Selection");
            }
        }

        public bool Enabled
        {
            get
            {
                if (_doc == null) return false;
                if (_doc.FocusMap == null) return false;

                foreach (IDatasetElement layer in _doc.FocusMap.MapElements)
                {
                    if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null && ((IWebServiceLayer)layer).WebServiceClass.Themes != null)
                    {
                        foreach (IWebServiceTheme theme in ((IWebServiceLayer)layer).WebServiceClass.Themes)
                        {
                            if (!(theme is IFeatureSelection)) continue;

                            ISelectionSet themeSelSet = ((IFeatureSelection)theme).SelectionSet;
                            if (themeSelSet == null) continue;

                            if (themeSelSet.Count > 0) return true;
                        }
                    }

                    if (!(layer is IFeatureSelection)) continue;

                    ISelectionSet selSet = ((IFeatureSelection)layer).SelectionSet;
                    if (selSet == null) continue;

                    if (selSet.Count > 0) return true;
                }
                return false;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent)) return;

            IMap map = ((MapEvent)MapEvent).Map;
            if (map == null) return;

            Envelope env = null;
            IEnvelope envelope = null;

            double maximumScale = 1.0;

            foreach (IDatasetElement layer in map.MapElements)
            {
                if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null && ((IWebServiceLayer)layer).WebServiceClass.Themes != null)
                {
                    foreach (IWebServiceTheme theme in ((IWebServiceLayer)layer).WebServiceClass.Themes)
                    {
                        envelope = SelectionEnvelope(map, theme as IFeatureLayer);
                        if (envelope == null) continue;
                        if (env == null)
                            env = new Envelope(envelope);
                        else
                            env.Union(envelope);

                        if (theme is ILayer)
                        {
                            maximumScale = Math.Max(maximumScale, ((ILayer)theme).MaximumZoomToFeatureScale);
                        }
                    }
                }

                envelope = SelectionEnvelope(map, layer as IFeatureLayer);
                if (envelope == null) continue;
                if (env == null)
                    env = new Envelope(envelope);
                else
                    env.Union(envelope);

                if (layer is ILayer)
                {
                    maximumScale = Math.Max(maximumScale, ((ILayer)layer).MaximumZoomToFeatureScale);
                }
            }

            if (env != null)
            {
                env.Raise(110.0);
                map.Display.ZoomTo(env);
                if (maximumScale > _doc.FocusMap.Display.mapScale)
                {
                    _doc.FocusMap.Display.mapScale = maximumScale;
                }
                //((Map)map).setScale(env.minx, env.miny, env.maxx, env.maxy, true);
                ((MapEvent)MapEvent).refreshMap = true;
            }

            /*
            if(!(MapEvent is MapEvent)) return;

            Map map=((MapEvent)MapEvent).map;
            if(map==null) return;

            gView.Geometry.Envelope env=null;

            IEnum layers=map.MapLayers;
            layers.Reset();
            ILayer layer;
            while((layer=(ILayer)layers.Next)!=null) 
            {
                if(!(layer is IFeatureSelection)) continue;
				
                if(((IFeatureSelection)layer).SelectionSet==null) continue;
                IQueryResult SelecionSet=((IFeatureSelection)layer).SelectionSet;

                foreach(DataRow row in SelecionSet.Table.Rows) 
                {
                    IGeometry geom=SelecionSet.Shape(row[layer.IDFieldName]);
                    if(geom==null) continue;

                    if(env==null) 
                    {
                        env=new gView.Geometry.Envelope(geom.Envelope);
                    } 
                    else 
                    {
                        env.Union(geom.Envelope);
                    }
                }
				
            }
            if(env!=null) 
            {
                map.setScale(env.minx,env.miny,env.maxx,env.maxy,true);
                ((MapEvent)MapEvent).refreshMap=true;
            }
            */
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion

        #region Helper
        private IEnvelope SelectionEnvelope(IMap map, IFeatureLayer fLayer)
        {
            if (!(fLayer is IFeatureSelection) ||
                fLayer.FeatureClass == null ||
                map == null || map.Display == null ||
                ((IFeatureSelection)fLayer).SelectionSet == null ||
                ((IFeatureSelection)fLayer).SelectionSet.Count == 0) return null;

            ISelectionSet selSet = ((IFeatureSelection)fLayer).SelectionSet;
            IFeatureClass fc = fLayer.FeatureClass;

            bool project = false;
            if (fc.SpatialReference != null && !fc.SpatialReference.Equals(map.Display.SpatialReference))
            {
                project = true;
            }

            IQueryFilter filter = null;
            if (selSet is IIDSelectionSet)
            {
                filter = new RowIDFilter(fc.IDFieldName, ((IIDSelectionSet)selSet).IDs);
            }
            else if (selSet is IQueryFilteredSelectionSet)
            {
                filter = ((IQueryFilteredSelectionSet)selSet).QueryFilter.Clone() as IQueryFilter;
            }
            if (filter == null) return null;

            filter.AddField(fc.ShapeFieldName);
            Envelope env = null;
            using (IFeatureCursor cursor = (fc is ISelectionCache) ? ((ISelectionCache)fc).GetSelectedFeatures() : fc.GetFeatures(filter))
            {
                if (cursor == null) return null;
                IFeature feat;
                while ((feat = cursor.NextFeature) != null)
                {
                    if (feat.Shape == null) continue;

                    IEnvelope envelope = feat.Shape.Envelope;
                    if (project)
                    {
                        IGeometry geom = GeometricTransformer.Transform2D(envelope, fc.SpatialReference, map.Display.SpatialReference);
                        if (geom == null) continue;
                        envelope = geom.Envelope;
                    }
                    if (env == null)
                    {
                        env = new gView.Framework.Geometry.Envelope(envelope);
                    }
                    else
                    {
                        env.Union(envelope);
                    }
                }
                cursor.Dispose();
            }

            return env;
        }
        #endregion
    }

    [RegisterPlugIn("16C05C00-7F21-4216-95A6-0B4B020D3B7D")]
    public class ClearSelection : gView.Framework.UI.ITool
    {
        IMapDocument _doc = null;

        #region ITool Member

        public object Image
        {
            get
            {
                Buttons b = new Buttons();
                return b.imageList1.Images[9];
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.ClearSelection", "Clear Selection");
            }
        }

        public bool Enabled
        {
            get
            {
                if (_doc == null) return false;
                if (_doc.FocusMap == null) return false;

                foreach (IDatasetElement layer in _doc.FocusMap.MapElements)
                {
                    if (layer is IWebServiceLayer && ((IWebServiceLayer)layer).WebServiceClass != null && ((IWebServiceLayer)layer).WebServiceClass.Themes != null)
                    {
                        foreach (IWebServiceTheme theme in ((IWebServiceLayer)layer).WebServiceClass.Themes)
                        {
                            if (!(theme is IFeatureSelection)) continue;

                            ISelectionSet themeSelSet = ((IFeatureSelection)theme).SelectionSet;
                            if (themeSelSet == null) continue;

                            if (themeSelSet.Count > 0) return true;
                        }
                    }

                    if (!(layer is IFeatureSelection)) continue;

                    ISelectionSet selSet = ((IFeatureSelection)layer).SelectionSet;
                    if (selSet == null) continue;

                    if (selSet.Count > 0) return true;
                }
                return false;
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEvent)) return;

            IMap map = ((MapEvent)MapEvent).Map;
            if (map == null) return;

            map.ClearSelection();

            ((MapEvent)MapEvent).refreshMap = true;
            ((MapEvent)MapEvent).drawPhase = DrawPhase.Selection | DrawPhase.Graphics;
        }

        public gView.Framework.UI.ToolType toolType
        {
            get
            {
                return ToolType.command;
            }
        }

        public string ToolTip
        {
            get
            {
                return "";
            }
        }

        #endregion
    }

    /*
	public class AddDataset : gView.Framework.UI.ITool
	{
		#region ITool Member

		public object Image
		{
			get
			{
				Buttons b=new Buttons();
				return b.imageList1.Images[2];	
			}
		}

		public void OnCreate(object hook)
		{
			
		}

		public string Name
		{
			get
			{
				return "Add Data";
			}
		}

		public bool Enabled
		{
			get
			{
				return true;
			}
		}

		public void OnEvent(object MapEvent)
		{
			if(!(MapEvent is MapEvent)) return;


			FormAddDataset dlg=new FormAddDataset(((MapEvent)MapEvent).map);
			dlg.ShowDialog();

			((MapEvent)MapEvent).refreshMap=true;
		}

		public gView.Framework.UI.ToolType toolType
		{
			get
			{
				return gView.Framework.UI.ToolType.command;
			}
		}

		public string ToolTip
		{
			get
			{
				return "";
			}
		}

		#endregion
	}
    */

    [RegisterPlugIn("306C83D1-E4FE-4474-A78E-F581D4304937")]
    public class FeatureClassDataTable : gView.Framework.UI.IDatasetElementContextMenuItem
    {
        IMapDocument _doc;

        #region ILayerContextMenuItem Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.DataTable", "Data Table...");
            }
        }

        public bool Enable(object element)
        {
            if ((element is ITableLayer) || (element is IFeatureLayer))
                return true;

            return false;
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public void OnEvent(object layer, object dataset)
        {
            if (dataset == null || !(layer is ILayer) || _doc == null) return;

            ITableClass table = null;
            if (layer is IFeatureLayer)
                table = ((IFeatureLayer)layer).FeatureClass;
            else if (layer is ITableLayer)
                table = ((ITableLayer)layer).TableClass;

            if (table == null) return;

            string Title = ((ILayer)layer).Title;

            /*
			if(_doc!=null) 
			{
				if(_doc.DocumentWindow!=null) 
				{
					Form frm=_doc.DocumentWindow.GetChildWindow(Title);
					if(frm!=null) 
					{
						frm.WindowState=FormWindowState.Normal;
						frm.Parent.Controls.SetChildIndex(frm,0);
						return;
					}
				}
			}
            */

            if (_doc.Application is IMapApplication)
            {
                IMapApplication appl = (IMapApplication)_doc.Application;

                foreach (IDockableWindow win in appl.DockableWindows)
                {
                    if (win is FormDataTable)
                    {
                        if (((FormDataTable)win).TableClass == table)
                        {
                            // Show The Window
                            appl.ShowDockableWindow(win);
                            return;
                        }
                    }
                }
            }

            FormDataTable dlg = new FormDataTable((ILayer)layer);
            dlg.Text = dlg.Name = Title;
            dlg.MapDocument = _doc;

            /*
			if(_doc!=null) 
			{
				if(_doc.DocumentWindow!=null) 
				{
					_doc.DocumentWindow.AddChildWindow(dlg);	
				}
			}
			dlg.Show();
            */

            if (_doc.Application is IMapApplication)
            {
                IMapApplication appl = (IMapApplication)_doc.Application;

                appl.AddDockableWindow(dlg, PlugInManager.PlugInID(new DataTableContainer()).ToString());
                appl.ShowDockableWindow(dlg);
            }

            _doc = null;
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.table; }
        }

        public int SortOrder
        {
            get { return 22; }
        }
        #endregion
    }

    [RegisterPlugIn("44A1902B-CDC6-43d7-9D48-3DA80437445E")]
    public class TableClassSelectByAttributes : gView.Framework.UI.IDatasetElementContextMenuItem
    {
        IMapDocument _doc;

        #region IDatasetElementContextMenuItem Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.SelectByAttributes", "Select By Attributes..."); }
        }

        public bool Enable(object element)
        {
            if (((element is ITableLayer) || (element is IFeatureLayer)) && element is IFeatureSelection)
                return true;

            return false;
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public void OnEvent(object element, object dataset)
        {
            if (element is IFeatureSelection && element is IFeatureLayer)
            {
                FormQueryBuilder dlg = new FormQueryBuilder((IFeatureLayer)element);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    QueryFilter filter = new QueryFilter();
                    filter.WhereClause = dlg.whereClause;

                    ((IFeatureSelection)element).Select(filter, dlg.combinationMethod);
                    ((IFeatureSelection)element).FireSelectionChangedEvent();

                    if (_doc != null)
                    {
                        if (_doc.Application is IMapApplication)
                        {
                            ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Selection);
                        }
                    }
                }
            }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.sql; }
        }

        public int SortOrder
        {
            get { return 23; }
        }

        #endregion
    }

    [RegisterPlugIn("0F9E298A-C82E-4cae-B1EE-142CF1295D9E")]
    public class FeatureLayerProperties : gView.Framework.UI.IDatasetElementContextMenuItem
    {
        IMapDocument _doc = null;

        #region ILayerContextMenuItem Member

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Properties", "Properties...");
            }
        }

        public bool Enable(object element)
        {
            if (_doc == null || _doc.Application == null) return false;

            if (_doc.Application is IMapApplication &&
                    ((IMapApplication)_doc.Application).ReadOnly == true) return false;

            if (element is ILayer) return true;

            return false;
        }

        public bool Visible(object element)
        {
            return Enable(element);
        }

        public void OnEvent(object layer, object dataset)
        {
            if (!(layer is ILayer) || (!(dataset is IDataset) && !(layer is IGroupLayer))) return;

            if (layer is ILayer)
            {
                FormLayerProperties dlg = new FormLayerProperties((IDataset)dataset, (ILayer)layer);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (_doc != null)
                    {
                        if (_doc.Application is IMapApplication)
                        {
                            ((IMapApplication)_doc.Application).RefreshTOCElement((ILayer)layer);
                            ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                        }
                        if (_doc.FocusMap != null) _doc.FocusMap.TOC.Reset();
                    }
                }
            }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.properties; }
        }

        public int SortOrder
        {
            get { return 99; }
        }

        #endregion
    }

    [RegisterPlugIn("F13D5923-70C8-4c6b-9372-0760D3A8C08C")]
    public class Identify : gView.Framework.UI.ITool, gView.Framework.UI.IToolWindow, IPersistable
    {
        private IMapDocument _doc = null;
        private FormIdentify _dlg = null;
        private ToolType _type = ToolType.click;
        private double _tolerance = 3.0;

        public Identify()
        {
            //_dlg = new FormIdentify();
        }

        #region ITool Members

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Identify", "Identify"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return "Identify"; }
        }

        public ToolType toolType
        {
            get
            {
                if (_dlg == null) return _type;
                if (_type == ToolType.rubberband)
                {
                    QueryThemeCombo combo = QueryCombo;
                    if (combo == null) return _type;
                    if (combo.ThemeMode == QueryThemeMode.Default)
                    {
                        List<IDatasetElement> allQueryableElements = _dlg.AllQueryableLayers;
                        if (allQueryableElements == null) return _type;

                        foreach (IDatasetElement element in allQueryableElements)
                        {
                            if (element.Class is IPointIdentify/* && queryPoint != null*/)
                            {
                                return ToolType.click;
                            }
                        }
                    }

                    return _type;
                }
                else
                {
                    return _type;
                }
            }
            set { _type = value; }
        }

        public object Image
        {
            get
            {
                return global::gView.Plugins.Tools.Properties.Resources.info;
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _dlg = new FormIdentify();
                _dlg.MapDocument = _doc;
            }
            if (hook is Control)
            {

            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_dlg == null || !(MapEvent is MapEvent)) return;
            Envelope envelope = null;
            IMap map = null;

            double tol = 0;

            map = ((MapEvent)MapEvent).Map;
            // 3 Pixel Toleranz
            if (map is IDisplay)
            {
                tol = _tolerance * ((IDisplay)map).mapScale / (96 / 0.0254);  // [m]
                if (map.Display.SpatialReference != null &&
                    map.Display.SpatialReference.SpatialParameters.IsGeographic)
                {
                    tol = (180.0 * tol / Math.PI) / 6370000.0;
                }
            }

            ISpatialReference mapSR = (map.Display.SpatialReference != null) ? map.Display.SpatialReference.Clone() as ISpatialReference : null;

            IPoint queryPoint = null;
            if (MapEvent is MapEventClick)
            {
                MapEventClick ev = (MapEventClick)MapEvent;
                map = ev.Map;
                if (map == null || map.Display == null) return;

                queryPoint = new Point(ev.x, ev.y);
                envelope = new Envelope(ev.x - tol / 2, ev.y - tol / 2, ev.x + tol / 2, ev.y + tol / 2);
                _dlg.Clear();
                _dlg.SetLocation(ev.x, ev.y);
            }
            else if (MapEvent is MapEventRubberband)
            {
                MapEventRubberband ev = (MapEventRubberband)MapEvent;
                map = ev.Map;
                if (map == null || map.Display == null) return;

                envelope = new Envelope(ev.minX, ev.minY, ev.maxX, ev.maxY);
                if (envelope.Width < tol)
                {
                    envelope.minx = 0.5 * envelope.minx + 0.5 * envelope.maxx - tol / 2.0;
                    envelope.maxx = 0.5 * envelope.minx + 0.5 * envelope.maxx + tol / 2.0;
                }
                if (envelope.Height < tol)
                {
                    envelope.miny = 0.5 * envelope.miny + 0.5 * envelope.maxy - tol / 2.0;
                    envelope.maxy = 0.5 * envelope.miny + 0.5 * envelope.maxy + tol / 2.0;
                }
                _dlg.Clear();
                //_dlg.setLocation(envelope.);
            }
            else
            {
                return;
            }

            QueryThemeCombo combo = QueryCombo;

            if (combo == null || combo.ThemeMode == QueryThemeMode.Default)
            {
                #region ThemeMode Default
                IdentifyMode mode = _dlg.Mode;

                int counter = 0;
                List<IDatasetElement> allQueryableElements = _dlg.AllQueryableLayers;
                if (allQueryableElements == null) return;

                foreach (IDatasetElement element in allQueryableElements)
                {
                    #region IPointIdentify
                    if (element.Class is IPointIdentify/* && queryPoint != null*/)
                    {
                        IPoint iPoint = null;
                        if (queryPoint != null)
                        {
                            iPoint = queryPoint;
                        }
                        else if (queryPoint == null && envelope != null)
                        {
                            if (envelope.Width < 3.0 * tol && envelope.Height < 3.0 * tol)
                            {
                                iPoint = new Point((envelope.minx + envelope.maxx) * 0.5, (envelope.miny + envelope.maxy) * 0.5);
                            }
                        }

                        if (iPoint != null)
                        {
                            using (ICursor cursor = ((IPointIdentify)element.Class).PointQuery(map.Display, queryPoint, map.Display.SpatialReference, new UserData()))
                            {
                                if (cursor is IRowCursor)
                                {
                                    IRow row;
                                    while ((row = ((IRowCursor)cursor).NextRow) != null)
                                    {
                                        _dlg.AddFeature(new Feature(row), mapSR, null, element.Title);
                                        counter++;
                                    }
                                }
                                else if (cursor is ITextCursor)
                                {
                                    _dlg.IdentifyText += ((ITextCursor)cursor).Text;
                                }
                                else if (cursor is IUrlCursor)
                                {
                                    _dlg.IdentifyUrl = ((IUrlCursor)cursor).Url;
                                }
                            }
                        }
                    }
                    #endregion

                    if (!(element is IFeatureLayer)) continue;
                    IFeatureLayer layer = (IFeatureLayer)element;

                    IFeatureClass fc = layer.Class as IFeatureClass;
                    if (fc == null) continue;

                    #region QueryFilter
                    SpatialFilter filter = new SpatialFilter();
                    filter.Geometry = envelope;
                    filter.FilterSpatialReference = map.Display.SpatialReference;
                    filter.FeatureSpatialReference = map.Display.SpatialReference;
                    filter.SpatialRelation = spatialRelation.SpatialRelationIntersects;

                    if (layer.FilterQuery != null)
                    {
                        filter.WhereClause = layer.FilterQuery.WhereClause;
                    }

                    IFields fields = layer.Fields;
                    if (fields == null)
                    {
                        filter.SubFields = "*";
                    }
                    else
                    {
                        foreach (IField field in fields.ToEnumerable())
                        {
                            if (!field.visible) continue;
                            filter.AddField(field.name);
                        }
                        if (layer.Fields.PrimaryDisplayField != null) filter.AddField(layer.Fields.PrimaryDisplayField.name);
                    }
                    #endregion

                    #region Layer Title
                    string primaryFieldName = (layer.Fields.PrimaryDisplayField != null) ? layer.Fields.PrimaryDisplayField.name : "";
                    string title = element.Title;

                    if (map.TOC != null)
                    {
                        ITOCElement tocElement = map.TOC.GetTOCElement(layer);
                        if (tocElement != null) title = tocElement.Name;
                    }
                    #endregion

                    #region Query
                    using (IFeatureCursor cursor = (IFeatureCursor)fc.Search(filter))
                    {
                        if (cursor != null)
                        {
                            IFeature feature;
                            while ((feature = cursor.NextFeature) != null)
                            {
                                _dlg.AddFeature(feature, mapSR, layer, title);
                                counter++;
                            }
                            cursor.Dispose();
                        }
                    }
                    #endregion

                    if (mode == IdentifyMode.topmost && counter > 0) break;
                }
                #endregion
            }
            else if (combo.ThemeMode == QueryThemeMode.Custom)
            {
                #region ThemeMode Custom
                foreach (QueryTheme theme in combo.UserDefinedQueries.Queries)
                {
                    if (theme.Text == combo.Text)
                    {
                        foreach (QueryThemeTable table in theme.Nodes)
                        {
                            IFeatureLayer layer = table.GetLayer(_doc) as IFeatureLayer;
                            if (layer == null || !(layer.Class is IFeatureClass)) continue;
                            IFeatureClass fc = layer.Class as IFeatureClass;

                            #region Fields
                            IFields fields = null;
                            IField primaryDisplayField = null;

                            if (layer.Fields != null && table.VisibleFieldDef != null && table.VisibleFieldDef.UseDefault == false)
                            {
                                fields = new Fields();

                                foreach (IField field in layer.Fields.ToEnumerable())
                                {
                                    if (table.VisibleFieldDef.PrimaryDisplayField == field.name)
                                        primaryDisplayField = field;

                                    DataRow[] r = table.VisibleFieldDef.Select("Visible=true AND Name='" + field.name + "'");
                                    if (r.Length == 0) continue;

                                    Field f = new Field(field);
                                    f.visible = true;
                                    f.aliasname = (string)r[0]["Alias"];
                                    ((Fields)fields).Add(f);
                                }
                            }
                            else
                            {
                                fields = layer.Fields;
                                primaryDisplayField = layer.Fields.PrimaryDisplayField;
                            }
                            #endregion

                            #region QueryFilter
                            SpatialFilter filter = new SpatialFilter();
                            filter.Geometry = envelope;
                            filter.FilterSpatialReference = map.Display.SpatialReference;
                            filter.FeatureSpatialReference = map.Display.SpatialReference;
                            filter.SpatialRelation = spatialRelation.SpatialRelationIntersects;

                            if (layer.FilterQuery != null)
                            {
                                filter.WhereClause = layer.FilterQuery.WhereClause;
                            }

                            if (fields == null)
                            {
                                filter.SubFields = "*";
                            }
                            else
                            {
                                foreach (IField field in fields.ToEnumerable())
                                {
                                    if (!field.visible) continue;
                                    filter.AddField(field.name);
                                }
                                if (primaryDisplayField != null) filter.AddField(primaryDisplayField.name);
                            }
                            #endregion

                            #region Layer Title
                            string primaryFieldName = (layer.Fields.PrimaryDisplayField != null) ? layer.Fields.PrimaryDisplayField.name : "";
                            string title = layer.Title;

                            if (map.TOC != null)
                            {
                                ITOCElement tocElement = map.TOC.GetTOCElement(layer);
                                if (tocElement != null) title = tocElement.Name;
                            }
                            #endregion

                            #region Query
                            using (IFeatureCursor cursor = (IFeatureCursor)fc.Search(filter))
                            {
                                IFeature feature;
                                while ((feature = cursor.NextFeature) != null)
                                {
                                    _dlg.AddFeature(feature, mapSR, layer, title, fields, primaryDisplayField);
                                }
                                cursor.Dispose();
                            }
                            #endregion
                        }
                    }
                }
                #endregion
            }

            _dlg.WriteFeatureCount();
            _dlg.ShowResult();

            if (_doc.Application is IGUIApplication)
            {
                IGUIApplication appl = (IGUIApplication)_doc.Application;

                foreach (IDockableWindow win in appl.DockableWindows)
                {
                    if (win == _dlg)
                    {
                        appl.ShowDockableWindow(win);
                        return;
                    }
                }

                appl.AddDockableWindow(_dlg, null);
                appl.ShowDockableWindow(_dlg);
            }
        }

        private object ICursor(IPointIdentify iPointIdentify)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IToolWindow Members

        public IDockableWindow ToolWindow
        {
            get { return _dlg; }
        }

        #endregion

        public double Tolerance
        {
            get { return _tolerance; }
            set { _tolerance = value; }
        }

        private QueryThemeCombo QueryCombo
        {
            get
            {
                if (_doc == null || !(_doc.Application is IGUIApplication)) return null;
                return ((IGUIApplication)_doc.Application).Tool(new Guid("51A2CF81-E343-4c58-9A42-9207C8DFBC01")) as QueryThemeCombo;
            }
        }

        private Find FindTool
        {
            get
            {
                if (_doc == null || !(_doc.Application is IGUIApplication)) return null;
                return ((IGUIApplication)_doc.Application).Tool(new Guid("ED5B0B59-2F5D-4b1a-BAD2-3CABEF073A6A")) as Find;
            }
        }

        internal QueryThemes UserDefinedQueries
        {
            get
            {
                QueryThemeCombo combo = QueryCombo;
                if (combo == null) return null;

                return combo.UserDefinedQueries;
            }
            set
            {
                QueryThemeCombo combo = QueryCombo;
                if (combo == null) return;

                combo.UserDefinedQueries = value;
            }
        }

        internal QueryThemeMode ThemeMode
        {
            get
            {
                QueryThemeCombo combo = QueryCombo;
                if (combo == null) return QueryThemeMode.Default;

                return combo.ThemeMode;
            }
            set
            {
                QueryThemeCombo combo = QueryCombo;
                if (combo != null)
                {
                    combo.ThemeMode = value;
                    combo.RebuildCombo();
                }

                Find find = FindTool;
                if (find != null)
                {
                    find.ThemeMode = value;
                }
            }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _type = (ToolType)stream.Load("ToolType", (int)ToolType.click);
            _tolerance = (double)stream.Load("Tolerance", (double)3.0);

            UserDefinedQueries = stream.Load("UserDefinedQueries", null, new QueryThemes(null)) as QueryThemes;
            ThemeMode = (QueryThemeMode)stream.Load("QueryMode", (int)QueryThemeMode.Default);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("ToolType", (int)_type);
            stream.Save("Tolerance", _tolerance);

            if (UserDefinedQueries != null) stream.Save("UserDefinedQueries", UserDefinedQueries);
            stream.Save("QueryMode", (int)ThemeMode);
        }

        #endregion
    }

    [RegisterPlugIn("ED5B0B59-2F5D-4b1a-BAD2-3CABEF073A6A")]
    public class Find : gView.Framework.UI.ITool, IShortCut
    {
        private IMapDocument _doc = null;
        private FormIdentify _dlgIdentify = null;
        private FormQuery _dlgQuery = null;

        public QueryThemeMode ThemeMode
        {
            set
            {
                if (_dlgQuery != null)
                    _dlgQuery.ThemeMode = value;
            }
        }

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Find", "Find..."); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.find; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                if (_doc != null && _doc.Application is IGUIApplication)
                {
                    Identify identify = ((IGUIApplication)_doc.Application).Tool(new Guid("F13D5923-70C8-4c6b-9372-0760D3A8C08C")) as Identify;
                    if (identify != null) _dlgIdentify = identify.ToolWindow as FormIdentify;
                }
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_dlgIdentify != null && _doc != null && _doc.Application is IMapApplication)
            {
                ((IMapApplication)_doc.Application).AddDockableWindow(_dlgIdentify, DockWindowState.right);
                ((IMapApplication)_doc.Application).ShowDockableWindow(_dlgIdentify);

                if (_dlgQuery == null)
                {
                    _dlgQuery = new FormQuery(_dlgIdentify);
                    _dlgQuery.Show(((IGUIApplication)_doc.Application).ApplicationWindow);
                }
                _dlgQuery.Visible = true;
                _dlgQuery.BringToFront();
            }
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.F;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+F";
            }
        }

        #endregion
    }

    [RegisterPlugIn("1E21835C-FD41-4e68-8462-9FAA66EA5A54")]
    public class ScaleText : gView.Framework.UI.ITool, gView.Framework.UI.IToolItem
    {
        #region ITool Member

        public string Name
        {
            get { return "QueryThemeText"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {

        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get
            {

                return new ToolStripLabel(
                    LocalizedResources.GetResString("Text.Search", "Search:")
                    );
            }
        }

        #endregion
    }

    public enum QueryThemeMode { Default, Custom }
    [RegisterPlugIn("51A2CF81-E343-4c58-9A42-9207C8DFBC01")]
    public class QueryThemeCombo : gView.Framework.UI.ITool, gView.Framework.UI.IToolItem, IToolItemLabel
    {
        internal delegate void SelectedItemChangedEvent(string itemText);
        internal SelectedItemChangedEvent SelectedItemChanged = null;

        private IMapDocument _doc = null;
        private ToolStripComboBox _dropDown;
        private QueryThemes _queries = null;
        private QueryThemeMode _mode = QueryThemeMode.Default;

        public QueryThemeCombo()
        {
            _dropDown = new ToolStripComboBox();

            _dropDown.Items.Clear();
            _dropDown.Items.Add(new ModeItem(IdentifyMode.visible));
            _dropDown.Items.Add(new ModeItem(IdentifyMode.selectable));
            _dropDown.Items.Add(new ModeItem(IdentifyMode.all));
            _dropDown.Items.Add(new ModeItem(IdentifyMode.topmost));
            _dropDown.SelectedIndex = 0;

            _dropDown.Width = 300;
            _dropDown.DropDownStyle = ComboBoxStyle.DropDownList;
            _dropDown.DropDown += new EventHandler(dropDown_DropDown);
            _dropDown.SelectedIndexChanged += new EventHandler(dropDown_SelectedIndexChanged);
        }

        #region DropDownEvents

        internal void RebuildCombo()
        {
            switch (_mode)
            {
                case QueryThemeMode.Default:
                    RebuildDefaultCombo();
                    break;
                case QueryThemeMode.Custom:
                    RebuildCustomCombo();
                    break;
            }
            if (_dropDown.SelectedIndex == -1 && _dropDown.Items.Count > 0)
            {
                _dropDown.SelectedIndex = 0;
            }
        }
        private void RebuildDefaultCombo()
        {
            ITOCElement selectedElement = null;
            if (_dropDown.SelectedItem is TOCElementItem)
            {
                selectedElement = ((TOCElementItem)_dropDown.SelectedItem).Element;
            }

            _dropDown.Items.Clear();
            _dropDown.Items.Add(new ModeItem(IdentifyMode.visible));
            _dropDown.Items.Add(new ModeItem(IdentifyMode.selectable));
            _dropDown.Items.Add(new ModeItem(IdentifyMode.all));
            _dropDown.Items.Add(new ModeItem(IdentifyMode.topmost));

            if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.TOC != null)
            {
                foreach (ITOCElement element in _doc.FocusMap.TOC.Elements)
                {
                    foreach (ILayer layer in element.Layers)
                    {
                        if (layer == null) continue;

                        if (layer.Class is IFeatureClass ||
                            layer.Class is IPointIdentify)
                        {
                            _dropDown.Items.Add(new TOCElementItem(element));
                            break;
                        }
                        //if (layer is IFeatureLayer)
                        //{
                        //    _dropDown.Items.Add(new TOCElementItem(element));
                        //    break;
                        //}
                    }
                }
            }

            if (_dropDown.SelectedItem == null)
            {
                for (int i = 0; i < _dropDown.Items.Count; i++)
                {
                    if (!(_dropDown.Items[i] is TOCElementItem)) continue;
                    if (((TOCElementItem)_dropDown.Items[i]).Element == selectedElement)
                    {
                        _dropDown.SelectedItem = _dropDown.Items[i];
                        break;
                    }
                }
            }

            using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(1, 1))
            {
                using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                {
                    _dropDown.DropDownWidth = _dropDown.Size.Width;
                    foreach (object obj in _dropDown.Items)
                    {
                        System.Drawing.SizeF size = gr.MeasureString(obj.ToString(), _dropDown.Font);

                        if (size.Width + 20 > _dropDown.DropDownWidth)
                            _dropDown.DropDownWidth = (int)size.Width + 20;
                    }
                }
            }
        }
        private void RebuildCustomCombo()
        {
            string selected = "";
            if (_dropDown.SelectedItem != null) selected = _dropDown.SelectedItem.ToString();

            _dropDown.Items.Clear();
            if (_queries == null) return;

            foreach (QueryTheme theme in _queries.Queries)
            {
                if (theme.Type == QueryTheme.NodeType.query)
                    _dropDown.Items.Add(theme.Text);
                else if (theme.Type == QueryTheme.NodeType.seperator)
                    _dropDown.Items.Add("------------------------------");
            }
            if (_dropDown.SelectedItem == null && _dropDown.Items.IndexOf(selected) != -1)
            {
                _dropDown.SelectedItem = selected;
            }

            using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(1, 1))
            {
                using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                {
                    _dropDown.DropDownWidth = _dropDown.Size.Width;
                    foreach (object obj in _dropDown.Items)
                    {
                        System.Drawing.SizeF size = gr.MeasureString(obj.ToString(), _dropDown.Font);

                        if (size.Width + 20 > _dropDown.DropDownWidth)
                            _dropDown.DropDownWidth = (int)size.Width + 20;
                    }
                }
            }
        }
        private void dropDown_DropDown(object sender, EventArgs e)
        {
            RebuildCombo();
        }
        void dropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dropDown.SelectedItem == null) return;
            if (SelectedItemChanged != null) SelectedItemChanged(_dropDown.SelectedItem.ToString());
        }
        #endregion

        internal IdentifyMode Mode
        {
            get
            {
                if (_dropDown.SelectedItem is ModeItem)
                    return ((ModeItem)_dropDown.SelectedItem).Mode;
                return IdentifyMode.layer;
            }
        }
        internal List<ILayer> Layers
        {
            get
            {
                if (_dropDown.SelectedItem is TOCElementItem && ((TOCElementItem)_dropDown.SelectedItem).Element != null)
                {
                    return ((TOCElementItem)_dropDown.SelectedItem).Element.Layers;
                }
                return null;
            }
        }

        public List<IDatasetElement> QueryableDatasetElements
        {
            get
            {
                if (_doc == null || _doc.FocusMap == null) return null;
                IMap map = _doc.FocusMap;

                List<IDatasetElement> layers;
                if (Mode == IdentifyMode.selectable)
                {
                    layers = map.SelectionEnvironment.SelectableElements;
                }
                else if (Mode == IdentifyMode.layer)
                {
                    if (this.Layers == null) return null;
                    layers = new List<IDatasetElement>();
                    foreach (ILayer layer in this.Layers)
                        layers.Add(layer);
                }
                else
                {
                    if (map.TOC != null && map.TOC.Layers != null)
                    {
                        layers = new List<IDatasetElement>();
                        foreach (ILayer layer in map.TOC.Layers)
                            layers.Add(layer);
                    }
                    else
                    {
                        layers = map.MapElements;
                    }
                }

                // Service Themes hinzufügen...
                List<IDatasetElement> allQueryableElements = new List<IDatasetElement>();
                foreach (IDatasetElement element in layers)
                {
                    if (element == null) continue;

                    if (element.Class is IFeatureClass ||
                        element.Class is IPointIdentify)
                        allQueryableElements.Add(element);
                    else if (element is IWebServiceLayer)
                    {
                        foreach (IWebServiceTheme theme in ((IWebServiceLayer)element).WebServiceClass.Themes)
                        {
                            if (theme.Class is IFeatureClass)
                                allQueryableElements.Add(theme);
                        }
                    }
                }

                if (Mode == IdentifyMode.visible)
                {
                    List<IDatasetElement> remove = new List<IDatasetElement>();
                    foreach (IDatasetElement element in allQueryableElements)
                    {
                        if (!(element is ILayer)) continue;

                        ILayer layer = element as ILayer;
                        if ((layer.Visible == false) ||
                            (layer.MinimumScale > 1 && layer.MinimumScale > map.Display.mapScale) ||
                            (layer.MaximumScale > 1 && layer.MaximumScale < map.Display.mapScale))
                        {
                            remove.Add(element);
                        }
                    }
                    foreach (IDatasetElement rem in remove)
                        allQueryableElements.Remove(rem);
                }

                return allQueryableElements;
            }
        }
        public string Text
        {
            get
            {
                if (_dropDown.SelectedItem == null) return "";
                return _dropDown.SelectedItem.ToString();
            }
        }
        public QueryThemeMode ThemeMode
        {
            get { return _mode; }
            set { _mode = value; }
        }
        internal QueryThemes UserDefinedQueries
        {
            get { return _queries; }
            set { _queries = value; }
        }

        #region ITool Member

        public string Name
        {
            get { return "QueryThemeCombo"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
            }
        }

        public void OnEvent(object MapEvent)
        {

        }

        #endregion

        #region IToolItem Member

        public ToolStripItem ToolItem
        {
            get { return _dropDown; }
        }

        #endregion

        #region IToolItemLabel Member

        public string Label
        {
            get { return LocalizedResources.GetResString("Text.Search", "Search:"); }
        }

        public ToolItemLabelPosition LabelPosition
        {
            get { return ToolItemLabelPosition.top; }
        }

        #endregion
    }

    [RegisterPlugIn("D185D794-4BC8-4f3c-A5EA-494155692EAC")]
    public class Measure : gView.Framework.Snapping.Core.SnapTool, ITool, IToolContextMenu, IToolWindow
    {
        internal delegate void ShapeChangedEventHandler(MeasureGraphicsElement grElement);
        internal event ShapeChangedEventHandler ShapeChanged = null;

        IMapDocument _doc = null;
        GraphicsContainer _container;
        MeasureGraphicsElement _grElement;
        GeoUnits _lengthUnit = GeoUnits.Unknown, _areaUnit = GeoUnits.Unknown;
        private FormMeasure _dlg = null;

        public Measure()
        {
            this.InitializeComponents();

            _container = new GraphicsContainer();
            _container.Elements.Add(_grElement = new MeasureGraphicsElement());
        }

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Measure", "Measure"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.click; }
        }

        public object Image
        {
            get { return (new Buttons()).imageList1.Images[19]; }
        }

        public override void OnCreate(object hook)
        {
            base.OnCreate(hook);

            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                _dlg = new FormMeasure(_doc);

                if (_doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).ActiveMapToolChanged += new ActiveMapToolChangedEvent(Measure_ActiveMapToolChanged);
                }
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (!(MapEvent is MapEventClick)) return;
            MapEventClick ev = (MapEventClick)MapEvent;

            if (_grElement.Stopped) RemoveGraphicFromMap();

            if (_lengthUnit == GeoUnits.Unknown && ev.Map != null && ev.Map.Display != null && ev.Map.Display.DisplayUnits != GeoUnits.Unknown)
            {
                _lengthUnit = ev.Map.Display.DisplayUnits;
            }
            if (_areaUnit == GeoUnits.Unknown && ev.Map != null && ev.Map.Display != null && ev.Map.Display.DisplayUnits != GeoUnits.Unknown)
            {
                _areaUnit = ev.Map.Display.DisplayUnits;
            }

            _grElement.AddPoint(ev.x, ev.y);
            ev.Map.Display.DrawOverlay(_container, true);
            if (ShapeChanged != null) ShapeChanged(_grElement);

            SetStatusText();
        }

        #endregion

        void Measure_ActiveMapToolChanged(ITool OldTool, ITool NewTool)
        {
            if (_doc != null && _doc.Application is IMapApplication)
            {
                if (NewTool == this)
                {
                    ((IMapApplication)_doc.Application).OnCursorPosChanged -= new OnCursorPosChangedEvent(Application_OnCursorPosChanged);
                    ((IMapApplication)_doc.Application).OnCursorPosChanged += new OnCursorPosChangedEvent(Application_OnCursorPosChanged);
                }
                else if (OldTool == this)
                {
                    ((IMapApplication)_doc.Application).OnCursorPosChanged -= new OnCursorPosChangedEvent(Application_OnCursorPosChanged);
                }
            }
        }

        private double _X, _Y;
        void Application_OnCursorPosChanged(double X, double Y)
        {
            SetStatusText();
            _X = X;
            _Y = Y;

            IPoint moveAble = ((MeasureGraphicsElement)_container.Elements[0]).MoveAble;
            if (moveAble == null)
            {
                return;
            }

            moveAble.X = X;
            moveAble.Y = Y;

            if (cmnDynamic.Checked)
            {
                _doc.FocusMap.Display.DrawOverlay(_container, true);
            }

            _grElement.Dynamic = cmnDynamic.Checked;
            SetStatusText();
        }

        internal void RemoveGraphicFromMap()
        {
            if (_doc == null) return;

            foreach (IMap map in _doc.Maps)
            {
                if (map.Display == null || map.Display.GraphicsContainer == null) continue;
                if (map.Display.GraphicsContainer.Elements.Contains(_grElement))
                {
                    map.Display.GraphicsContainer.Elements.Remove(_grElement);
                    //if (map == _doc.FocusMap) map.RefreshMap(DrawPhase.Graphics, null);
                    if (_doc.Application is IMapApplication)
                        ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
                }
            }
            _grElement.BeginNew();
            if (ShapeChanged != null) ShapeChanged(_grElement);
        }

        internal void Stop(bool close)
        {
            _grElement.Stop();
            if (close) _grElement.Close();

            if (!_doc.FocusMap.Display.GraphicsContainer.Elements.Contains(_grElement))
                _doc.FocusMap.Display.GraphicsContainer.Elements.Add(_grElement);
            //if (_doc.Application != null) _doc.Application.RefreshActiveMap(DrawPhase.Graphics);

            SetStatusText();
        }

        private void SetStatusText()
        {
            if (_doc == null || !(_doc.Application is IMapApplication) || _doc.FocusMap == null || _doc.FocusMap.Display == null) return;
            IStatusBar statusbar = ((IMapApplication)_doc.Application).StatusBar;
            if (statusbar == null) return;

            double length = _grElement.Length;
            double area = _grElement.Area;
            double segLength = _grElement.SegmentLength;
            double segAngle = _grElement.SegmentAngle;

            GeoUnitConverter converter = new GeoUnitConverter();
            length = Math.Round(converter.Convert(length, _doc.FocusMap.Display.MapUnits, _lengthUnit), 2);
            area = Math.Round(converter.Convert(area, _doc.FocusMap.Display.MapUnits, _areaUnit, 2), 2);
            segLength = Math.Round(converter.Convert(segLength, _doc.FocusMap.Display.MapUnits, _lengthUnit), 2);
            segAngle = Math.Round(segAngle, 2);

            string msg = "";
            if (!_grElement.Stopped)
            {
                msg += "Segment Length=" + segLength + " " + _lengthUnit.ToString();
                msg += " Angle=" + segAngle + "\u00b0 ";
            }
            msg += "Total Length=" + length + " " + _lengthUnit.ToString();
            if (area > 0.0) msg += " Area=" + area + " " + _areaUnit.ToString() + "\u00b2";  // ^2

            statusbar.Text = msg;

            if (_dlg != null)
            {
                _dlg.SetSegmentLength(segLength, _lengthUnit);
                _dlg.SetSegmentAngle(segAngle);
                _dlg.SetTotalLength(length, _lengthUnit);
                _dlg.SetTotalArea(area, _areaUnit);
            }
        }

        #region IToolContextMenu Member

        #region GUI
        private ContextMenuStrip contextMenuStripMeasure;
        private ToolStripMenuItem cmnDynamic;
        private ToolStripMenuItem cmnStopMeasuring;
        private ToolStripMenuItem cmnClosePolygonAndStop;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem cmnShowArea;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem cmnDistanceUnit;
        private ToolStripMenuItem cmnAreaUnit;

        private void InitializeComponents()
        {
            contextMenuStripMeasure = new System.Windows.Forms.ContextMenuStrip();
            cmnDynamic = new ToolStripMenuItem();
            cmnStopMeasuring = new System.Windows.Forms.ToolStripMenuItem();
            cmnClosePolygonAndStop = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            cmnShowArea = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            cmnDistanceUnit = new System.Windows.Forms.ToolStripMenuItem();
            cmnAreaUnit = new System.Windows.Forms.ToolStripMenuItem();
            // 
            // contextMenuStripMeasure
            // 
            this.contextMenuStripMeasure.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmnDynamic,
            this.cmnStopMeasuring,
            this.cmnClosePolygonAndStop,
            this.toolStripSeparator1,
            this.cmnShowArea,
            this.toolStripSeparator2,
            cmnDistanceUnit,
            cmnAreaUnit});
            this.contextMenuStripMeasure.Name = "contextMenuStripMeasure";
            this.contextMenuStripMeasure.Size = new System.Drawing.Size(248, 104);
            // 
            // cmnDynamic
            // 
            this.cmnDynamic.Name = "cmnDynamic";
            this.cmnDynamic.Size = new System.Drawing.Size(247, 22);
            this.cmnDynamic.Text = "Dynamic";
            this.cmnDynamic.Checked = true;
            this.cmnDynamic.Click += new EventHandler(cmnDynamic_Click);
            // 
            // 
            // cmnStopMeasuring
            // 
            this.cmnStopMeasuring.Name = "cmnStopMeasuring";
            this.cmnStopMeasuring.Size = new System.Drawing.Size(247, 22);
            this.cmnStopMeasuring.Text = "Stop Measuring";
            this.cmnStopMeasuring.Click += new EventHandler(cmnStopMeasuring_Click);
            // 
            // cmnClosePolygonAndStop
            // 
            this.cmnClosePolygonAndStop.Name = "cmnClosePolygonAndStop";
            this.cmnClosePolygonAndStop.Size = new System.Drawing.Size(247, 22);
            this.cmnClosePolygonAndStop.Text = "Close Polygon and Stop Measuring";
            this.cmnClosePolygonAndStop.Click += new EventHandler(cmnClosePolygonAndStop_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(244, 6);
            // 
            // cmnShowArea
            // 
            this.cmnShowArea.Name = "cmnShowArea";
            this.cmnShowArea.Size = new System.Drawing.Size(247, 22);
            this.cmnShowArea.Text = "Show Area";
            this.cmnShowArea.Click += new EventHandler(cmnShowArea_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(244, 6);
            // 
            // cmnDistanceUnit
            // 
            this.cmnDistanceUnit.Name = "cmnDistanceUnit";
            this.cmnDistanceUnit.Size = new System.Drawing.Size(247, 22);
            this.cmnDistanceUnit.Text = "Distance Unit";
            // 
            // cmnAreaUnit
            // 
            this.cmnAreaUnit.Name = "cmnAreaUnit";
            this.cmnAreaUnit.Size = new System.Drawing.Size(247, 22);
            this.cmnAreaUnit.Text = "Area Unit";

            // Distance Units
            foreach (GeoUnits unit in Enum.GetValues(typeof(GeoUnits)))
            {
                if ((int)unit <= 0) continue;

                UnitMenuItem item1 = new UnitMenuItem(unit, false);
                UnitMenuItem item2 = new UnitMenuItem(unit, true);

                item1.Click += new EventHandler(UnitItem_Click);
                item2.Click += new EventHandler(UnitItem_Click);
                cmnDistanceUnit.DropDownItems.Add(item1);
                cmnAreaUnit.DropDownItems.Add(item2);
            }
        }

        void UnitItem_Click(object sender, EventArgs e)
        {
            if (!(sender is UnitMenuItem)) return;

            if (((UnitMenuItem)sender).Square)
                _areaUnit = ((UnitMenuItem)sender).Unit;
            else
                _lengthUnit = ((UnitMenuItem)sender).Unit;

            SetStatusText();
        }

        void cmnDynamic_Click(object sender, EventArgs e)
        {
            cmnDynamic.Checked = !cmnDynamic.Checked;
            _grElement.Dynamic = cmnDynamic.Checked;

            _doc.FocusMap.Display.DrawOverlay(_container, true);
        }

        void cmnShowArea_Click(object sender, EventArgs e)
        {
            ((MeasureGraphicsElement)_container.Elements[0]).ShowArea = !((MeasureGraphicsElement)_container.Elements[0]).ShowArea;
        }

        void cmnClosePolygonAndStop_Click(object sender, EventArgs e)
        {
            this.Stop(true);
        }

        void cmnStopMeasuring_Click(object sender, EventArgs e)
        {
            this.Stop(false);
        }

        #endregion

        public ContextMenuStrip ContextMenu
        {
            get
            {
                cmnStopMeasuring.Enabled = !_grElement.Stopped;
                cmnClosePolygonAndStop.Enabled = !_grElement.Stopped;
                cmnShowArea.Checked = _grElement.ShowArea;

                foreach (UnitMenuItem item in cmnDistanceUnit.DropDownItems)
                    item.Checked = _lengthUnit == item.Unit;
                foreach (UnitMenuItem item in cmnAreaUnit.DropDownItems)
                    item.Checked = _areaUnit == item.Unit;

                return contextMenuStripMeasure;
            }
        }

        #endregion

        #region IToolWindow Member

        public IDockableWindow ToolWindow
        {
            get { return _dlg; }
        }

        #endregion

        public override bool ShowSnapMarker
        {
            get
            {
                if (_grElement != null && _grElement.PointCount > 0)
                    return false;

                return base.ShowSnapMarker;
            }
        }
    }

    internal class UnitMenuItem : ToolStripMenuItem
    {
        private GeoUnits _unit;
        private bool _square;

        public UnitMenuItem(GeoUnits unit, bool square)
        {
            _unit = unit;
            _square = square;

            if (!square)
                this.Text = unit.ToString();
            else
                this.Text = "Square " + unit.ToString();
        }

        public GeoUnits Unit
        {
            get { return _unit; }
        }

        public bool Square
        {
            get { return _square; }
        }
    }

    internal class MeasureGraphicsElement : IGraphicElement
    {
        private IPolygon _polygon;
        private SimpleLineSymbol _lineSymbol;
        private HatchSymbol _fillSymbol;
        private SimplePointSymbol _pointSymbol;
        private object lockThis = new object();
        private bool _stopped = true, _showArea = true, _dynamic = false;

        public MeasureGraphicsElement()
        {
            BeginNew();

            _lineSymbol = new SimpleLineSymbol();
            _lineSymbol.Color = System.Drawing.Color.Blue;
            _lineSymbol.Width = 2;
            _pointSymbol = new SimplePointSymbol();
            _pointSymbol.Color = System.Drawing.Color.Blue;
            _pointSymbol.Size = 4;
            _fillSymbol = new HatchSymbol();
        }

        public void BeginNew()
        {
            _polygon = new Polygon();
            _polygon.AddRing(new Ring());
            _stopped = false;
        }

        public void Stop()
        {
            _stopped = true;

            if (_moveAble != null) // MoveAble löschen
            {
                _moveAble = null;
                _polygon[0].RemovePoint(_polygon[0].PointCount - 1);
            }
        }

        public void Close()
        {
            if (_moveAble == null)
            {
                if (_polygon[0].PointCount > 2)
                {
                    _polygon[0].AddPoint(new Point(_polygon[0][0].X, _polygon[0][0].Y));
                }
            }
            else
            {
                if (_polygon[0].PointCount > 3)
                {
                    _moveAble.X = _polygon[0][0].X;
                    _moveAble.Y = _polygon[0][0].Y;
                    _moveAble = null;
                }
            }
        }

        public bool Stopped { get { return _stopped; } }

        public bool ShowArea
        {
            get { return _showArea; }
            set { _showArea = value; }
        }

        private IPoint _moveAble = null;
        public void AddPoint(double X, double Y)
        {
            if (_stopped) BeginNew();

            lock (lockThis)
            {
                if (_moveAble != null)
                {
                    _moveAble.X = X;
                    _moveAble.Y = Y;
                    _moveAble = null;
                }
                else
                {
                    _polygon[0].AddPoint(new Point(X, Y));
                }
            }
        }

        public IPoint MoveAble
        {
            get
            {
                if (_stopped || _polygon[0].PointCount == 0) return null;

                lock (lockThis)
                {
                    if (_moveAble == null) _polygon[0].AddPoint(new Point(0, 0));
                    return _moveAble = _polygon[0][_polygon[0].PointCount - 1];
                }
            }
        }

        public bool Dynamic
        {
            get { return _dynamic; }
            set { _dynamic = value; }
        }
        public double Length
        {
            get
            {
                //return _polygon[0].Length;

                gView.Framework.Geometry.Path path = new gView.Framework.Geometry.Path();
                for (int i = 0; i < _polygon[0].PointCount; i++)
                {
                    if (!_dynamic && _polygon[0][i] == _moveAble) continue;
                    path.AddPoint(_polygon[0][i]);
                }
                return path.Length;
            }
        }
        public double SegmentLength
        {
            get
            {
                if (_moveAble == null || _polygon[0].PointCount < 2) return 0.0;

                double dx = _moveAble.X - _polygon[0][_polygon[0].PointCount - 2].X;
                double dy = _moveAble.Y - _polygon[0][_polygon[0].PointCount - 2].Y;

                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        public double SegmentAngle
        {
            get
            {
                if (_moveAble == null || _polygon[0].PointCount < 2) return 0.0;

                double dx = _moveAble.X - _polygon[0][_polygon[0].PointCount - 2].X;
                double dy = _moveAble.Y - _polygon[0][_polygon[0].PointCount - 2].Y;

                return Math.Atan2(dy, dx) * 180.0 / Math.PI;
            }
        }

        public double Area
        {
            get
            {
                //return _polygon[0].Area;

                Ring ring = new Ring();
                for (int i = 0; i < _polygon[0].PointCount; i++)
                {
                    if (!_dynamic && _polygon[0][i] == _moveAble) continue;
                    ring.AddPoint(_polygon[0][i]);
                }
                return ring.Area;
            }
        }

        public int PointCount
        {
            get
            {
                return (_moveAble != null) ? _polygon[0].PointCount - 1 : _polygon[0].PointCount;
            }
        }

        #region IGraphicElement Member

        public void Draw(IDisplay display)
        {
            if (_lineSymbol != null && _polygon[0].PointCount > 1)
            {
                Polyline line = new Polyline();
                line.AddPath(_polygon[0]);
                display.Draw(_lineSymbol, line);
            }
            if (_pointSymbol != null)
            {
                for (int i = 0; i < _polygon[0].PointCount - ((_moveAble == null) ? 0 : 1); i++)
                {
                    display.Draw(_pointSymbol, _polygon[0][i]);
                }
            }
            if (_fillSymbol != null && _polygon[0].PointCount > 2 && ShowArea)
                display.Draw(_fillSymbol, _polygon);

        }

        #endregion
    }

    [RegisterPlugIn("0728E12C-AC12-4264-9B47-ECE6BB0CFFA9")]
    public class TOCCommand : ITool
    {
        private IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.TOC", "TOC"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return (new Buttons().imageList1.Images[21]); }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null || !(_doc.Application is IMapApplication)) return;

            foreach (IDockableWindow win in ((IMapApplication)_doc.Application).DockableWindows)
            {
                if (win.Name == "TOC")
                    ((IMapApplication)_doc.Application).ShowDockableWindow(win);
            }
        }

        #endregion
    }

    [RegisterPlugIn("F4F7F60D-B560-4233-96F7-89012FD856A8")]
    public class RefreshMap : ITool, IShortCut
    {
        IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.RefreshMap", "Refresh Map"); }
        }

        public bool Enabled
        {
            get
            {
                return (_doc != null && _doc.FocusMap != null);
            }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.Refresh; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc != null && _doc.Application is IMapApplication)
                ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get { return Keys.F5; }
        }

        public string ShortcutKeyDisplayString
        {
            get { return "F5"; }
        }

        #endregion
    }

    [RegisterPlugIn("7219766E-55AB-4f64-B65E-C2DBC70E5786")]
    public class About : ITool, IExTool
    {
        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.About", "About..."); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.help; }
        }

        public void OnCreate(object hook)
        {

        }

        public void OnEvent(object MapEvent)
        {
            gView.Framework.system.UI.AboutBox dlg = new gView.Framework.system.UI.AboutBox();
            dlg.ShowDialog();
        }

        #endregion
    }

    [RegisterPlugIn("6782B011-83C8-420B-9158-9267EB9C70D0")]
    public class PdfDocs : ITool, IExTool, IToolMenu
    {
        private List<ITool> _docTools = null;

        #region ITool Member

        public string Name
        {
            get { return "PDF Dokumentations"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (_docTools != null) return;

            _docTools = new List<ITool>();
            try
            {
                foreach (FileInfo fi in (new DirectoryInfo(SystemVariables.ApplicationDirectory + @"\doc").GetFiles("*.pdf")))
                {
                    _docTools.Add(new PdfDocTool(fi));
                }
            }
            catch { }
        }

        public void OnEvent(object MapEvent)
        {

        }

        #endregion

        #region IToolMenu Member

        public List<ITool> DropDownTools
        {
            get { return _docTools; }
        }

        public ITool SelectedTool
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        #endregion

        #region HelperClasses
        private class PdfDocTool : ITool, IExTool
        {
            private FileInfo _fi;
            private string _name;

            public PdfDocTool(FileInfo fi)
            {
                _fi = fi;
                _name = _fi.Name.Substring(0, _fi.Name.Length - _fi.Extension.Length);
            }

            #region ITool Member

            public string Name
            {
                get { return _name; }
            }

            public bool Enabled
            {
                get { return true; }
            }

            public string ToolTip
            {
                get { return String.Empty; }
            }

            public ToolType toolType
            {
                get { return ToolType.command; }
            }

            public object Image
            {
                get { return gView.Plugins.Tools.Properties.Resources.pdf; }
            }

            public void OnCreate(object hook)
            {

            }

            public void OnEvent(object MapEvent)
            {
                try
                {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = _fi.FullName;
                    p.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            #endregion
        }
        #endregion
    }


    [RegisterPlugIn("8874DF04-1B5D-4c22-9913-D1F45B9DC958")]
    public class ShowLegend : ITool
    {
        private IMapDocument _doc = null;
        FormLegend _dlg = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.ShowLegend", "Legend Window..."); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = hook as IMapDocument;

                _doc.AfterSetFocusMap += new AfterSetFocusMapEvent(_doc_AfterSetFocusMap);
                _doc.LayerAdded += new LayerAddedEvent(_doc_LayerAdded);
                _doc.LayerRemoved += new LayerRemovedEvent(_doc_LayerRemoved);
                _doc.MapAdded += new MapAddedEvent(_doc_MapAdded);
                _doc.MapDeleted += new MapDeletedEvent(_doc_MapDeleted);
                _doc.MapScaleChanged += new MapScaleChangedEvent(_doc_MapScaleChanged);
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null) return;

            if (_dlg == null)
            {
                _dlg = new FormLegend(_doc);
                _dlg.FormClosing += new FormClosingEventHandler(dlg_FormClosing);
            }
            _dlg.Show();
        }

        #endregion

        private void RefreshLegend()
        {
            if (_dlg == null) return;
            _dlg.RefreshLegend();
        }
        void dlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dlg = null;
        }

        void _doc_MapScaleChanged(IDisplay sender)
        {
            RefreshLegend();
        }

        void _doc_MapDeleted(IMap map)
        {
            RefreshLegend();
        }

        void _doc_MapAdded(IMap map)
        {
            RefreshLegend();
        }

        void _doc_LayerRemoved(IMap sender, ILayer layer)
        {
            RefreshLegend();
        }

        void _doc_LayerAdded(IMap sender, ILayer layer)
        {
            RefreshLegend();
        }

        void _doc_AfterSetFocusMap(IMap map)
        {
            RefreshLegend();
        }
    }

    [RegisterPlugIn("61301C1E-BC8E-4081-A8BB-65BCC13C89EC")]
    public class OverViewMap : ITool
    {
        private IMapDocument _doc;
        private FormOverviewMap _dlg = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.OverViewMap", "Overview Map Window"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = hook as IMapDocument;
                _doc.MapScaleChanged += new MapScaleChangedEvent(_doc_MapScaleChanged);
            }
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null) return;

            if (_dlg == null)
            {
                _dlg = new FormOverviewMap(_doc);
                _dlg.FormClosing += new FormClosingEventHandler(dlg_FormClosing);
            }
            _dlg.Show();
        }

        #endregion

        void dlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dlg = null;
        }

        void _doc_MapScaleChanged(IDisplay sender)
        {
            if (_dlg != null)
                _dlg.DrawMapExtent(sender);
        }
    }

    [RegisterPlugIn("F1B1602A-DD53-40a2-A504-61DC47A7B261")]
    public class PerformanceMonitor : ITool, IToolWindow
    {
        private IMapDocument _doc;
        private FormPerformanceMonitor _dlg = null;

        #region ITool Member

        public string Name
        {
            get { return "Performance Monitor"; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return String.Empty; }
        }

        public ToolType toolType
        {
            get { return ToolType.click; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.time; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
            {
                _doc = (IMapDocument)hook;
                if (_dlg == null)
                    _dlg = new FormPerformanceMonitor(_doc);
            }
        }

        public void OnEvent(object MapEvent)
        {

        }

        #endregion

        #region IToolWindow Member

        public IDockableWindow ToolWindow
        {
            get { return _dlg; }
        }

        #endregion
    }

    [RegisterPlugIn("2AC4447E-ACF3-453D-BB2E-72ECF0C8506E")]
    public class XY : ITool
    {
        IMapDocument _doc = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.XY", "Zoom To Coordinate"); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string ToolTip
        {
            get { return LocalizedResources.GetResString("Tools.XY", "Zoom To Coordinate"); }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.xy; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = (IMapDocument)hook;
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc == null ||
                _doc.FocusMap == null ||
                _doc.FocusMap.Display == null) return;

            FormXY dlg = new FormXY(_doc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                IPoint p = dlg.GetPoint(_doc.FocusMap.Display.SpatialReference);

                _doc.FocusMap.Display.ZoomTo(
                    new Envelope(p.X - 1, p.Y - 1, p.X + 1, p.Y + 1));
                _doc.FocusMap.Display.mapScale = 1000;

                if (_doc.Application is IMapApplication)
                {
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.All);
                }
            }
        }

        #endregion
    }
}
