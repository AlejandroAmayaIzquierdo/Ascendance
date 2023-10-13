
using Microsoft.Xna.Framework;

namespace nx.tile;

public class Camera2D
{

    public Vector2 position;

    public Camera2D move(float x, float y)
    {
        position += new Vector2(x, y);

        return this;
    }

    public Camera2D setPosition(float x, float y)
    {
        position = new Vector2(x, y);

        return this;
    }


}