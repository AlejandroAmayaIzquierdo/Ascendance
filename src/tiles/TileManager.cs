using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.entity;
using nx.world;
using TiledSharp;

namespace nx.tile;

public class TileManager
{
    private int maxWorldCol = 20;
    private int maxWorldRow = 20;
    private SpriteBatch spriteBatch;
    private TmxMap map;
    private Texture2D tileset;
    private int tilesetTilesWide;
    private int tileWidth;
    private int tileHeight;
    private Camera2D mainCamera;

    public TileManager(SpriteBatch _spriteBatch, TmxMap _map, Texture2D _tileset, int _tilesetTilesWide, int _tileWidth, int _tileHeight, Camera2D camera)

    {
        spriteBatch = _spriteBatch;
        map = _map;
        tileset = _tileset;
        tilesetTilesWide = _tilesetTilesWide;
        tileWidth = _tileWidth;
        tileHeight = _tileHeight;
        mainCamera = camera;
    }

    public void Draw()
    {
        for (var i = 0; i < map.TileLayers.Count; i++)
        {
            for (var j = 0; j < map.TileLayers[i].Tiles.Count; j++)
            {
                int gid = map.TileLayers[i].Tiles[j].Gid;
                if (gid == 0)
                    continue;

                int tileFrame = gid - 1;

                //Crop the texture from the tiles
                int column = tileFrame % tilesetTilesWide;
                int row = (int)Math.Floor(tileFrame / (double)tilesetTilesWide);

                float y = (float)Math.Floor(j / (double)map.Width) * map.TileHeight;
                Rectangle tilesetRec = new(tileWidth * column, tileHeight * row, tileWidth, tileHeight);

                int worlSpaceX = j % map.Width * Engine.TILE_SIZE;
                int worlSpaceY = (int)Math.Floor(j / (double)map.Width) * Engine.TILE_SIZE;


                //int worldHeigth = map.Height * Engine.TILE_SIZE;
                //int screenHeigth = Engine.maxScreenRow * Engine.TILE_SIZE;


                if (y < 0 - Engine.TILE_SIZE || y > Engine.viewport.Y + Engine.TILE_SIZE)
                {
                    continue;
                }

                spriteBatch.Draw(
                    tileset,
                    new Rectangle(
                        worlSpaceX,
                        (int)(Engine.SCREEN_CENTER_Y - mainCamera.position.Y + worlSpaceY),
                        Engine.TILE_SIZE,
                        Engine.TILE_SIZE
                        ),
                    tilesetRec,
                    Color.White
                );
            }
        }
    }
}