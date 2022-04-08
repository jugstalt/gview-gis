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
