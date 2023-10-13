

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TiledSharp;

namespace nx.world;

public enum Worlds
{
    START_LEVEL
}

public class WorldData
{
    public readonly string name;
    public readonly string mapUri;
    public readonly string tileSetUri;
    public readonly Vector2 spawn;



    public WorldData(string name, string mapUri, string tileSetUri, Vector2 spawn)
    {
        this.name = name;
        this.mapUri = mapUri;
        this.tileSetUri = tileSetUri;
        this.spawn = spawn;
    }
    private static readonly Dictionary<Worlds, WorldData> data = new Dictionary<Worlds, WorldData>
    {
        [Worlds.START_LEVEL] = new WorldData(
                "start",
                "Content/map.tmx",
                "Content/sheet.png",
                new Vector2(0, 416)
            )
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
