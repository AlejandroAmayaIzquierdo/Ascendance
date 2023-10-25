

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

    private Camera2D mainCamera;

    public World(Game game, WorldData worldData)
    {
        TmxMap map = new TmxMap(worldData.mapUri);

        var tileset = game.Content.Load<Texture2D>(worldData.tileSetUri);

        var tileWidth = map.Tilesets[0].TileWidth;
        var tileHeight = map.Tilesets[0].TileHeight;
        var TileSetTilesWide = tileset.Width / tileWidth;
        mainCamera = new Camera2D(Vector2.Zero);
        Level = new TileManager(Engine.SpriteBatch, map, tileset, TileSetTilesWide, tileWidth, tileHeight, mainCamera);

        collisionManager = new CollisionManager(map.ObjectGroups["Platforms"]);

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

    public Camera2D getMainCamera() { return mainCamera; }

    public void Update(GameTime gameTime)
    {
        player.Update(gameTime);
        mainCamera.setPosition(player.position);
    }

    public void Draw(GameTime gameTime)
    {
        Level.Draw();
        player.Draw(gameTime);
    }
}