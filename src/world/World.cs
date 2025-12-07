using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    private List<Entity> _entities = [];
    private Player player;
    public Vector2 spawn;

    public static int worldHeight;
    public static int worldWidth;

    private Camera2D mainCamera;

    public World(Game game, WorldData worldData)
    {
        TmxMap map = new(worldData.mapUri);

        var tileset = game.Content.Load<Texture2D>(worldData.tileSetUri);

        var tileWidth = map.Tilesets[0].TileWidth;
        var tileHeight = map.Tilesets[0].TileHeight;
        var TileSetTilesWide = tileset.Width / tileWidth;
        mainCamera = new Camera2D(Vector2.Zero);
        Level = new TileManager(
            Engine.SpriteBatch,
            map,
            tileset,
            TileSetTilesWide,
            tileWidth,
            tileHeight,
            mainCamera
        );

        LoadEntities(game, map);

        collisionManager = new CollisionManager(
            map.ObjectGroups["Platforms"],
            [.. _entities.Where(e => e is IColider).Cast<IColider>(), player]
        );

        worldHeight = map.Height * Engine.TILE_SIZE;

        worldWidth = map.Width * Engine.TILE_SIZE;
    }

    private void LoadEntities(Game game, TmxMap map)
    {
        if (map.ObjectGroups["Entities"] == null)
            return;
        foreach (var entity in map.ObjectGroups["Entities"].Objects)
        {
            if (entity.Properties["type"] == null)
                continue;

            var entityWorldPosition = new Vector2((float)entity.X, (float)entity.Y) * Engine.scale;

            switch (entity.Properties["type"])
            {
                case "player":
                    player = (Player)
                        Entity.LoadEntityByName(game, Entities.PLAYER, entityWorldPosition);
                    break;
                case "bot_master":
                    BotMaster botMaster = (BotMaster)
                        Entity.LoadEntityByName(game, Entities.BOT_MASTER, entityWorldPosition);
                    _entities.Add(botMaster);
                    break;
                default:
                    break;
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        collisionManager.Update(gameTime);
        player.Update(gameTime);
        foreach (var entity in _entities)
            entity.Update(gameTime);
        mainCamera.setPosition(player.position);
    }

    public void Draw(GameTime gameTime)
    {
        Level.Draw();
        foreach (var entity in _entities)
            entity.Draw(gameTime);
        player.Draw(gameTime);
    }
}
