using System;
using Microsoft.Xna.Framework;

namespace nx.world;

public class Camera2D
{
    public Vector2 Position;
    public CameraBehavior behavior;

    public Vector2 minLimits;
    public Vector2 maxLimits;

    public Camera2D(Vector2 pos)
    {
        Position = pos;
        behavior = CameraBehavior.FOLLOW;

        float yLimit = World.worldHeight - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE;
        float xLimit = World.worldWidth - Engine.SCREEN_CENTER_X - Engine.TILE_SIZE;

        maxLimits = new(xLimit, yLimit);
        minLimits = new(0, Engine.SCREEN_CENTER_Y);
    }

    public Camera2D(float x, float y)
        : this(new Vector2(x, y)) { }

    public Camera2D SetPosition(Vector2 newPos)
    {
        if (behavior == CameraBehavior.FOLLOW)
        {
            (bool isPastXLimits, _) = IsPastLimitsX(newPos.X);
            (bool isPastYLimits, float closedYLimit) = IsPastLimitsY(newPos.Y);

            float x = isPastXLimits ? maxLimits.X : newPos.X;
            float y = isPastYLimits ? closedYLimit : newPos.Y;

            Position = new Vector2(x, y);
        }

        return this;
    }

    public Camera2D SetPosition(float x, float y)
    {
        return SetPosition(new Vector2(x, y));
    }

    public (bool isPass, float closedLimit) IsPastLimitsX(float x)
    {
        float clamped = Math.Clamp(x, minLimits.X, maxLimits.X);
        bool isPast = clamped != x;

        return (isPast, clamped);
    }

    public (bool isPass, float closedLimit) IsPastLimitsY(float y)
    {
        float clamped = Math.Clamp(y, minLimits.Y, maxLimits.Y);
        bool isPast = clamped != y;

        return (isPast, clamped);
    }
}

public enum CameraBehavior
{
    IDLE,
    FOLLOW,
}
