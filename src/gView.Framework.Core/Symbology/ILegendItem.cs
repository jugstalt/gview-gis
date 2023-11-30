namespace gView.Framework.Core.Symbology
{
    public interface ILegendItem
    {
        string LegendLabel { get; set; }
        bool ShowInTOC { get; set; }
        int IconHeight { get; }
    }
}
