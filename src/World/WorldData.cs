using System.Collections.Generic;
using System.Linq;

namespace nx.world;

public class WorldData(string name, string mapUri, string tileSetUri)
{
    public const string MONOCHROME_SHEET_PATH = "assets/textures/monochrome_sheet";
    public readonly string name = name;
    public readonly string mapUri = mapUri;
    public readonly string tileSetUri = tileSetUri;
    private static readonly Dictionary<int, WorldData> data = new()
    {
        [1] = new WorldData(
            "start",
            "Content/assets/levels/startLevel/level.tmx",
            MONOCHROME_SHEET_PATH
        ),
        [2] = new WorldData(
            "level_1",
            "Content/assets/levels/level_1/level.tmx",
            MONOCHROME_SHEET_PATH
        ),
    };

    public static WorldData GetByName(string name)
    {
        return data.First(pair => pair.Value.name == name).Value;
    }

    public static WorldData? GetWorld(int worldID)
    {
        if (data.ContainsKey(worldID))
            return data[worldID];
        return null;
    }
}
