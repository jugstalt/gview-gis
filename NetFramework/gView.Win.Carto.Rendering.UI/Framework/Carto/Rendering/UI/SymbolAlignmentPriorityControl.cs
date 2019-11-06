using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using gView.Framework.Symbology;

namespace gView.Win.Carto.Rendering.UI.Framework.Carto.Rendering.UI
{
    public partial class SymbolAlignmentPriorityControl : UserControl
    {
        public SymbolAlignmentPriorityControl()
        {
            InitializeComponent();
        }

        #region Properties 

        private TextSymbolAlignment _primaryAlignment;
        public TextSymbolAlignment PrimarySymbolAlignment
        {
            get { return _primaryAlignment; }
            set
            {
                foreach(PriorityButton button in panelContent.Controls)
                {
                    if(button.SymbolAlignment==value)
                    {
                        button.Text = "1";
                        button.BackColor = Color.LightPink;
                    } else
                    {
                        button.Text = "0";
                        button.BackColor = Color.White;
                    }
                }

                _primaryAlignment = value;
            }
        }

        #endregion

        #region Methods



        #endregion

        private void SymbolAlignmentPriorityControl_Load(object sender, EventArgs e)
        {
            BuildGUI();
        }

        private void panelContent_Resize(object sender, EventArgs e)
        {
            BuildGUI();
        }

        private void BuildGUI()
        {
            int width = panelContent.Width,
                height = panelContent.Height;

            int buttonWidth = width / 3,
                buttonHeight = height / 3;

            for (var y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    var button = new PriorityButton(GetAlignment(y * 3 + x))
                    {
                        Width = buttonWidth,
                        Height = buttonHeight,
                        Left = x * buttonWidth,
                        Top = y * buttonHeight,
                        Text = "0",
                        BackColor = Color.White
                    };

                    button.Click += Button_Click;

                    panelContent.Controls.Add(button);
                }
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (sender is PriorityButton)
            {
                var button = (PriorityButton)sender;
                if (button.Text == "0")
                {
                    button.Text = (GetMaxPriorty() + 1).ToString();
                    button.BackColor = Color.LightGreen;
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            foreach (Button button in panelContent.Controls)
            {
                button.Text = "0";
                button.BackColor = Color.White;
            }
        }

        #region Helper

        private TextSymbolAlignment GetAlignment(int counter)
        {
            switch(counter % 9)
            {
                case 0:
                    return TextSymbolAlignment.rightAlignOver;
                case 1:
                    return TextSymbolAlignment.Over;
                case 2:
                    return TextSymbolAlignment.leftAlignOver;
                case 3:
                    return TextSymbolAlignment.rightAlignCenter;
                case 4:
                    return TextSymbolAlignment.Center;
                case 5:
                    return TextSymbolAlignment.leftAlignCenter;
                case 6:
                    return TextSymbolAlignment.rightAlignUnder;
                case 7:
                    return TextSymbolAlignment.Under;
                case 8:
                    return TextSymbolAlignment.leftAlignUnder;
            }

            return TextSymbolAlignment.Center;
        }

        private int GetMaxPriorty()
        {
            int max = 0;
            foreach(Button button in panelContent.Controls)
            {
                max = Math.Max(max, int.Parse(button.Text));
            }

            return max;
        }

        #endregion

        #region Control Classes

        private class PriorityButton : Button
        {
            public PriorityButton(TextSymbolAlignment alignment)
            {
                this.SymbolAlignment = alignment;
            }

            public TextSymbolAlignment SymbolAlignment { get; private set; }
        }

        #endregion
    }
}
