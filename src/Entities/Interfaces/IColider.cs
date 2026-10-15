using Microsoft.Xna.Framework;

namespace nx.entity;

public interface ICollider
{
    Rectangle CollisionsBounds { get; }

    public void DrawCollision(GameTime gameTime) { }
}
