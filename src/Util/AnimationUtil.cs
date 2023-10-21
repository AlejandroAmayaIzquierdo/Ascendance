using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nx.util;


public class AnimationUtil
{


    public static List<Texture2D> LoadAnimationsByLine(Texture2D tileSet, int line, int tileSizeX = Engine.TILE_SIZE, int tileSizeY = Engine.TILE_SIZE)
    {
        List<Texture2D> tiles = new();

        int columns = tileSet.Width / tileSizeX;
        int rows = tileSet.Height / tileSizeY;
        int totalFrames = columns;

        if (line < 0 || line > rows)
            throw new System.Exception("No Animation found");

        for (int i = 0; i < totalFrames; i++)
        {
            int x = i * tileSizeX;
            int y = line * tileSizeY;

            // Create a cropped texture for the current frame
            Texture2D croppedImage = new Texture2D(tileSet.GraphicsDevice, tileSizeX, tileSizeY);
            Color[] data = new Color[tileSizeX * tileSizeY];
            tileSet.GetData(0, new Rectangle(x, y, tileSizeX, tileSizeY), data, 0, tileSizeX * tileSizeY);
            croppedImage.SetData(data);

            tiles.Add(croppedImage);
        }

        return tiles;
    }
}