
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
    private readonly List<Entity> Entities;

    private Player playerRef;

    private int ViewportCenterY;

    private Polygon2 playerShape;
    public CollisionManager(TmxObjectGroup collisionGroup)
    {
        playerShape = new Polygon2(new[] {
        new Vector2(0, 0),
        new Vector2(16, 0),
        new Vector2(0,16),
        new Vector2(16,16)
        });
        collisionObjects = new List<CollisionObject>();
        foreach (var o in collisionGroup.Objects)
        {

            Debug.WriteLine(o.Points);

            Collection<TmxObjectPoint> tmxObjectPoints = o.Points;

            if (tmxObjectPoints == null)
                continue;

            Vector2[] vertices = tmxObjectPoints.Select(p => new Vector2((float)p.X * Engine.scale, (float)p.Y * Engine.scale)).ToArray();


            collisionObjects.Add(new CollisionObject(new Vector2((float)o.X, (float)o.Y), new Polygon2(vertices)));

        }

        playerRef = Player.GetInstance();

        ViewportCenterY = Engine.viewport.Y / 2;



    }

    private void GeneratedColissionShapes()
    {

    }
    public CollisionManager(TmxObjectGroup collisionGroup, Entity[] entities)
    {
        //TODO make constructor
    }

    public bool Update()
    {

        foreach (var collisionPolygon in collisionObjects)
        {
            if (Polygon2.Intersects(playerShape, collisionPolygon.shape, playerRef.position, collisionPolygon.position * Engine.scale, false))
            {
                //playerRef.isGrounded = true;
                return true;
            }
        }
        playerRef.isGrounded = false;
        return false;
    }
}