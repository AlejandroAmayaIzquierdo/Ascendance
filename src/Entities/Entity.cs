using System;
using Microsoft.Xna.Framework;
using nx.world;

namespace nx.entity;

public enum Entities
{
    PLAYER,
    BOT_MASTER,
}

public enum DIRECTION
{
    LEFT,
    RIGHT,
    NONE,
}

public abstract class Entity : DrawableGameComponent
{
    public Vector2 position;
    public Vector2 velocity;
    protected float velocityGoal;

    public float animationDuration;

    public DIRECTION direction;
    public bool isGrounded = false;

    public Entity(Game game, Vector2 position)
        : base(game)
    {
        InitializeEntity(position);
    }

    public Entity(Game game, short x, short y)
        : this(game, new Vector2(x, y)) { }

    private void InitializeEntity(Vector2 position)
    {
        this.position = position;
    }

    public virtual void Update(GameTime gameTime, World world) { }

    public override void Draw(GameTime gameTime)
    {
        throw new NotImplementedException();
    }

    public static Entity LoadEntityByName(Game game, Entities entity, Vector2 position)
    {
        return entity switch
        {
            Entities.PLAYER => Player.GetInstance(game, position),
            Entities.BOT_MASTER => new BotMaster(game, position),
            _ => null,
        };
    }
}
