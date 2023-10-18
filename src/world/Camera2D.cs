
using System.Diagnostics;
using Microsoft.Xna.Framework;
using nx.entity;

namespace nx.world;

public class Camera2D
{

    public Vector2 position;
    public CameraBehavior behavior;

    public Vector2 limits;

    public Camera2D(Vector2 pos)
    {
        position = pos;
        behavior = CameraBehavior.FOLLOW;

        float yLimit = World.worldHeight - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE;
        float xLimit = World.worldWidth - Engine.SCREEN_CENTER_X - Engine.TILE_SIZE;

        limits = new(xLimit, yLimit);
    }
    public Camera2D(float x, float y) : this(new Vector2(x, y)) { }


    public Camera2D move(float x, float y)
    {
        if (behavior != CameraBehavior.FOLLOW)
            return this;


        if (isPastLimitsX(x) && isPastLimitsY(y))
            position += new Vector2(x, y);

        return this;
    }

    public Camera2D setPosition(Vector2 newPos)
    {
        if (behavior == CameraBehavior.FOLLOW)
        {

            float x = isPastLimitsX(newPos.X) ? limits.X : newPos.X;
            float y = isPastLimitsY(newPos.Y) ? limits.Y : newPos.Y;

            position = new Vector2(x, y);

            //Debug.WriteLine("Is pass limit " + isPastLimitsX(newPos.X) + " " + x + " " + newPos.X + " " + limits.X);
        }

        return this;
    }

    public Camera2D setPosition(float x, float y)
    {
        return setPosition(new Vector2(x, y));
    }

    public bool isPastLimitsX(float x)
    {
        if (x < limits.X || x > limits.X) //FIXME minLimit is suppose to be max and max is not set correctly. Now works but maybe in other project will not.
            return true;
        return false;
    }
    public bool isPastLimitsY(float y)
    {
        if (y < 0 || y >= limits.Y)
            return true;
        return false;
    }
}

public enum CameraBehavior
{
    IDLE,
    FOLLOW
}