using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.tile;
using nx.util;
using SharpMath2;

namespace nx.entity;

class Player : Entity
{
    private static Player instance;

    protected const string TEXT = "Content/kevin.png";
    private static readonly object lockObject = new object();

    private const float SPEED = 350.0f;
    private const float GRAVITY = -9.81f * 2;
    private const float JUMP_HEIGHT = 3.5f;

    public Vector2 screenPosition;

    public Engine engine;


    private Player(Game game, Vector2 position) : base(game, position, TEXT, true)
    {
        velocity = new(0, 0);
        velocityGoal = 0.0f;
        collisionBounds = new((int)position.X, (int)position.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);
        screenPosition = new Vector2(position.X, Engine.SCREEN_CENTER_Y);

        engine = (Engine)game;
    }

    public static Player GetInstance(Game game, Vector2 position)
    {
        if (instance == null)
        {
            lock (lockObject) // Thread-safe locking
            {
                if (instance == null)
                    instance = new Player(game, position);
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

        bool isJumping = kstate.IsKeyDown(Keys.Space);

        if (kstate.IsKeyDown(Keys.D))
        {
            if (isGrounded)
            {
                velocityGoal = 1;
                direction = DIRECTION.RIGHT;
            }


        }
        else if (kstate.IsKeyDown(Keys.A))
        {
            if (isGrounded)
            {
                velocityGoal = -1;
                direction = DIRECTION.LEFT;
            }


        }
        else
        {
            if (isGrounded)
            {
                velocityGoal = 0;
                direction = DIRECTION.NONE;
            }


        }

        handleCollision();

        moveX(gameTime);

        updateGravity(gameTime, isJumping);


        float realSpeed = SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

        position += new Vector2(velocity.X * realSpeed, velocity.Y);
        screenPosition.X += velocity.X * realSpeed;

        collisionBounds.X = (int)position.X;
        collisionBounds.Y = (int)position.Y;

        base.Update(gameTime);
    }

    private void handleCollision()
    {
        List<CollisionObject> collisionObjects = engine.world.collisionManager.CheckCollision(this, position);
        isGrounded = false;
        Vector2 playerCenter = position + new Vector2(collisionBounds.Width / 2, collisionBounds.Height / 2);



        if (collisionObjects.Count > 0)
        {
            foreach (var collisionObject in collisionObjects)
            {
                float minDistance = float.MaxValue;
                Line2 minDistanceLine = null;


                foreach (Line2 line in collisionObject.shape.Lines)
                {
                    float distance = Line2.Distance(line, collisionObject.position * Engine.scale, playerCenter);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minDistanceLine = line;
                    }
                }

                Debug.WriteLine(minDistanceLine.Vertical);

                if (minDistanceLine.Horizontal)
                {
                    if (position.Y <= collisionObject.position.Y * Engine.scale)
                    {
                        isGrounded = true;
                        velocity.Y = 0;
                        position.Y = (collisionObject.position.Y * Engine.scale) - collisionBounds.Height;
                    }
                    else
                    {
                        velocity.Y = 0;
                        position.Y = ((collisionObject.position.Y * Engine.scale) + minDistanceLine.MaxY);
                    }

                }
                else if (minDistanceLine.Vertical)
                {
                    velocityGoal = 0;
                    velocity.X = 0;
                    if (position.X <= (collisionObject.position.X * Engine.scale))
                    {
                        position.X = ((collisionObject.position.X * Engine.scale) + minDistanceLine.MinX) - (collisionBounds.Width + 0.1f);
                        screenPosition.X = ((collisionObject.position.X * Engine.scale) + minDistanceLine.MinX) - (collisionBounds.Width + 0.1f);
                    }
                    else
                    {
                        position.X = (collisionObject.position.X * Engine.scale) + minDistanceLine.MinX + 0.1f;
                        screenPosition.X = (collisionObject.position.X * Engine.scale) + minDistanceLine.MinX + 0.1f;
                    }

                }
            }


        }
    }

    private void moveX(GameTime gameTime)
    {
        velocity.X = MovementUtility.lerp(velocityGoal, velocity.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 15);
    }

    private void updateGravity(GameTime gameTime, bool isJumping)
    {
        if (!isGrounded)
        {
            velocity.Y -= GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            velocity.Y = 0.0f;

            if (isJumping)
            {
                isGrounded = false;
                velocity.Y -= (float)Math.Sqrt(JUMP_HEIGHT * -2f * GRAVITY);

            }
        }
    }

}