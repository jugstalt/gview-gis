namespace gView.Framework.UI.Controls
{
    partial class GridControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridControl));
            this.panelGrid = new System.Windows.Forms.Panel();
            this.btnAddColorClass = new System.Windows.Forms.Button();
            this.btnRemoveColorClass = new System.Windows.Forms.Button();
            this.btnEditColorClass = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnWizard = new System.Windows.Forms.Button();
            this.btnRemoveAll = new System.Windows.Forms.Button();
            this.btnHillShade = new System.Windows.Forms.Button();
            this.chkUseHillShade = new System.Windows.Forms.CheckBox();
            this.symbolsListView1 = new SymbolsListView();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelGrid
            // 
            this.panelGrid.AccessibleDescription = null;
            this.panelGrid.AccessibleName = null;
            resources.ApplyResources(this.panelGrid, "panelGrid");
            this.panelGrid.BackgroundImage = null;
            this.panelGrid.Font = null;
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Paint += new System.Windows.Forms.PaintEventHandler(this.panelGrid_Paint);
            // 
            // btnAddColorClass
            // 
            this.btnAddColorClass.AccessibleDescription = null;
            this.btnAddColorClass.AccessibleName = null;
            resources.ApplyResources(this.btnAddColorClass, "btnAddColorClass");
            this.btnAddColorClass.BackgroundImage = null;
            this.btnAddColorClass.Font = null;
            this.btnAddColorClass.Name = "btnAddColorClass";
            this.btnAddColorClass.UseVisualStyleBackColor = true;
            this.btnAddColorClass.Click += new System.EventHandler(this.btnAddColorClass_Click);
            // 
            // btnRemoveColorClass
            // 
            this.btnRemoveColorClass.AccessibleDescription = null;
            this.btnRemoveColorClass.AccessibleName = null;
            resources.ApplyResources(this.btnRemoveColorClass, "btnRemoveColorClass");
            this.btnRemoveColorClass.BackgroundImage = null;
            this.btnRemoveColorClass.Font = null;
            this.btnRemoveColorClass.Name = "btnRemoveColorClass";
            this.btnRemoveColorClass.UseVisualStyleBackColor = true;
            this.btnRemoveColorClass.Click += new System.EventHandler(this.btnRemoveColorClass_Click);
            // 
            // btnEditColorClass
            // 
            this.btnEditColorClass.AccessibleDescription = null;
            this.btnEditColorClass.AccessibleName = null;
            resources.ApplyResources(this.btnEditColorClass, "btnEditColorClass");
            this.btnEditColorClass.BackgroundImage = null;
            this.btnEditColorClass.Font = null;
            this.btnEditColorClass.Name = "btnEditColorClass";
            this.btnEditColorClass.UseVisualStyleBackColor = true;
            this.btnEditColorClass.Click += new System.EventHandler(this.btnEditColorClass_Click);
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this.btnWizard);
            this.panel1.Controls.Add(this.btnRemoveAll);
            this.panel1.Controls.Add(this.btnHillShade);
            this.panel1.Controls.Add(this.chkUseHillShade);
            this.panel1.Controls.Add(this.btnEditColorClass);
            this.panel1.Controls.Add(this.btnAddColorClass);
            this.panel1.Controls.Add(this.btnRemoveColorClass);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // btnWizard
            // 
            this.btnWizard.AccessibleDescription = null;
            this.btnWizard.AccessibleName = null;
            resources.ApplyResources(this.btnWizard, "btnWizard");
            this.btnWizard.BackgroundImage = null;
            this.btnWizard.Font = null;
            this.btnWizard.Name = "btnWizard";
            this.btnWizard.UseVisualStyleBackColor = true;
            this.btnWizard.Click += new System.EventHandler(this.btnWizard_Click);
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.AccessibleDescription = null;
            this.btnRemoveAll.AccessibleName = null;
            resources.ApplyResources(this.btnRemoveAll, "btnRemoveAll");
            this.btnRemoveAll.BackgroundImage = null;
            this.btnRemoveAll.Font = null;
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.UseVisualStyleBackColor = true;
            this.btnRemoveAll.Click += new System.EventHandler(this.btnRemoveAll_Click);
            // 
            // btnHillShade
            // 
            this.btnHillShade.AccessibleDescription = null;
            this.btnHillShade.AccessibleName = null;
            resources.ApplyResources(this.btnHillShade, "btnHillShade");
            this.btnHillShade.BackgroundImage = null;
            this.btnHillShade.Font = null;
            this.btnHillShade.Name = "btnHillShade";
            this.btnHillShade.UseVisualStyleBackColor = true;
            this.btnHillShade.Click += new System.EventHandler(this.btnHillShade_Click);
            // 
            // chkUseHillShade
            // 
            this.chkUseHillShade.AccessibleDescription = null;
            this.chkUseHillShade.AccessibleName = null;
            resources.ApplyResources(this.chkUseHillShade, "chkUseHillShade");
            this.chkUseHillShade.BackgroundImage = null;
            this.chkUseHillShade.Font = null;
            this.chkUseHillShade.Name = "chkUseHillShade";
            this.chkUseHillShade.UseVisualStyleBackColor = true;
            // 
            // symbolsListView1
            // 
            this.symbolsListView1.AccessibleDescription = null;
            this.symbolsListView1.AccessibleName = null;
            resources.ApplyResources(this.symbolsListView1, "symbolsListView1");
            this.symbolsListView1.BackgroundImage = null;
            this.symbolsListView1.Font = null;
            this.symbolsListView1.LegendText = "";
            this.symbolsListView1.Name = "symbolsListView1";
            this.symbolsListView1.ValueText = "";
            // 
            // GridControl
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.symbolsListView1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelGrid);
            this.Font = null;
            this.Name = "GridControl";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelGrid;
        private SymbolsListView symbolsListView1;
        private System.Windows.Forms.Button btnAddColorClass;
        private System.Windows.Forms.Button btnRemoveColorClass;
        private System.Windows.Forms.Button btnEditColorClass;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnRemoveAll;
        private System.Windows.Forms.Button btnHillShade;
        private System.Windows.Forms.CheckBox chkUseHillShade;
        private System.Windows.Forms.Button btnWizard;
    }
}
