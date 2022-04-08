using gView.Framework.Symbology;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace gView.Win.Carto.Rendering.UI.Framework.Carto.Rendering.UI
{
    public partial class SymbolAlignmentPriorityControl : UserControl
    {
        public SymbolAlignmentPriorityControl()
        {
            InitializeComponent();
        }

        #region Properties 

        private TextSymbolAlignment _primaryAlignment = TextSymbolAlignment.leftAlignOver;
        public TextSymbolAlignment PrimarySymbolAlignment
        {
            get { return _primaryAlignment; }
            set
            {
                foreach (PriorityButton button in panelContent.Controls)
                {
                    if (button.SymbolAlignment == value)
                    {
                        button.Text = "1";
                        button.BackColor = Color.LightPink;
                    }
                    else
                    {
                        button.Text = "0";
                        button.BackColor = Color.White;
                    }
                }

                _primaryAlignment = value;
            }
        }

        public IEnumerable<TextSymbolAlignment> SecondarySymbolAlignments
        {
            get
            {
                List<TextSymbolAlignment> result = new List<TextSymbolAlignment>();

                for (int p = 1; p <= 9; p++)
                {
                    foreach (PriorityButton button in panelContent.Controls)
                    {
                        if (button.Text == p.ToString())
                        {
                            result.Add(button.SymbolAlignment);
                        }
                    }
                }

                return result.Count > 1 ? result.ToArray() : null;
            }
            set
            {
                this.PrimarySymbolAlignment = _primaryAlignment; // set all to default;

                if (value != null)
                {
                    foreach (var symbolAlignment in value)
                    {
                        foreach (PriorityButton button in panelContent.Controls)
                        {
                            if (button.SymbolAlignment == symbolAlignment && button.SymbolAlignment != _primaryAlignment)
                            {
                                button.Text = (GetMaxPriorty() + 1).ToString();
                                button.BackColor = Color.LightGreen;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods



        #endregion

        public event EventHandler OnPriorityChanged;

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
                    var symbolAlignment = GetAlignment(y * 3 + x);
                    var button = GetPriorityButton(symbolAlignment);

                    if (button == null)
                    {
                        button = new PriorityButton(symbolAlignment)
                        {
                            Text = "0",
                            BackColor = Color.White
                        };

                        button.Click += Button_Click;
                        panelContent.Controls.Add(button);
                    }

                    button.Width = buttonWidth;
                    button.Height = buttonHeight;
                    button.Left = x * buttonWidth;
                    button.Top = y * buttonHeight;
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
                else if (button.Text == "1")
                {
                    // Primary Alignment => do nothing
                }
                else
                {
                    int priority = int.Parse(button.Text);
                    button.Text = "0";
                    button.BackColor = Color.White;

                    foreach (PriorityButton pButton in panelContent.Controls)
                    {
                        if (int.Parse(pButton.Text) > priority)
                        {
                            pButton.Text = (int.Parse(pButton.Text) - 1).ToString();
                        }
                    }
                }
            }

            OnPriorityChanged?.Invoke(this, new EventArgs());
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            foreach (Button button in panelContent.Controls)
            {
                button.Text = "0";
                button.BackColor = Color.White;
            }

            this.PrimarySymbolAlignment = _primaryAlignment;

            OnPriorityChanged?.Invoke(this, new EventArgs());
        }

        #region Helper

        private TextSymbolAlignment GetAlignment(int counter)
        {
            switch (counter % 9)
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
            foreach (Button button in panelContent.Controls)
            {
                max = Math.Max(max, int.Parse(button.Text));
            }

            return max;
        }

        private PriorityButton GetPriorityButton(TextSymbolAlignment symbolAlignment)
        {
            foreach (PriorityButton button in panelContent.Controls)
            {
                if (button.SymbolAlignment == symbolAlignment)
                {
                    return button;
                }
            }

            return null;
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
