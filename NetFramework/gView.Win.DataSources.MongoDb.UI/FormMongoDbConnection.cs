using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.DataSources.MongoDb.UI
{
    public partial class FormMongoDbConnection : Form
    {
        public FormMongoDbConnection()
        {
            InitializeComponent();
        }

        public string ConnectionString =>
            $"Connection={txtConnection.Text};Database={txtDatabase.Text}";
    }
}
