
namespace gView.Framework.Symbology.UI.Controls
{
    partial class SymbolSelectorControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listViewSymbols = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // listViewSymbols
            // 
            this.listViewSymbols.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.listViewSymbols.BackColor = System.Drawing.Color.LightGray;
            this.listViewSymbols.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewSymbols.GridLines = true;
            this.listViewSymbols.HideSelection = false;
            this.listViewSymbols.LargeImageList = this.imageList1;
            this.listViewSymbols.Location = new System.Drawing.Point(0, 0);
            this.listViewSymbols.Margin = new System.Windows.Forms.Padding(10);
            this.listViewSymbols.MultiSelect = false;
            this.listViewSymbols.Name = "listViewSymbols";
            this.listViewSymbols.OwnerDraw = true;
            this.listViewSymbols.Size = new System.Drawing.Size(829, 507);
            this.listViewSymbols.TabIndex = 0;
            this.listViewSymbols.TileSize = new System.Drawing.Size(200, 60);
            this.listViewSymbols.UseCompatibleStateImageBehavior = false;
            this.listViewSymbols.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewSymbols_DrawItem);
            this.listViewSymbols.SelectedIndexChanged += new System.EventHandler(this.listViewSymbols_SelectedIndexChanged);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(100, 150);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // SymbolSelectorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listViewSymbols);
            this.Name = "SymbolSelectorControl";
            this.Size = new System.Drawing.Size(829, 507);
            this.Load += new System.EventHandler(this.SymbolSelectorControl_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewSymbols;
        private System.Windows.Forms.ImageList imageList1;
    }
}
