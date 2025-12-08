using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using nx.entity;
using SharpMath2;
using TiledSharp;

namespace nx.tile;

public enum CollisionType
{
    GROUNDED,
    WALL,
    HEAD,
    NONE,
}

public class CollisionObject(Vector2 position, Polygon2 shape)
{
    public Vector2 position = position;
    public Polygon2 shape = shape;
}

public record CollisionInfo(
    CollisionObject Object,
    Vector2 Normal,
    float Penetration,
    CollisionType Type,
    Line2 ContactLine
);

public class CollisionManager
{
    private readonly List<CollisionObject> collisionObjects = [];

    private readonly List<IColider> _entities = [];

    public List<IColider> Coliders => _entities;
    private TmxObjectGroup _tmxObjectGroup;

    public CollisionManager(TmxObjectGroup collisionGroup, List<IColider> coliders)
    {
        _tmxObjectGroup = collisionGroup;
        foreach (var o in _tmxObjectGroup.Objects)
        {
            Collection<TmxObjectPoint> tmxObjectPoints = o.Points;

            if (tmxObjectPoints == null)
                continue;

            Vector2[] vertices =
            [
                .. tmxObjectPoints.Select(p => new Vector2(
                    (float)p.X * Engine.scale,
                    (float)p.Y * Engine.scale
                )),
            ];

            collisionObjects.Add(
                new CollisionObject(new Vector2((float)o.X, (float)o.Y), new Polygon2(vertices))
            );

            _entities = coliders;
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var entity in _entities)
        {
            CollisionInfo[] collision = CheckCollisionDetailed(entity);

            if (collision.Length > 0)
                entity.HandleCollision(collision);
        }
    }

    public CollisionInfo[] CheckCollisionDetailed(IColider entity)
    {
        var entityShape = new Polygon2(
            [
                new Vector2(0, 0),
                new Vector2(entity.CollisionsBounds.Width, 0),
                new Vector2(0, entity.CollisionsBounds.Height),
                new Vector2(entity.CollisionsBounds.Width, entity.CollisionsBounds.Height),
            ]
        );
        List<CollisionInfo> collisionInfos = [];
        Vector2 entityPosition = new(entity.CollisionsBounds.X, entity.CollisionsBounds.Y);

        foreach (var collisionObject in collisionObjects)
        {
            bool intersects = Polygon2.Intersects(
                entityShape,
                collisionObject.shape,
                entityPosition,
                collisionObject.position * Engine.scale,
                false
            );

            if (intersects)
                collisionInfos.Add(CalculateCollisionInfo(entity, entityPosition, collisionObject));
        }

        return [.. collisionInfos];
    }

    // Calcular información detallada de la colisión
    private CollisionInfo CalculateCollisionInfo(
        IColider entity,
        Vector2 entityPosition,
        CollisionObject collisionObject
    )
    {
        Vector2 entityCenter =
            entityPosition
            + new Vector2(entity.CollisionsBounds.Width / 2, entity.CollisionsBounds.Height / 2);

        float minDistance = float.MaxValue;
        Line2 minDistanceLine = null;

        // Encontrar la línea más cercana
        foreach (Line2 line in collisionObject.shape.Lines)
        {
            float distance = Line2.Distance(
                line,
                collisionObject.position * Engine.scale,
                entityCenter
            );
            if (distance < minDistance)
            {
                minDistance = distance;
                minDistanceLine = line;
            }
        }

        // Determinar tipo de colisión y normal
        CollisionType type = CollisionType.NONE;
        Vector2 normal = Vector2.Zero;

        if (minDistanceLine.Horizontal)
        {
            if (entityPosition.Y <= collisionObject.position.Y * Engine.scale)
            {
                type = CollisionType.GROUNDED;
                normal = new Vector2(0, -1); // Normal hacia arriba
            }
            else
            {
                type = CollisionType.HEAD;
                normal = new Vector2(0, 1); // Normal hacia abajo
            }
        }
        else if (minDistanceLine.Vertical)
        {
            type = CollisionType.WALL;
            if (entityPosition.X <= collisionObject.position.X * Engine.scale)
            {
                normal = new Vector2(-1, 0); // Normal hacia la izquierda
            }
            else
            {
                normal = new Vector2(1, 0); // Normal hacia la derecha
            }
        }

        return new(collisionObject, normal, minDistance, type, minDistanceLine);
    }

    // Método original mantenido para compatibilidad
    [Obsolete]
    public List<CollisionObject> CheckCollision(IColider entity, Vector2 nextMovement)
    {
        var entityShape = new Polygon2(
            [
                new Vector2(0, 0),
                new Vector2(entity.CollisionsBounds.Width, 0),
                new Vector2(0, entity.CollisionsBounds.Height),
                new Vector2(entity.CollisionsBounds.Width, entity.CollisionsBounds.Height),
            ]
        );
        List<CollisionObject> collisionPolygonsToReturn = [];

        foreach (var collisionPolygon in collisionObjects)
        {
            bool intersects = Polygon2.Intersects(
                entityShape,
                collisionPolygon.shape,
                nextMovement,
                collisionPolygon.position * Engine.scale,
                false
            );

            if (intersects)
            {
                collisionPolygonsToReturn.Add(collisionPolygon);
            }
        }

        return collisionPolygonsToReturn;
    }

    public static float CalculateAngleBetweenEntityAndPolygon(
        Entity entity,
        CollisionObject collisionPolygon
    )
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
