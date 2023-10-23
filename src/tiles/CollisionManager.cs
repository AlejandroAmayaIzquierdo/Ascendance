
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using nx.entity;
using nx.entity.Objects;
using SharpMath2;
using TiledSharp;

namespace nx.tile;

public enum CollisionType
{
    GROUNDED,
    WALL,
    HEAD,
    NONE
}

public class CollisionManager
{
    private readonly List<CollisionObject> collisionObjects;
    public CollisionManager(TmxObjectGroup collisionGroup)
    {
        collisionObjects = new List<CollisionObject>();
        foreach (var o in collisionGroup.Objects)
        {
            Collection<TmxObjectPoint> tmxObjectPoints = o.Points;

            if (tmxObjectPoints == null)
                continue;

            Vector2[] vertices = tmxObjectPoints.Select(p => new Vector2((float)p.X * Engine.scale, (float)p.Y * Engine.scale)).ToArray();


            var typeObject = o.Properties.FirstOrDefault(e => e.Key == "type").Value ?? "";
            CollisionObject collisionObject = (CollisionObject)CollisionObject.CreateCollisionObject(typeObject + "CollisionObject", new Vector2((float)o.X, (float)o.Y), new Polygon2(vertices));

            collisionObjects.Add(collisionObject);
        }
    }

    public List<CollisionObject> CheckCollision(Entity entity, Vector2 nexMovement)
    {
        var entityShape = new Polygon2(new[] {
        new Vector2(0, 0),
        new Vector2(entity.collisionBounds.Width, 0),
        new Vector2(0, entity.collisionBounds.Height),
        new Vector2(entity.collisionBounds.Width, entity.collisionBounds.Height)
    });
        List<CollisionObject> collisionPolygonsToReturn = new List<CollisionObject>();

        foreach (var collisionPolygon in collisionObjects)
        {
            bool intersects = Polygon2.Intersects(
                entityShape,
                collisionPolygon.shape,
                nexMovement,
                collisionPolygon.position * Engine.scale,
                false
            );

            if (intersects)
                collisionPolygonsToReturn.Add(collisionPolygon);
        }

        return collisionPolygonsToReturn;
    }

    public static float CalculateAngleBetweenEntityAndPolygon(Entity entity, CollisionObject collisionPolygon)
    {
        // Calculate the vector from the entity to the center of the collision polygon
        Vector2 vectorToCenter = entity.position - (collisionPolygon.position * Engine.scale);

        // Calculate the angle in radians using Math.Atan2
        float angleRadians = (float)Math.Atan2(vectorToCenter.Y, vectorToCenter.X);

        // Convert the angle from radians to degrees
        float angleDegrees = MathHelper.ToDegrees(angleRadians);

        // Ensure the angle is within the range [0, 360]
        if (angleDegrees < 0)
        {
            angleDegrees += 360;
        }

        return angleDegrees;
    }

}