using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nx.entity;

public abstract class Entity : DrawableGameComponent
{
    protected Vector2 position { get; set; }
    protected Texture2D texture { get; set; }
    public Entity(Game game, Vector2 position, Texture2D texture) : base(game)
    {
        this.position = position;
        this.texture = texture;
    }
    public Entity(Game game, Vector2 position, string textureURI) : base(game)
    {
        this.position = position;

        FileStream fileStream = new(textureURI, FileMode.Open);
        this.texture = Texture2D.FromStream(GraphicsDevice, fileStream);
        fileStream.Dispose();
    }
    public Entity(Game game, Int16 x, Int16 y, string textureURI) : base(game)
    {
        this.position = new Vector2(x, y);

        FileStream fileStream = new(textureURI, FileMode.Open);
        this.texture = Texture2D.FromStream(GraphicsDevice, fileStream);
        fileStream.Dispose();
    }
    public virtual void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        if (texture != null)
            _spriteBatch.Draw(texture, position, Color.White);
    }
}