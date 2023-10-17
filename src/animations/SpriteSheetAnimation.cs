using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.entity;

namespace nx.animation;


public class SpriteSheetAnimation : AbstractAnimation
{
    private readonly Dictionary<int, List<Texture2D>> sheets = new();

    private int _currentAnimation;
    public int CurrentAnimation { get { return _currentAnimation; } private set { SetAnimation(value); } }


    public SpriteSheetAnimation(Entity parent, Texture2D texture, int framerate = 0, SpriteEffects spriteEffect = SpriteEffects.None) : base(parent, texture.Width / Engine.TILE_SIZE, framerate, spriteEffect)
    {
        int rows = texture.Height / Engine.originalTileSize;

        for (int i = 0; i < rows; i++)
        {
            sheets.Add(i, LoadAnimationsByLine(texture, i, Engine.originalTileSize));
        }
        _currentAnimation = 2;
    }

    internal override Texture2D GetTexture()
    {
        return sheets[_currentAnimation][CurrentFrame];
    }

    public void SetAnimation(int id)
    {
        if (id < 0 || id > sheets.Count)
            throw new System.Exception("No Animation found");

        _currentAnimation = id;
    }

    public static List<Texture2D> LoadAnimationsByLine(Texture2D tileSet, int line, int tileSize = Engine.TILE_SIZE)
    {
        List<Texture2D> tiles = new();

        int columns = tileSet.Width / tileSize;
        int rows = tileSet.Height / tileSize;
        int totalFrames = columns;

        if (line < 0 || line > rows)
            throw new System.Exception("No Animation found");

        for (int i = 0; i < totalFrames; i++)
        {
            int x = i * tileSize;
            int y = line * tileSize;

            // Create a cropped texture for the current frame
            Texture2D croppedImage = new Texture2D(tileSet.GraphicsDevice, tileSize, tileSize);
            Color[] data = new Color[tileSize * tileSize];
            tileSet.GetData(0, new Rectangle(x, y, tileSize, tileSize), data, 0, tileSize * tileSize);
            croppedImage.SetData(data);

            tiles.Add(croppedImage);
        }

        return tiles;
    }

}