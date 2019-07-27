using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.CosmoDb.UI
{
    public partial class FormCosmoDbConnection : Form
    {
        public FormCosmoDbConnection()
        {
            InitializeComponent();
        }

        public string ConnectionString =>
            $"AccountEndpoint={txtAccountEndpoint.Text};AccountKey={txtAccountKey.Text};Database={txtDatabase.Text}";
    }
}
