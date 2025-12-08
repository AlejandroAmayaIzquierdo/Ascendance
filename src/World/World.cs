using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using nx.entity;
using nx.tile;
using nx.util;
using TiledSharp;

namespace nx.world;

public class World
{
    private readonly Game _game;
    private readonly LevelManager _levelManger;
    private readonly List<Entity> _entities = [];

    public CollisionManager? collisionManager;

    public Camera2D MainCamera;

    public static int worldHeight;
    public static int worldWidth;

    public World(Game game)
    {
        _game = game;
        MainCamera = new Camera2D(Vector2.Zero);

        _levelManger = new LevelManager(game, MainCamera);

        NextLevel();
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
            Entity? newEntity = null;
            switch (entity.Properties["type"])
            {
                case "player":
                    newEntity = (Player)
                        Entity.LoadEntityByName(game, Entities.PLAYER, entityWorldPosition);
                    break;
                case "bot_master":
                    newEntity = (BotMaster)
                        Entity.LoadEntityByName(game, Entities.BOT_MASTER, entityWorldPosition);
                    break;
                default:
                    break;
            }

            if (newEntity is not null)
                _entities.Add(newEntity);
        }
    }

    public void NextLevel()
    {
        _entities.Clear();
        _levelManger.NextLevel();

        if (_levelManger.CurrentLevel is null)
            throw new Exception("Error al cargar el siguiente mapa");

        var map = _levelManger.CurrentLevel.Map;

        LoadEntities(_game, map);

        collisionManager = new CollisionManager(
            map.ObjectGroups["Platforms"],
            [.. _entities.Where(e => e is ICollider).Cast<ICollider>()]
        );

        worldHeight = map.Height * Engine.TILE_SIZE;

        worldWidth = map.Width * Engine.TILE_SIZE;
    }

    public void Update(GameTime gameTime)
    {
        collisionManager?.Update(gameTime);
        var entitiesToUpdate = _entities.ToList();
        foreach (var entity in entitiesToUpdate)
            entity.Update(gameTime);

        if (Player.IsInitialized)
            MainCamera.SetPosition(Player.Instance.Position);
    }

    public void Draw(GameTime gameTime)
    {
        _levelManger.CurrentLevel!.Draw();
        var entitiesToDraw = _entities.ToList();

        foreach (var entity in entitiesToDraw)
            entity.Draw(gameTime);
    }

    public void DrawDebug(GameTime gameTime)
    {
        foreach (var colider in collisionManager?.Coliders ?? [])
            colider.DrawCollision(gameTime);

        foreach (var collisionObject in collisionManager?.CollisionObjects ?? [])
        {
            var currentCameraPosition = MainCamera.Position;

            Vector2 objectPosition = new()
            {
                X = collisionObject.position.X * Engine.scale,
                Y =
                    (collisionObject.position.Y * Engine.scale)
                    - currentCameraPosition.Y
                    + Engine.SCREEN_CENTER_Y,
            };

            foreach (var line in collisionObject.shape.Lines)
            {
                var startPos = new Vector2(
                    objectPosition.X + line.MinX,
                    objectPosition.Y + line.MinY
                );
                var endPos = new Vector2(
                    objectPosition.X + line.MaxX,
                    objectPosition.Y + line.MaxY
                );

                Engine.SpriteBatch.DrawLineBetween(startPos, endPos, 2, Color.Yellow);
            }
        }
    }
}
