using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

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


            MemoryStream ms=new MemoryStream(
                System.Text.Encoding.Default.GetBytes(html));

            webBrowser1.DocumentStream = ms;
        }
    }
}
