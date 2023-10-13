using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.entity;
using TiledSharp;

namespace nx.tile;

public class TileManager
{
    private int maxWorldCol = 20;
    private int maxWorldRow = 20;
    private int worldWidth;
    private int worldHeigth;
    private SpriteBatch spriteBatch;
    TmxMap map;
    Texture2D tileset;
    int tilesetTilesWide;
    int tileWidth;
    int tileHeight;

    private Player playerRef;

    private int ViewportCenterY;

    public TileManager(SpriteBatch _spriteBatch, TmxMap _map, Texture2D _tileset, int _tilesetTilesWide, int _tileWidth, int _tileHeight)

    {
        spriteBatch = _spriteBatch;
        map = _map;
        tileset = _tileset;
        tilesetTilesWide = _tilesetTilesWide;
        tileWidth = _tileWidth;
        tileHeight = _tileHeight;
        playerRef = Player.GetInstance();

        ViewportCenterY = Engine.viewport.Y / 2;

        worldWidth = Engine.TILE_SIZE * maxWorldCol;
        worldHeigth = Engine.TILE_SIZE * maxWorldRow;
    }

    public void Draw()
    {
        /*
        var yOffset = ViewportCenterY - playerRef.position.Y;

        int worldCol = 0;
        int worldRow = 0;


        while (worldCol < maxWorldCol && worldRow < maxWorldRow)
        {
            int worldX = worldCol * Engine.TILE_SIZE;
            int worldY = worldRow * Engine.TILE_SIZE;

            for (int i = 0; i < map.TileLayers[0].Tiles.Count; i++)
            {
                //int gid = map.TileLayers[i].Tiles[j].Gid;
            }
        }
        */


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


                if (y < 0 - Engine.TILE_SIZE || y > Engine.viewport.Y + Engine.TILE_SIZE)
                {
                    continue;
                }

                spriteBatch.Draw(
                    tileset,
                    new Rectangle(
                        worlSpaceX,
                        (int)(Engine.SCREEN_CENTER_Y - playerRef.position.Y + worlSpaceY),
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