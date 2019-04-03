using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using gView.Framework.IO;

namespace gView.Framework.UI.Dialogs
{
    public partial class FormMetadata_old : Form
    {
        public FormMetadata_old(IPersistStream stream)
        {
            InitializeComponent();

            persistStreamGrid1.PersistStream = stream;
        }

        public IPersistStream Stream
        {
            get { return persistStreamGrid1.PersistStream; }
        }
    }
}