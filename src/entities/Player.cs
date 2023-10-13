using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.util;

namespace nx.entity;

class Player : Entity
{
    private static Player instance;
    private static readonly object lockObject = new object();

    private const float SPEED = 350.0f;
    private const float GRAVITY = -9.81f * 2;
    private const float JUMP_HEIGHT = 3.0f;

    public Vector2 velocity;
    protected float velocityGoal;

    public bool isGrounded = false;

    public Vector2 screenPosition;


    private Player(Game game, Vector2 position, string texture) : base(game, position, texture)
    {
        velocity = new(0, 0);
        velocityGoal = 0.0f;
        collisionBounds = new((int)position.X, (int)position.Y, 16, 16);
        screenPosition = new Vector2(position.X, Engine.SCREEN_CENTER_Y);
    }

    public static Player GetInstance(Game game, Vector2 position, string texture)
    {
        if (instance == null)
        {
            lock (lockObject) // Thread-safe locking
            {
                if (instance == null)
                    instance = new Player(game, position, texture);
            }
        }
        return instance;
    }
    public static Player GetInstance()
    {
        return instance;
    }

    public override void Draw(GameTime gameTime)
    {
        if (texture != null)
        {
            Rectangle destinationRectangle = new Rectangle((int)screenPosition.X, (int)screenPosition.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);


            Engine.SpriteBatch.Draw(texture, destinationRectangle, Color.White);
        }
    }
    public override void Update(GameTime gameTime)
    {
        var kstate = Keyboard.GetState();

        float realSpeed = SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (kstate.IsKeyDown(Keys.D))
        {
            //if (isGrounded)
            velocityGoal = 1;
        }
        else if (kstate.IsKeyDown(Keys.A))
        {
            //if (isGrounded)
            velocityGoal = -1;
        }
        else
        {
            //if (isGrounded)
            velocityGoal = 0;
        }

        velocity.X = MovementUtility.lerp(velocityGoal, velocity.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 15);

        updateGravity(gameTime, kstate.IsKeyDown(Keys.Space));

        position += new Vector2(velocity.X * realSpeed, velocity.Y);
        screenPosition.X += velocity.X * realSpeed;

        collisionBounds.X = (int)position.X;
        collisionBounds.Y = (int)position.Y;

        Debug.WriteLine(position);


        base.Update(gameTime);
    }

    public void updateGravity(GameTime gameTime, bool isJumping)
    {
        if (!isGrounded) //if (!isGrounded)
        {
            velocity.Y -= GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            velocity.Y = 0.0f;

            if (isJumping)
            {
                velocity.Y -= (float)Math.Sqrt(JUMP_HEIGHT * -2f * GRAVITY);
            }
        }
    }

}