using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gView.Plugins.MapTools.Dialogs
{
    public partial class FormPublishMap : Form
    {
        public FormPublishMap()
        {
            InitializeComponent();

            cmbServerVersion.SelectedIndex = 0;
        }

        public string Server { get { return txtServer.Text; } set { txtServer.Text = value; } }
        public int Port { get { return (int)numPort.Value; } set { numPort.Value = value; } }

        public string Username { get { return txtUserName.Text; } set { txtUserName.Text = value; } }
        public string Password { get { return txtPassword.Text; } set { txtPassword.Text = value; } }

        public string ServiceName { get { return txtServiceName.Text; } set { txtServiceName.Text = value; } }

        public ServerVersion Version
        {
            get
            {
                switch(cmbServerVersion.SelectedItem?.ToString())
                {
                    case "4.x":
                        return ServerVersion.gViewServer4;
                    case "5.x":
                        return ServerVersion.gViewServer5;
                }

                return ServerVersion.Unknown;
            }
        }

        #region Enums

        public enum ServerVersion
        {
            Unknown,
            gViewServer4,
            gViewServer5
        }

        #endregion
    }
}
