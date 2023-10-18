using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.entity;

namespace nx.animation;


public abstract class AbstractAnimation : IAnimation
{

    internal int CurrentFrame;
    internal int TotalFrames;
    internal int Framerate;
    private float timeAcc = 0;


    public bool Looping = true;
    public bool Running = false;


    protected Entity Parent { get; set; }
    public float Scale = 1f;
    public Vector2 Offset = Vector2.Zero;



    protected SpriteEffects SpriteEffect { get; set; }

    public Vector2 Origin;

    public int StartFrame = 0;
    public int EndFrame
    {
        get => TotalFrames;

        set => TotalFrames = value;
    }

    internal Rectangle SourceRectangle;

    public AbstractAnimation(Entity parent, int totalFrames, int framerate, SpriteEffects spriteEffect = SpriteEffects.None)
    {
        if (framerate < 1)
        {
            throw new Exception("Invalid framerate!");
        }
        Framerate = framerate;
        Parent = parent;
        CurrentFrame = StartFrame;
        TotalFrames = totalFrames;
        SpriteEffect = spriteEffect;
    }

    internal abstract Texture2D GetTexture();

    public void Update(GameTime gameTime)
    {
        if (!Running)
        {
            return;
        }

        int frameDuration = 1000 / Framerate;

        if (timeAcc >= frameDuration)
        {
            CurrentFrame++;
            timeAcc = 0.0f;
        }

        if (CurrentFrame == TotalFrames)
        {
            if (!Looping)
                Stop();
            else
                Init();
        }

        //Debug.WriteLine(timeAcc);

        timeAcc += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public virtual void Draw(Rectangle rectangle)
    {
        //Vector2 pos = Parent == null ? Offset : Parent.position + Offset;
        Engine.SpriteBatch.Draw(GetTexture(), rectangle, GetTexture().Bounds, Color.White, 0, Vector2.Zero, SpriteEffect, 0.0f);

        //Engine.SpriteBatch.Draw(GetTexture(), rectangle, Color.White);
    }


    public int GetCurrentFrame()
    {
        return CurrentFrame;
    }

    public void Init(int? startFrame = null)
    {
        if (startFrame == null)
            CurrentFrame = StartFrame;
        else
            CurrentFrame = (int)startFrame;
        Running = true;
    }

    public void Stop()
    {
        CurrentFrame = TotalFrames - 1;
        Running = false;
    }

    public void Flip(SpriteEffects effect = SpriteEffects.FlipHorizontally)
    {
        SpriteEffect = effect;
    }
    public bool isFlip()
    {
        return SpriteEffect == SpriteEffects.FlipHorizontally;
    }
}