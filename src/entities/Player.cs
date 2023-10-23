using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.animation;
using nx.entity.Objects;
using nx.tile;
using nx.util;
using nx.world;
using SharpMath2;

namespace nx.entity;

class Player : Entity
{
    public Engine engine;

    private static Player instance;

    private static readonly object lockObject = new object();


    protected const string TEXT = "assets/textures/player/kevin";

    private const float SPEED = 350.0f;
    private const float GRAVITY = -9.81f * 2;
    private const float SLIDING_GRAVITY = -9.81f;
    private const float MAX_JUMP_HEIGHT = 3.5f;

    private const float DECREES_VELOCITY_FACTOR = -0.7f;

    private float jumpHeight = 0.5f;
    private float timeOfJumpHeight = 0.0f;

    private bool isSliding = false;

    private bool isJumpKingMovementOn = true;


    public Vector2 screenPosition;

    private readonly SpriteGroupAnimation animations;



    private Player(Game game, Vector2 position) : base(game, position, TEXT, true)
    {
        velocity = new(0, 0);
        velocityGoal = 0.0f;
        collisionBounds = new((int)position.X, (int)position.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);
        screenPosition = new Vector2(position.X, Engine.screenheigth - World.worldHeight + position.Y);

        engine = (Engine)game;


        var WalkAnimation = new SpriteSheetAnimation(
            this,
            "Walk",
            game.Content.Load<Texture2D>("assets/textures/player/Luca3"),
            18,
            18,
            5
        );
        var IdleAnimation = new SpriteSheetAnimation(
            this,
            "Idle",
            game.Content.Load<Texture2D>("assets/textures/player/Lucas_idle"),
            18,
            18,
            3
        );

        animations = new("Idle", WalkAnimation, IdleAnimation);

    }

    public static Player GetInstance(Game game, Vector2 position)
    {
        if (instance == null)
        {
            lock (lockObject) // Thread-safe locking
            {
                instance ??= new Player(game, position);
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
        Rectangle destinationRectangle = new((int)screenPosition.X, (int)screenPosition.Y, Engine.TILE_SIZE, Engine.TILE_SIZE);

        animations.Draw(destinationRectangle);
    }
    public override void Update(GameTime gameTime)
    {
        var kstate = Keyboard.GetState();

        bool isSpaceBarPress = kstate.IsKeyDown(Keys.Space);

        if (kstate.IsKeyDown(Keys.D))
        {
            if (isGrounded && isJumpKingMovementOn)
            {
                velocityGoal = 1;
                direction = DIRECTION.RIGHT;
                if (animations.isFlip())
                    animations.Flip(SpriteEffects.None);
            }


        }
        else if (kstate.IsKeyDown(Keys.A))
        {
            if (isGrounded)
            {
                velocityGoal = -1;
                direction = DIRECTION.LEFT;
                if (!animations.isFlip())
                    animations.Flip(SpriteEffects.FlipHorizontally);
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

        screenPosition.X = Engine.screenWidth - World.worldWidth + position.X;

        if (position.Y >= World.worldHeight - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE)
            screenPosition.Y = Engine.screenheigth - World.worldHeight + position.Y;
        else
            screenPosition.Y = Engine.SCREEN_CENTER_Y;

        collisionBounds.X = (int)position.X;
        collisionBounds.Y = (int)position.Y;

        //Debug.WriteLine(position);

        if (velocityGoal == 0)
            animations.SetGroupAnimation("Idle");
        else
            animations.SetGroupAnimation("Walk");

        animations.Update(gameTime);

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
                    float distance = Line2.Distance(line, collisionObject.position * Engine.scale, playerCenter); //FIXME min distances are wrong.
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
                        isSliding = false;
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
                    ChangeDirection();
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

        /*
        if (isSliding)
        {
            position.X = 0;
            screenPosition.X = 0;
        }
        */
    }

    private void ChangeDirection()
    {
        velocityGoal *= DECREES_VELOCITY_FACTOR;
        velocity.X *= -1;
        if (animations.isFlip())
            animations.Flip(SpriteEffects.None);
        else
            animations.Flip(SpriteEffects.FlipHorizontally);

    }

    private void moveX(GameTime gameTime)
    {
        velocity.X = MovementUtility.lerp(velocityGoal, velocity.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 15);
    }

    private void updateGravity(GameTime gameTime, bool isSpaceBarPress)
    {
        if (!isGrounded)
        {
            velocity.Y -= isSliding ? SLIDING_GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds : GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            jumpHeight = 0;
        }
        else
        {
            velocity.Y = 0.0f;

            handleJumpHeight(gameTime, isSpaceBarPress);
        }

        Debug.WriteLine(jumpHeight + " | " + direction + " " + isGrounded + " " + isSliding);

    }

    private void handleJumpHeight(GameTime gameTime, bool isSpaceBarPress)
    {
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
            isSliding = false;
        }

        timeOfJumpHeight += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

}