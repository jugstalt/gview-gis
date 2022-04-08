using System.IO;
using System.Windows.Forms;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormCopyrightInformation : Form
    {
        public FormCopyrightInformation(string html)
        {
            InitializeComponent();

            html = @"
<html>
<head>
<style>
    body { font-family:verdana;font-size:8.5pt }
    h1 { font-family:verdana;font-size:12pt }
</style>
</head>
<body>
" + html + "</body></html>";


            MemoryStream ms = new MemoryStream(
                System.Text.Encoding.Default.GetBytes(html));

            webBrowser1.DocumentStream = ms;
        }
    }
}
