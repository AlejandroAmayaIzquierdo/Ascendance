
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using nx.entity;
using SharpMath2;
using TiledSharp;

namespace nx.tile;

public enum Collision
{
    GROUNDED,
    WALL,
    HEAD,
    NONE
}

public class CollisionObject
{
    public CollisionObject(Vector2 position, Polygon2 shape)
    {
        this.position = position;
        this.shape = shape;
    }
    public Vector2 position;
    public Polygon2 shape;
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


            collisionObjects.Add(new CollisionObject(new Vector2((float)o.X, (float)o.Y), new Polygon2(vertices)));

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


            //Debug.WriteLine(entity.position + " " + (collisionPolygon.position * Engine.scale));

            if (intersects)
            {
                /*
                //Debug.WriteLine(CalculateAngleBetweenEntityAndPolygon(entity, collisionPolygon));
                Vector2 centerPolygon = collisionPolygon.shape.Center * Engine.scale;
                //Vector2 LeftCenterPolygon = new Vector2(centerPolygon.X - (collisionPolygon.shape.LongestAxisLength / 2), collisionPolygon.shape.Center.Y);
                float side2Length = (float)Math.Sqrt(Math.Pow(centerPolygon.X, 2) + Math.Pow(centerPolygon.Y, 2));
                Vector2 rightTrianglePoint = new Vector2(centerPolygon.X, centerPolygon.Y + side2Length);
                float minDistance = float.MaxValue;
                Line2 minDistanceLine = null;

                foreach (Line2 line in collisionPolygon.shape.Lines)
                {
                    float distance = Line2.Distance(line, collisionPolygon.position * Engine.scale, entity.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minDistanceLine = line;
                    }
                }
                Debug.WriteLine(minDistanceLine);
                */


                collisionPolygonsToReturn.Add(collisionPolygon);
                // You may perform additional collision handling here.
            }
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