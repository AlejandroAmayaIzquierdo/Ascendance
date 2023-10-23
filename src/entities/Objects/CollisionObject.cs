using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using SharpMath2;

namespace nx.entity.Objects;

public class CollisionObject : ICollisionObject
{
    public CollisionObject(Vector2 position, Polygon2 shape)
    {
        this.position = position;
        this.shape = shape;
    }

    public Vector2 position;
    public Polygon2 shape;

    public virtual void Execute(Entity entity) { }

    public static ICollisionObject CreateCollisionObject(string objectType, Vector2 position, Polygon2 shape)
    {
        // Assuming objectType is a string obtained from the "type" property
        Type type = Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.Name == objectType);

        if (type != null && typeof(ICollisionObject).IsAssignableFrom(type))
        {
            return (ICollisionObject)Activator.CreateInstance(type, position, shape);
        }
        else
        {
            return null;
        }
    }
}