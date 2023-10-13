using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nx.entity;

public abstract class Entity : DrawableGameComponent
{
    public Vector2 position;
    protected Texture2D texture { get; set; }
    public bool isCollider { get; private set; }
    public Rectangle collisionBounds;
    public Entity(Game game, Vector2 position, Texture2D texture, bool isCollider = false) : base(game)
    {
        InitializeEntity(game, position, texture, isCollider);
    }
    public Entity(Game game, Vector2 position, string textureURI, bool isCollider = false) : base(game)
    {


        FileStream fileStream = new(textureURI, FileMode.Open);
        Texture2D text = Texture2D.FromStream(GraphicsDevice, fileStream);
        fileStream.Dispose();

        InitializeEntity(game, position, text, isCollider);
    }
    public Entity(Game game, Int16 x, Int16 y, string textureURI, bool isCollider = false) : this(game, new Vector2(x, y), textureURI, isCollider) { }

    private void InitializeEntity(Game game, Vector2 position, Texture2D texture, bool isCollider = false)
    {
        this.position = position;
        this.texture = texture;
        this.isCollider = isCollider;
        if (isCollider)
            collisionBounds = new Rectangle((int)position.X, (int)position.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);
    }
    public override void Draw(GameTime gameTime)
    {
        if (texture != null)
        {
            // Define the destination rectangle for drawing the entity
            Rectangle destinationRectangle = new Rectangle((int)position.X, (int)position.Y, 16, 16);

            Engine.SpriteBatch.Draw(texture, destinationRectangle, Color.White);
        }
    }

    public virtual void onCollisionEnter()
    {

    }
    public virtual void onCollisionExit()
    {

    }

}