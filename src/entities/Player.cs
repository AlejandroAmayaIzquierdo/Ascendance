using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.animation;
using nx.tile;
using nx.util;
using nx.world;
using SharpMath2;

namespace nx.entity;

class Player : Entity
{
    private static Player instance;

    protected const string TEXT = "assets/textures/player/kevin";
    private static readonly object lockObject = new object();

    private const float SPEED = 350.0f;
    private const float GRAVITY = -9.81f * 2;
    private const float MAX_JUMP_HEIGHT = 3.5f;

    private const float DECREES_VELOCITY_FACTOR = -0.7f;

    private float jumpHeight = 0.5f;

    public Vector2 screenPosition;

    public Engine engine;

    private float timeOfJumpHeight = 0.0f;

    private SpriteSheetAnimation animation;


    private Player(Game game, Vector2 position) : base(game, position, TEXT, true)
    {
        velocity = new(0, 0);
        velocityGoal = 0.0f;
        collisionBounds = new((int)position.X, (int)position.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);
        screenPosition = new Vector2(position.X, Engine.screenheigth - World.worldHeigth + position.Y);

        engine = (Engine)game;

        animation = new SpriteSheetAnimation(
            this,
            game.Content.Load<Texture2D>("assets/textures/nude/Tiny16_Nude"),
            5
        );

        animation.Init(0);
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
            //Rectangle destinationRectangle = new((int)screenPosition.X, (int)screenPosition.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);


            //Engine.SpriteBatch.Draw(texture, destinationRectangle, Color.White);
        }
        Rectangle destinationRectangle = new((int)screenPosition.X, (int)screenPosition.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);


        animation.Draw(destinationRectangle);
    }
    public override void Update(GameTime gameTime)
    {
        var kstate = Keyboard.GetState();

        bool isSpaceBarPress = kstate.IsKeyDown(Keys.Space);

        if (kstate.IsKeyDown(Keys.D))
        {
            if (isGrounded)
            {
                velocityGoal = 1;
                direction = DIRECTION.RIGHT;
                animation.SetAnimation(2);
            }

        }
        else if (kstate.IsKeyDown(Keys.A))
        {
            if (isGrounded)
            {
                velocityGoal = -1;
                direction = DIRECTION.LEFT;
                animation.SetAnimation(1);
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

        updateGravity(gameTime, isSpaceBarPress);


        float realSpeed = SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

        position += new Vector2(velocity.X * realSpeed, velocity.Y);
        screenPosition.X += velocity.X * realSpeed;

        if (position.Y >= World.worldHeigth - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE)
            screenPosition.Y = Engine.screenheigth - World.worldHeigth + position.Y;
        else
            screenPosition.Y = Engine.SCREEN_CENTER_Y;

        collisionBounds.X = (int)position.X;
        collisionBounds.Y = (int)position.Y;

        //Debug.WriteLine(position);

        animation.Update(gameTime);

        base.Update(gameTime);
    }

    private void handleCollision()
    {
        List<CollisionObject> collisionObjects = engine.world.collisionManager.CheckCollision(this, position);
        Vector2 playerCenter = position + new Vector2(collisionBounds.Width / 2, collisionBounds.Height / 2);

        isGrounded = false;

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
                        position.Y = (collisionObject.position.Y * Engine.scale) + minDistanceLine.MaxY;
                    }

                }
                else if (minDistanceLine.Vertical)
                {
                    velocityGoal *= DECREES_VELOCITY_FACTOR;
                    velocity.X *= -1;
                    if (position.X <= (collisionObject.position.X * Engine.scale))
                    {
                        position.X = (collisionObject.position.X * Engine.scale) + minDistanceLine.MinX - (collisionBounds.Width + 0.1f);
                        screenPosition.X = (collisionObject.position.X * Engine.scale) + minDistanceLine.MinX - (collisionBounds.Width + 0.1f);
                    }
                    else
                    {
                        position.X = (collisionObject.position.X * Engine.scale) + minDistanceLine.MinX + 0.1f;
                        screenPosition.X = (collisionObject.position.X * Engine.scale) + minDistanceLine.MinX + 0.1f;
                    }

                }
            }
        }

        if (position.X <= 0)
        {
            position.X = 0;
            screenPosition.X = 0;
            velocityGoal *= DECREES_VELOCITY_FACTOR;
            velocity.X *= -1;

        }
        else if (position.X >= Engine.screenWidth - collisionBounds.Width)
        {
            position.X = Engine.screenWidth - collisionBounds.Width;
            screenPosition.X = Engine.screenWidth - collisionBounds.Width;
            velocityGoal *= DECREES_VELOCITY_FACTOR;
            velocity.X *= -1;
        }
    }

    private void moveX(GameTime gameTime)
    {
        velocity.X = MovementUtility.lerp(velocityGoal, velocity.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 15);
    }

    private void updateGravity(GameTime gameTime, bool isSpaceBarPress)
    {
        if (!isGrounded)
        {
            velocity.Y -= GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            jumpHeight = 0;
        }
        else
        {
            velocity.Y = 0.0f;

            if (isSpaceBarPress)
            {
                velocityGoal = 0;
                velocity.X = 0;
            }


            if (isSpaceBarPress && jumpHeight <= MAX_JUMP_HEIGHT && timeOfJumpHeight >= 0.05f)
            {
                jumpHeight += 0.25f;
                timeOfJumpHeight = 0.0f;
            }

            if (!isSpaceBarPress && jumpHeight > 0.5f)
            {
                velocity.Y -= (float)Math.Sqrt(jumpHeight * -2f * GRAVITY);
                isGrounded = false;
            }

            timeOfJumpHeight += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Debug.WriteLine(jumpHeight + " | " + direction);
        }
    }

}