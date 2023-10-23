using System.Diagnostics;
using Microsoft.Xna.Framework;
using SharpMath2;

namespace nx.entity.Objects;


public class StickyCollisionObject : CollisionObject
{
    public StickyCollisionObject(Vector2 position, Polygon2 shape) : base(position, shape) { }

    public override void Execute(Entity entity)
    {
        //TODO Sticky collision
    }
}