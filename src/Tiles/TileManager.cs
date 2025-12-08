using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.world;
using TiledSharp;

namespace nx.tile;

public class TileManager(
    SpriteBatch spriteBatch,
    TmxMap map,
    Texture2D tileset,
    int tileSetTilesWide,
    int tileWidth,
    int tileHeight,
    Camera2D camera
)
{
    private SpriteBatch _spriteBatch = spriteBatch;
    private TmxMap _map = map;

    public TmxMap Map => _map;
    private Texture2D _tileSet = tileset;
    private int _tileSetTilesWide = tileSetTilesWide;
    private int _tileWidth = tileWidth;
    private int _tileHeight = tileHeight;
    private Camera2D _mainCamera = camera;

    public void Draw()
    {
        for (var i = 0; i < _map.TileLayers.Count; i++)
        {
            for (var j = 0; j < _map.TileLayers[i].Tiles.Count; j++)
            {
                int gid = _map.TileLayers[i].Tiles[j].Gid;
                if (gid == 0)
                    continue;

                int tileFrame = gid - 1;

                int column = tileFrame % _tileSetTilesWide;
                int row = (int)Math.Floor(tileFrame / (double)_tileSetTilesWide);

                float y = (float)Math.Floor(j / (double)_map.Width) * _map.TileHeight;

                int worldSpaceX = j % _map.Width * Engine.TILE_SIZE;
                int worldSpaceY = (int)Math.Floor(j / (double)_map.Width) * Engine.TILE_SIZE;

                if (
                    y < 0 - Engine.TILE_SIZE
                    || y > Engine.viewport.Y + Engine.TILE_SIZE
                    || worldSpaceX + Engine.TILE_SIZE < 0
                    || worldSpaceX > Engine.viewport.X
                )
                {
                    continue;
                }

                Rectangle tileSetRec = new(
                    _tileWidth * column,
                    _tileHeight * row,
                    _tileWidth,
                    _tileHeight
                );
                Rectangle screenRect = new(
                    worldSpaceX,
                    (int)(Engine.SCREEN_CENTER_Y - _mainCamera.Position.Y + worldSpaceY),
                    Engine.TILE_SIZE,
                    Engine.TILE_SIZE
                );

                _spriteBatch.Draw(_tileSet, screenRect, tileSetRec, Color.White);
            }
        }
    }
}
