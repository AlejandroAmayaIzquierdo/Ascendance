using Microsoft.Xna.Framework;
using nx.tile;

namespace nx.entity;

public interface ICollider
{
    Rectangle CollisionsBounds { get; }
    public void HandleCollision(CollisionInfo[] other);

    public void DrawCollision(GameTime gameTime) { }
}
