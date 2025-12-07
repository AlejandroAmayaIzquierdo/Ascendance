using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TiledSharp;

namespace nx.world;

public enum Worlds
{
    START_LEVEL,
}

public class WorldData(string name, string mapUri, string tileSetUri)
{
    public readonly string name = name;
    public readonly string mapUri = mapUri;
    public readonly string tileSetUri = tileSetUri;
    private static readonly Dictionary<Worlds, WorldData> data = new Dictionary<Worlds, WorldData>
    {
        [Worlds.START_LEVEL] = new WorldData(
            "start",
            "Content/assets/levels/startLevel/verticalTest.tmx",
            "assets/levels/startLevel/monochrome_sheet"
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
