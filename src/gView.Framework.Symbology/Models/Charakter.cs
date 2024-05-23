namespace gView.Framework.Symbology.Models;

public class Charakter
{
    public byte Value { get; set; }

    public override string ToString()
    {
        return $"Char {Value}";
    }
}
