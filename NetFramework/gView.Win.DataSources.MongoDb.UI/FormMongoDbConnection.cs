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
