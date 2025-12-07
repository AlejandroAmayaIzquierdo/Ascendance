using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TiledSharp;

namespace nx.world;

public enum Worlds
{
    START_LEVEL,
    LEVEL_1,
}

public class WorldData(string name, string mapUri, string tileSetUri)
{
    public const string MONOCHROME_SHEET_PATH = "assets/textures/monochrome_sheet";
    public readonly string name = name;
    public readonly string mapUri = mapUri;
    public readonly string tileSetUri = tileSetUri;
    private static readonly Dictionary<Worlds, WorldData> data = new()
    {
        [Worlds.START_LEVEL] = new WorldData(
            "start",
            "Content/assets/levels/startLevel/level.tmx",
            MONOCHROME_SHEET_PATH
        ),
        [Worlds.LEVEL_1] = new WorldData(
            "level_1",
            "Content/assets/levels/level_1/level.tmx",
            MONOCHROME_SHEET_PATH
        ),
    };

    public static WorldData GetByName(string name)
    {
        return data.First(pair => pair.Value.name == name).Value;
    }

    public static WorldData GetWorld(Worlds world)
    {
        return data[world];
    }
}
