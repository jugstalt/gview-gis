using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Carto;

namespace gView.Plugins.MapTools.Dialogs
{
    internal partial class FormPrintPreview : UserControl, IControl
    {
        public FormPrintPreview()
        {
            InitializeComponent();
        }

        public PrintDocument Document
        {
            get
            {
                return printPreviewControl1.Document;
            }
            set
            {
                printPreviewControl1.Document = value;
                if (value != null)
                {
                    numResolution.Value = Math.Max(
                        value.DefaultPageSettings.PrinterResolution.X,
                        value.DefaultPageSettings.PrinterResolution.Y);
                }
            }
        }

        private void btnPageSetup_Click(object sender, EventArgs e)
        {
            pageSetupDialog1.Document = Document;

            if (pageSetupDialog1.ShowDialog() == DialogResult.OK)
            {
                Document.DefaultPageSettings.PrinterResolution.X =
                    Document.DefaultPageSettings.PrinterResolution.Y = (int)numResolution.Value;

                this.Controls.Remove(printPreviewControl1);
                printPreviewControl1 = new PrintPreviewControl();
                printPreviewControl1.Dock = DockStyle.Fill;
                this.Controls.Add(printPreviewControl1);
                printPreviewControl1.BringToFront();
                printPreviewControl1.Document = pageSetupDialog1.Document;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            printDialog1.Document = Document;
            printDialog1.ShowDialog();
        }

        private void btnPrint_Click_1(object sender, EventArgs e)
        {
            if (Document == null) return;
            Document.DefaultPageSettings.PrinterResolution.X =
                    Document.DefaultPageSettings.PrinterResolution.Y = (int)numResolution.Value;

            Document.Print();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            float scale = 0;
            //MessageBox.Show(printPreviewControl1.Zoom.ToString());
            if (float.TryParse(comboBox1.SelectedItem.ToString(), out scale))
            {
                printPreviewControl1.Zoom = scale / 100;
            }
            else
            {
                printPreviewControl1.AutoZoom = true;
            }
        }

        private void FormPrintPreview_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void numResolution_ValueChanged(object sender, EventArgs e)
        {
            if (Document == null) return;

            Document.DefaultPageSettings.PrinterResolution.X =
                Document.DefaultPageSettings.PrinterResolution.Y = (int)numResolution.Value;

        }

        #region IControl Member

        PrinterPage _page = null;
        public void OnShowControl(object hook)
        {
            UnloadControl();
            if (hook is IMapDocument)
            {
                IMapDocument doc = (IMapDocument)hook;

                this.Document = PrinterPage.Document;

                PrinterMap map = new PrinterMap(doc.FocusMap as Map);

                _page = new PrinterPage(map, doc.FocusMap.Display.Envelope);
                FormPrintPreview_Load(this, new EventArgs());
            }
        }

        public void UnloadControl()
        {
            if (_page != null)
            {
                _page.Release();
                _page = null;
            }
        }
        #endregion
    }
}