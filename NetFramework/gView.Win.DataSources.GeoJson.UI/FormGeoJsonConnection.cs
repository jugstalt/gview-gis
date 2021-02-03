using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Win.DataSources.GeoJson.UI
{
    public partial class FormGeoJsonConnection : Form
    {
        public FormGeoJsonConnection()
        {
            InitializeComponent();
        }

        public string ConnectionString
        {
            get
            {
                return $"target={ txtTarget.Text }";
            }
        }
    }
}
