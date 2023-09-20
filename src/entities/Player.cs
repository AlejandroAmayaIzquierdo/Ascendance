using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.util;

namespace nx.entity;

class Player : Entity
{

    private const float SPEED = 200.0f;
    private const float GRAVITY = -9.81f;
    private const float GROUND_LEVEL = 240.0f;

    protected Vector2 velocity;
    protected float velocityGoal;


    public Player(Game game, Vector2 position, string texture) : base(game, position, texture)
    {
        velocity = new(0, 0);
        velocityGoal = 0.0f;
    }

    public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
    {
        base.Draw(gameTime, _spriteBatch);
    }
    public override void Update(GameTime gameTime)
    {
        var kstate = Keyboard.GetState();

        float realSpeed = SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (kstate.IsKeyDown(Keys.D))
        {
            if (isGrounded())
                velocityGoal = 1;
        }
        else if (kstate.IsKeyDown(Keys.A))
        {
            if (isGrounded())
                velocityGoal = -1;
        }
        else
        {
            if (isGrounded())
                velocityGoal = 0;
        }

        velocity.X = MovementUtility.lerp(velocityGoal, velocity.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 15);

        updateGravity(gameTime, kstate.IsKeyDown(Keys.Space));

        position += new Vector2(velocity.X * realSpeed, velocity.Y);


        base.Update(gameTime);
    }

    public void updateGravity(GameTime gameTime, bool isJumping)
    {
        if (position.Y >= GROUND_LEVEL)
        {
            if (isJumping)
            {
                velocity.Y -= (float)Math.Sqrt(0.5f * -2f * GRAVITY);
            }
            else
            {
                position = new(position.X, GROUND_LEVEL);
                velocity.Y = 0.0F;
            }

        }

        velocity.Y -= GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }


    public bool isGrounded() { return position.Y >= GROUND_LEVEL; }
}