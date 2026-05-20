using gView.Cmd.Import.Aprx;

var reader = new AprxReader(@"G:\github\jugstalt\gview-gis\src\gView.Cmd.Import.Lib\allg_nb_sdet.aprx");
var maps = await reader.ReadMapsAsync();
Console.WriteLine($"Maps: {maps.Count}");
foreach (var map in maps) {
    Console.WriteLine($"  Map: {map.Map.Name}, Layers: {map.Layers.Count}");
    foreach (var layer in map.Layers) {
        Console.WriteLine($"    Layer: {layer.GetType().Name} | {layer.Name}");
        if (layer is gView.Cmd.Import.Aprx.Models.CimGroupLayer g) {
            Console.WriteLine($"      Layers(paths): {g.Layers?.Count ?? 0}, LayerDefs: {g.LayerDefinitions?.Count ?? 0}");
            if (g.LayerDefinitions != null) foreach (var c in g.LayerDefinitions) Console.WriteLine($"      Child: {c.GetType().Name} | {c.Name}");
        }
    }
}
