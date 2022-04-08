namespace gView.Framework.Symbology
{
    public interface IPenWidth
    {
        float PenWidth { get; set; }
        float MaxPenWidth { get; set; }
        float MinPenWidth { get; set; }
        DrawingUnit PenWidthUnit { get; set; }
    }
}
