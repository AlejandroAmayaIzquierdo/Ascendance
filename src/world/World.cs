

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.entity;
using nx.tile;
using TiledSharp;

namespace nx.world;

public class World
{
    private TileManager Level;
    public CollisionManager collisionManager;
    //private List<Entity> entities;
    private Player player;
    public Vector2 spawn;

    public static int worldHeight;
    public static int worldWidth;

    public World(Game game, WorldData worldData, Camera2D mainCamera)
    {
        TmxMap map = new TmxMap(worldData.mapUri);

        var tileset = game.Content.Load<Texture2D>(worldData.tileSetUri);

        var tileWidth = map.Tilesets[0].TileWidth;
        var tileHeight = map.Tilesets[0].TileHeight;
        var TileSetTilesWide = tileset.Width / tileWidth;
        Level = new TileManager(Engine.SpriteBatch, map, tileset, TileSetTilesWide, tileWidth, tileHeight, mainCamera);

        collisionManager = new CollisionManager(map.ObjectGroups["Platforms"]);

        Engine.MAX_SCREEN_COL = map.Width;
        Engine.MAX_SCREEN_ROW = map.Height;

        ((Engine)game).loadScreenByResolution(Engine.TILE_SIZE * Engine.MAX_SCREEN_COL, Engine.TILE_SIZE * Engine.MAX_SCREEN_ROW);

        worldHeight = map.Height * Engine.TILE_SIZE;

        worldWidth = map.Width * Engine.TILE_SIZE;

        LoadEntities(game, map);
    }

    private void LoadEntities(Game game, TmxMap map)
    {
        if (map.ObjectGroups["Entities"] == null)
            return;
        foreach (var entity in map.ObjectGroups["Entities"].Objects)
        {
            if (entity.Properties["type"] == null)
                continue;


            switch (entity.Properties["type"])
            {
                case "player":
                    player = (Player)Entity.LoadEntityByName(game, Entities.PLAYER, new Vector2((float)entity.X, (float)entity.Y) * Engine.scale);
                    break;
                default:
                    break;
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        player.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        Level.Draw();
        player.Draw(gameTime);
    }
}