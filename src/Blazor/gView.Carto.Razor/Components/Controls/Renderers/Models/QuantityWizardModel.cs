using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;
using gView.Framework.Data.Filters;
using System.Numerics;
using System.Windows.Markup;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;

internal enum StepMethod { FixWidth, FixCount };
internal enum QuantityWizardSymbol { Minimum, Maximum };
internal class QuantityWizardModel
{
    private QuantityWizardModel() { }

    public double MinValue { get; set; }
    public double MaxValue { get; set; }

    public ISymbol? MinValueSymbol { get; set; }
    public ISymbol? MaxValueSymbol { get; set; }

    public ISymbol? this[QuantityWizardSymbol sym]
    {
        get => sym switch
        {
            QuantityWizardSymbol.Minimum => MinValueSymbol,
            QuantityWizardSymbol.Maximum => MaxValueSymbol,
            _ => null
        };

        set
        {
            switch (sym)
            {
                case QuantityWizardSymbol.Minimum:
                    MinValueSymbol = value ?? MinValueSymbol;
                    break;
                case QuantityWizardSymbol.Maximum:
                    MaxValueSymbol = value ?? MaxValueSymbol;
                    break;
            }
        }
    }

    public StepMethod FixStepMethod { get; set; }

    public double FixStepWidth { get; set; }
    public int FixStepCount { get; set; }

    async static public Task<QuantityWizardModel> CreateAsync(
        QuantityRenderer renderer,
        IFeatureLayer featureLayer)
    {
        var wizard = new QuantityWizardModel();
        var featureClass = featureLayer.FeatureClass;

        wizard.MinValueSymbol = RendererFunctions.CreateStandardSymbol(featureLayer.LayerGeometryType);
        wizard.MaxValueSymbol = RendererFunctions.CreateStandardSymbol(featureLayer.LayerGeometryType);

        if (featureClass is not null)
        {
            object minObj = await FunctionFilter.QueryScalar(
                    featureClass,
                    new FunctionFilter("MIN", renderer.ValueField, "field_min"),
                    "field_min");
            object maxObj = await FunctionFilter.QueryScalar(
                featureClass,
                new FunctionFilter("MAX", renderer.ValueField, "field_max"),
                "field_max");

            if (minObj != null)
            {
                wizard.MinValue = Convert.ToDouble(minObj);
            }

            if (maxObj != null)
            {
                wizard.MaxValue = Convert.ToDouble(maxObj);
            }

            wizard.FixStepMethod = StepMethod.FixCount;
            wizard.FixStepCount = 10;
            wizard.FixStepWidth = (double)((int)((wizard.MaxValue - wizard.MinValue) / 10));
        }

        return wizard;
    }
}
