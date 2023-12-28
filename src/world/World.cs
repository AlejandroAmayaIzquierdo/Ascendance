

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
    private ITileDrawer Level;
    public CollisionManager collisionManager;
    //private List<Entity> entities;
    private Player player;
    public Vector2 spawn;

    public static int worldHeight;
    public static int worldWidth;

    public World(Game game, Camera2D mainCamera, WorldData worldData = null, WORLD_GENERATION generationType = WORLD_GENERATION.WAVE_FUNCTION_COLLAPSE)
    {
        if (worldData != null)
        {
            TmxMap map = new TmxMap(worldData.mapUri);

            var tileset = game.Content.Load<Texture2D>(worldData.tileSetUri);

            var tileWidth = map.Tilesets[0].TileWidth;
            var tileHeight = map.Tilesets[0].TileHeight;
            var TileSetTilesWide = tileset.Width / tileWidth;
            Level = new TileManager(Engine.SpriteBatch, map, tileset, TileSetTilesWide, tileWidth, tileHeight, mainCamera);

            collisionManager = new CollisionManager(map.ObjectGroups["Platforms"]);

            worldHeight = map.Height * Engine.TILE_SIZE;

            worldWidth = map.Width * Engine.TILE_SIZE;

            LoadEntities(game, map);
        }
        else
        {
            Level = null;
        }
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