namespace gView.Framework.Core.Data
{
    public interface IGridClass
    {
        GridRenderMethod ImplementsRenderMethods { get; }

        GridColorClass[] ColorClasses { get; set; }

        bool UseHillShade { get; set; }
        double[] HillShadeVector
        {
            get;
            set;
        }

        double MinValue { get; }
        double MaxValue { get; }
        double IgnoreDataValue { get; set; }
        bool UseIgnoreDataValue { get; set; }

        bool RenderRawGridValues { get; set; }
    }
}