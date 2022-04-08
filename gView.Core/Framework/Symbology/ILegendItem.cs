namespace gView.Framework.Symbology
{
    public interface ILegendItem
    {
        string LegendLabel { get; set; }
        bool ShowInTOC { get; set; }
        int IconHeight { get; }
    }
}
