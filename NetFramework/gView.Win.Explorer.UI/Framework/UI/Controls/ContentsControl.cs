using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace gView.Framework.UI.Controls
{
	/// <summary>
	/// Zusammenfassung für ContentsControl.
	/// </summary>
	public class ContentsControl : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ContentsControl()
		{
			// Dieser Aufruf ist für den Windows Form-Designer erforderlich.
			InitializeComponent();
		}

		/// <summary> 
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Komponenten-Designer generierter Code
		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // ContentsControl
            // 
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Name = "ContentsControl";
            this.Size = new System.Drawing.Size(288, 296);
            this.ResumeLayout(false);

		}
		#endregion
	}
}
