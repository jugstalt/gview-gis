using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonPlayground;

internal class MapServerSettingsPlayground
{
    static public void Do()
    {
        var settingsJson = """{"status":0,"accessrules":[{"username":"sys_user_1","servicetypes":["map","query","_all"]}],"refreshticks":0}""";

        var settings = JsonSerializer.Deserialize<gView.Server.AppCode.MapServiceSettings>(settingsJson,
            new JsonSerializerOptions()
            {
                //Converters = { new MapServiceAccessConvertFactory() }
            });

        var serializedSettingsJson = JsonSerializer.Serialize(settings);

        Console.WriteLine(settingsJson);
        Console.WriteLine(serializedSettingsJson);
        Console.WriteLine($"Ident: {serializedSettingsJson.Equals(settingsJson)}");
        Console.WriteLine("---------------------------------------------------------------------------");

    }
}
