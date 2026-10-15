using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using nx.entity;
using SharpMath2;
using TiledSharp;

namespace nx.tile;

public class CollisionObject
{
    // Posición del objeto en coordenadas de Tiled (sin escalar), usada para dibujar el debug.
    public Vector2 position;
    public Polygon2 shape;

    // Caja envolvente en coordenadas de mundo (escaladas), usada para resolver colisiones.
    public readonly float Left;
    public readonly float Top;
    public readonly float Right;
    public readonly float Bottom;

    public CollisionObject(Vector2 position, Polygon2 shape)
    {
        this.position = position;
        this.shape = shape;

        Vector2 origin = position * Engine.scale;
        Left = origin.X + shape.Vertices.Min(v => v.X);
        Right = origin.X + shape.Vertices.Max(v => v.X);
        Top = origin.Y + shape.Vertices.Min(v => v.Y);
        Bottom = origin.Y + shape.Vertices.Max(v => v.Y);
    }
}

public struct CollisionResult
{
    public bool HitLeft;
    public bool HitRight;
    public bool HitCeiling;
    public bool HitFloor;

    public readonly bool HitWall => HitLeft || HitRight;
}

public class CollisionManager
{
    // Margen para que dos cajas que solo se tocan por un borde no cuenten como solapadas.
    private const float EPSILON = 0.001f;

    // Distancia máxima bajo los pies a la que se sigue considerando que la entidad está en el suelo.
    private const float GROUND_PROBE = 0.5f;

    public readonly List<CollisionObject> CollisionObjects = [];

    private readonly List<ICollider> _entities;

    public List<ICollider> Coliders => _entities;

    public CollisionManager(TmxObjectGroup collisionGroup, List<ICollider> coliders)
    {
        _entities = coliders;

        foreach (var o in collisionGroup.Objects)
        {
            Vector2[] vertices;

            if (o.Points is not null)
            {
                vertices =
                [
                    .. o.Points.Select(p => new Vector2(
                        (float)p.X * Engine.scale,
                        (float)p.Y * Engine.scale
                    )),
                ];
            }
            else if (o.Width > 0 && o.Height > 0)
            {
                // Objetos rectangulares de Tiled (sin <polygon>).
                float w = (float)o.Width * Engine.scale;
                float h = (float)o.Height * Engine.scale;
                vertices = [new(0, 0), new(w, 0), new(w, h), new(0, h)];
            }
            else
            {
                continue;
            }

            CollisionObjects.Add(
                new CollisionObject(new Vector2((float)o.X, (float)o.Y), new Polygon2(vertices))
            );
        }
    }

    /// <summary>
    /// Mueve la caja de colisión <paramref name="hitbox"/> (relativa a <paramref name="position"/>)
    /// según <paramref name="delta"/>, primero en X y después en Y, deteniéndola contra los
    /// sólidos del nivel. El barrido evita atravesar plataformas aunque la velocidad sea alta.
    /// </summary>
    public CollisionResult Move(ref Vector2 position, Rectangle hitbox, Vector2 delta)
    {
        CollisionResult result = new();

        if (delta.X != 0)
        {
            float moved = SweepX(position, hitbox, delta.X, out bool hit);
            position.X += moved;
            if (hit)
            {
                result.HitRight = delta.X > 0;
                result.HitLeft = delta.X < 0;
            }
        }

        if (delta.Y != 0)
        {
            float moved = SweepY(position, hitbox, delta.Y, out bool hit);
            position.Y += moved;
            if (hit)
            {
                result.HitFloor = delta.Y > 0;
                result.HitCeiling = delta.Y < 0;
            }
        }

        return result;
    }

    public bool IsOnGround(Vector2 position, Rectangle hitbox)
    {
        SweepY(position, hitbox, GROUND_PROBE, out bool hit);
        return hit;
    }

    private float SweepX(Vector2 position, Rectangle hitbox, float dx, out bool hit)
    {
        float left = position.X + hitbox.Left;
        float right = position.X + hitbox.Right;
        float top = position.Y + hitbox.Top;
        float bottom = position.Y + hitbox.Bottom;

        hit = false;
        float allowed = dx;

        foreach (var solid in CollisionObjects)
        {
            if (!Overlaps(top, bottom, solid.Top, solid.Bottom))
                continue;

            if (dx > 0 && solid.Left >= right - EPSILON && solid.Left - right < allowed)
            {
                allowed = Math.Max(0, solid.Left - right);
                hit = true;
            }
            else if (dx < 0 && solid.Right <= left + EPSILON && solid.Right - left > allowed)
            {
                allowed = Math.Min(0, solid.Right - left);
                hit = true;
            }
        }

        return allowed;
    }

    private float SweepY(Vector2 position, Rectangle hitbox, float dy, out bool hit)
    {
        float left = position.X + hitbox.Left;
        float right = position.X + hitbox.Right;
        float top = position.Y + hitbox.Top;
        float bottom = position.Y + hitbox.Bottom;

        hit = false;
        float allowed = dy;

        foreach (var solid in CollisionObjects)
        {
            if (!Overlaps(left, right, solid.Left, solid.Right))
                continue;

            if (dy > 0 && solid.Top >= bottom - EPSILON && solid.Top - bottom < allowed)
            {
                allowed = Math.Max(0, solid.Top - bottom);
                hit = true;
            }
            else if (dy < 0 && solid.Bottom <= top + EPSILON && solid.Bottom - top > allowed)
            {
                allowed = Math.Min(0, solid.Bottom - top);
                hit = true;
            }
        }

        return allowed;
    }

    private static bool Overlaps(float minA, float maxA, float minB, float maxB) =>
        minA < maxB - EPSILON && maxA > minB + EPSILON;
}
