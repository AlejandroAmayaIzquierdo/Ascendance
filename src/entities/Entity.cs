using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.world;

namespace nx.entity;

public enum Entities
{
    PLAYER
}
public enum DIRECTION
{
    LEFT,
    RIGHT,
    NONE
}

public abstract class Entity : DrawableGameComponent
{
    public Vector2 position;
    protected Texture2D texture { get; set; }
    public bool isCollider { get; private set; }
    public Rectangle collisionBounds;
    public Vector2 velocity;
    protected float velocityGoal;

    public DIRECTION direction;
    public bool isGrounded = false;
    public Entity(Game game, Vector2 position, Texture2D texture, bool isCollider = false) : base(game)
    {
        InitializeEntity(position, texture, isCollider);
    }
    public Entity(Game game, Vector2 position, string textureURI, bool isCollider = false) : base(game)
    {
        Texture2D text = game.Content.Load<Texture2D>(textureURI);

        InitializeEntity(position, text, isCollider);
    }
    public Entity(Game game, Int16 x, Int16 y, string textureURI, bool isCollider = false) : this(game, new Vector2(x, y), textureURI, isCollider) { }

    private void InitializeEntity(Vector2 position, Texture2D texture, bool isCollider = false)
    {
        this.position = position;
        this.texture = texture;
        this.isCollider = isCollider;
        if (isCollider)
            collisionBounds = new Rectangle((int)position.X, (int)position.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);
    }
    public virtual void Update(GameTime gameTime, World world) { }
    public override void Draw(GameTime gameTime)
    {
        if (texture != null)
        {
            Rectangle destinationRectangle = new Rectangle((int)position.X, (int)position.Y, 16, 16);

            Engine.SpriteBatch.Draw(texture, destinationRectangle, Color.White);
        }
    }

    public virtual void onCollision()
    {
        throw new NotImplementedException();
    }

    public static Entity LoadEntityByName(Game game, Entities entity, Vector2 position)
    {
        switch (entity)
        {
            case Entities.PLAYER:
                return Player.GetInstance(game, position);
            default:
                return null;
        }
    }


}