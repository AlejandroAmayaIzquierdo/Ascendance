
using Microsoft.Xna.Framework;
using nx.entity;

namespace nx.world;

public class Camera2D
{

    public Vector2 position;
    public CameraBehavior behavior;

    public Camera2D(Vector2 pos)
    {
        this.position = pos;
        behavior = CameraBehavior.FOLLOW;
    }

    public Camera2D move(float x, float y)
    {
        if (behavior == CameraBehavior.FOLLOW)
            position += new Vector2(x, y);

        return this;
    }

    public Camera2D setPosition(Vector2 newPos)
    {
        if (behavior == CameraBehavior.FOLLOW)
        {

            if (newPos.Y <= World.worldHeigth - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE)
                position = newPos;
            else
                position.Y = World.worldHeigth - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE;

        }

        return this;
    }

    public Camera2D setPosition(float x, float y)
    {
        if (behavior == CameraBehavior.FOLLOW)
            position = new Vector2(x, y);

        return this;
    }


}

public enum CameraBehavior
{
    IDLE,
    FOLLOW
}