using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using gView.Framework.Carto;

namespace gView.Framework.Carto.Rendering.UI
{
    public partial class FormCartographicInterpretation : Form
    {
        public FormCartographicInterpretation(LegendGroupCartographicMethod method)
        {
            InitializeComponent();

            switch (method)
            {
                case LegendGroupCartographicMethod.Simple:
                    btnCartoSimple.Checked = true;
                    break;
                case LegendGroupCartographicMethod.LegendOrdering:
                    btnCartoLegendOrdering.Checked = true;
                    break;
                case LegendGroupCartographicMethod.LegendAndSymbolOrdering:
                    btnCartoLegendAndSymbolOrdering.Checked = true;
                    break;
            }
        }

        public LegendGroupCartographicMethod CartographicMethod
        {
            get
            {
                if (btnCartoLegendOrdering.Checked)
                    return LegendGroupCartographicMethod.LegendOrdering;

                if (btnCartoLegendAndSymbolOrdering.Checked)
                    return LegendGroupCartographicMethod.LegendAndSymbolOrdering;

                return LegendGroupCartographicMethod.Simple;
            }
        }

    }
}
