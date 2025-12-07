using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.entity;
using nx.util;

namespace nx.animation;

public class SpriteSheetAnimation : AbstractAnimation
{
    private readonly Dictionary<int, List<Texture2D>> sheets = new();

    public string SpriteSheetName;
    private int _currentAnimation;
    public int CurrentAnimation
    {
        get { return _currentAnimation; }
        private set { SetAnimation(value); }
    }

    public SpriteSheetAnimation(
        Entity parent,
        string name,
        Texture2D texture,
        int tileSizeX = Engine.TILE_SIZE,
        int tileSizeY = Engine.TILE_SIZE,
        int framerate = 0,
        SpriteEffects spriteEffect = SpriteEffects.None
    )
        : base(parent, texture.Width / Engine.TILE_SIZE, framerate, spriteEffect)
    {
        SpriteSheetName = name;
        int rows = texture.Height / tileSizeY;

        for (int i = 0; i < rows; i++)
        {
            sheets.Add(i, AnimationUtil.LoadAnimationsByLine(texture, i, tileSizeX, tileSizeY));
        }
        _currentAnimation = 0;
        Init(0);
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
}
