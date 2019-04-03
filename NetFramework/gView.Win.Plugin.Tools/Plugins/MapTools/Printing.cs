using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Carto;
using gView.Framework.UI.Events;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using gView.Framework.UI;
using gView.Plugins.MapTools.Dialogs;

namespace gView.Plugins.MapTools
{
    internal class PrinterPage
    {
        static public PrintDocument Document = new PrintDocument();
        private PrinterMap _map = null;
        private IEnvelope _envelope = null;

        public PrinterPage(PrinterMap map,IEnvelope env)
        {
            Release();

            Document.DefaultPageSettings.PrinterResolution.X = 300;
            Document.DefaultPageSettings.PrinterResolution.Y = 300;

            _map = map;
            _envelope = env;
            if (_map != null)
            {
                Document.PrintPage += new PrintPageEventHandler(Document_PrintPage);
                Document.BeginPrint += new PrintEventHandler(Document_BeginPrint);
                Document.EndPrint += new PrintEventHandler(Document_EndPrint);
                Document.QueryPageSettings += new QueryPageSettingsEventHandler(Document_QueryPageSettings);
            }
        }

        public void Release()
        {
            _map = null;
            _envelope = null;
            Document.PrintPage -= new PrintPageEventHandler(Document_PrintPage);
            Document.BeginPrint -= new PrintEventHandler(Document_BeginPrint);
            Document.EndPrint -= new PrintEventHandler(Document_EndPrint);
            Document.QueryPageSettings -= new QueryPageSettingsEventHandler(Document_QueryPageSettings);
        }

        private void Document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (_map == null || _envelope == null) return;

            _map.iWidth = e.MarginBounds.Width;
            _map.iHeight = e.MarginBounds.Height;
            _map.setScale(_map.mapScale,
                _envelope.minx * 0.5 + _envelope.maxx * 0.5,
                _envelope.miny * 0.5 + _envelope.maxy * 0.5);
            _map.GraphicsContext = e.Graphics;
            _map.SetOrigin(e.MarginBounds.X, e.MarginBounds.Y);

            RectangleF rect = e.Graphics.ClipBounds;
            e.Graphics.SetClip(e.MarginBounds);

            _map.RefreshMap(DrawPhase.All, null);

            e.Graphics.SetClip(rect);
            //if(_map._image==null) return;

            //e.Graphics.DrawImage(_map._image, e.MarginBounds, 0, 0, _map.iWidth, _map.iHeight, GraphicsUnit.Pixel);

            using (Pen pen = new Pen(Color.Black, 0))
            {
                e.Graphics.DrawRectangle(pen, e.MarginBounds);
            }
        }

        private void Document_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

        }

        private void Document_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

        }

        private void Document_QueryPageSettings(object sender, System.Drawing.Printing.QueryPageSettingsEventArgs e)
        {

        }
    }

    [gView.Framework.system.RegisterPlugIn("23E7CCE3-E370-418f-8B8F-4F8E08A0A6A7")]
    public class PrintPageSetup : ITool
    {
        #region ITool Members

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.PageSetup", "Page Setup...");
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
                return (new Buttons()).imageListPrint.Images[0];
            }
        }

        public void OnCreate(object hook)
        {
            
        }

        public void OnEvent(object MapEvent)
        {
            PageSetupDialog dlg = new PageSetupDialog();
            dlg.Document = PrinterPage.Document;

            dlg.ShowDialog();
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("968B39BA-F35D-4ffd-B576-913B81E89EEF")]
    public class PrintPreview : ITool, IToolControl
    {
        #region ITool Members

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.PrintPreview", "Print Preview...");
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
                return (new Buttons()).imageListPrint.Images[1];
            }
        }

        public void OnCreate(object hook)
        {
            
        }

        public void OnEvent(object MapEvent)
        {
            FormPrintPreview dlg = new FormPrintPreview();
            //PrintPreviewDialog dlg = new PrintPreviewDialog();
            dlg.Document = PrinterPage.Document;

            PrinterMap map=new PrinterMap((Map)((MapEvent)MapEvent).Map);

            PrinterPage page = new PrinterPage(map,((MapEvent)MapEvent).Map.Display.Envelope);
            //dlg.ShowDialog();
            page.Release();
        }

        #endregion

        #region IToolControl Member

        public object Control
        {
            get { return new FormPrintPreview(); }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("AFDF2582-1B65-4e10-9A36-FEBB470140DA")]
    public class Print : ITool, IShortCut
    {

        #region ITool Members

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Print", "Print");
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
                return (new Buttons()).imageListPrint.Images[2];
            }
        }

        public void OnCreate(object hook)
        {
            
        }

        public void OnEvent(object MapEvent)
        {
            PrintDialog dlg = new PrintDialog();
            dlg.Document = PrinterPage.Document;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PrinterPage.Document.Print();
            }
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.P;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+P";
            }
        }

        #endregion
    }
}
