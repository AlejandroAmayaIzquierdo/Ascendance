using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.world;
using TiledSharp;

namespace nx.tile;

public class TileManager : ITileDrawer
{
    private SpriteBatch spriteBatch;
    private TmxMap map;
    private Texture2D tileSet;
    private int tileSetTilesWide;
    private int tileWidth;
    private int tileHeight;
    private Camera2D mainCamera;

    public TileManager(SpriteBatch _spriteBatch, TmxMap _map, Texture2D _tileset, int _tileSetTilesWide, int _tileWidth, int _tileHeight, Camera2D camera)
    {
        map = _map;
        tileSet = _tileset;
        mainCamera = camera;
        tileWidth = _tileWidth;
        tileHeight = _tileHeight;
        spriteBatch = _spriteBatch;
        tileSetTilesWide = _tileSetTilesWide;

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

                int column = tileFrame % tileSetTilesWide;
                int row = (int)Math.Floor(tileFrame / (double)tileSetTilesWide);

                float y = (float)Math.Floor(j / (double)map.Width) * map.TileHeight;

                int worldSpaceX = j % map.Width * Engine.TILE_SIZE;
                int worldSpaceY = (int)Math.Floor(j / (double)map.Width) * Engine.TILE_SIZE;

                if (y < 0 - Engine.TILE_SIZE || y > Engine.viewport.Y + Engine.TILE_SIZE || worldSpaceX + Engine.TILE_SIZE < 0 || worldSpaceX > Engine.viewport.X)
                    continue;

                Rectangle tileSetRec = new(tileWidth * column, tileHeight * row, tileWidth, tileHeight);
                Rectangle screenRect = new(
                    (int)(Engine.SCREEN_CENTER_X - mainCamera.position.X + worldSpaceX - Engine.FULSCREEN_OFFSET_X),
                    (int)(Engine.SCREEN_CENTER_Y - mainCamera.position.Y + worldSpaceY),
                    Engine.TILE_SIZE,
                    Engine.TILE_SIZE
                );


                spriteBatch.Draw(tileSet, screenRect, tileSetRec, Color.White);

            }
        }
    }
}