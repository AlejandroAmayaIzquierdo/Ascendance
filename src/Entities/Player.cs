using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.animation;
using nx.input;
using nx.tile;
using nx.util;
using nx.world;

namespace nx.entity;

public class Player : Entity, IColider
{
    public Engine engine;

    private static Player instance;

    private static readonly Lock lockObject = new();

    private const float SPEED = 350.0f;
    private const float GRAVITY = -9.81f * 2;
    private const float MAX_JUMP_HEIGHT = 3.5f;
    private const float DECREES_VELOCITY_FACTOR = -0.7f;
    private const float ACCELERATION = 10f;

    private const string WALK_ANIMATION_PATH = "assets/textures/player/Luca_walk";
    private const string IDLE_ANIMATION_PATH = "assets/textures/player/Lucas_idle";

    private const int CELL_SPRITE_SHEET_SIZE = 18;

    private const int WALK_ANIMATION_FRAMERATE = 5;
    private const int IDLE_ANIMATION_FRAMERATE = 3;

    private float jumpHeight = 0.5f;
    private float timeOfJumpHeight = 0.0f;

    public Vector2 screenPosition;

    private readonly SpriteGroupAnimation animations;

    public Rectangle CollisionsBounds => _collisionsBounds;
    private Rectangle _collisionsBounds;

    private InputManager _inputManager;

    private Player(Game game, Vector2 position)
        : base(game, position)
    {
        velocity = new(0, 0);
        velocityGoal = 0.0f;
        _collisionsBounds = new(
            (int)position.X,
            (int)position.Y,
            Engine.TILE_SIZE,
            Engine.TILE_SIZE
        );
        screenPosition = new Vector2(
            position.X,
            Engine.screenHeight - World.worldHeight + position.Y
        );

        engine = (Engine)game;

        var WalkAnimation = new SpriteSheetAnimation(
            this,
            "Walk",
            game.Content.Load<Texture2D>(WALK_ANIMATION_PATH),
            CELL_SPRITE_SHEET_SIZE,
            CELL_SPRITE_SHEET_SIZE,
            WALK_ANIMATION_FRAMERATE
        );
        var IdleAnimation = new SpriteSheetAnimation(
            this,
            "Idle",
            game.Content.Load<Texture2D>(IDLE_ANIMATION_PATH),
            CELL_SPRITE_SHEET_SIZE,
            CELL_SPRITE_SHEET_SIZE,
            IDLE_ANIMATION_FRAMERATE
        );

        animations = new("Idle", WalkAnimation, IdleAnimation);

        _inputManager = new();
    }

    public static Player GetInstance(Game game, Vector2 position)
    {
        if (instance == null)
        {
            lock (lockObject) // Thread-safe locking
                instance ??= new Player(game, position);
        }
        return instance;
    }

    public static Player GetInstance() => instance;

    public override void Draw(GameTime gameTime)
    {
        Rectangle destinationRectangle = new(
            (int)screenPosition.X,
            (int)screenPosition.Y,
            Engine.TILE_SIZE,
            Engine.TILE_SIZE
        );

        animations.Draw(destinationRectangle);
    }

    public override void Update(GameTime gameTime)
    {
        _inputManager.Update();

        HandleMovementInput();
        ApplyPhysics(gameTime);
        UpdatePosition(gameTime);
        CheckWorldBounds();
        UpdateAnimations(gameTime);

        base.Update(gameTime);

        if (position.Y < 0)
        {
            Engine.World!.NextLevel();
        }

        isGrounded = false;
    }

    public void HandleMovementInput()
    {
        if (!isGrounded)
            return;

        if (_inputManager.IsMovingRight())
        {
            velocityGoal = 1;
            direction = DIRECTION.RIGHT;
            if (animations.isFlip())
                animations.Flip(SpriteEffects.None);
            return;
        }

        if (_inputManager.IsMovingLeft())
        {
            velocityGoal = -1;
            direction = DIRECTION.LEFT;
            if (!animations.isFlip())
                animations.Flip(SpriteEffects.FlipHorizontally);
            return;
        }

        velocityGoal = 0;
        direction = DIRECTION.NONE;
    }

    public void ApplyPhysics(GameTime gameTime)
    {
        float newVelocityX = MovementUtility.lerp(
            velocityGoal,
            velocity.X,
            (float)gameTime.ElapsedGameTime.TotalSeconds * ACCELERATION
        );

        velocity = new Vector2(newVelocityX, velocity.Y);

        UpdateGravity(gameTime);
    }

    public void HandleCollision(CollisionInfo[] collisions)
    {
        isGrounded = false;
        foreach (var collision in collisions)
        {
            var collisionObject = collision.Object;
            switch (collision.Type)
            {
                case CollisionType.GROUNDED:
                    isGrounded = true;
                    velocity.Y = 0;
                    position.Y =
                        (collisionObject.position.Y * Engine.scale) - _collisionsBounds.Height;
                    break;
                case CollisionType.HEAD:
                    velocity.Y = 0;
                    position.Y =
                        collisionObject.position.Y * Engine.scale + _collisionsBounds.Height + 1; // El rectagulo de colision + la altura del jugador + un offset
                    break;
                case CollisionType.WALL:
                    ChangeDirection();
                    if (position.X <= (collisionObject.position.X * Engine.scale))
                    {
                        position.X =
                            (collisionObject.position.X * Engine.scale)
                            + collision.ContactLine.MinX
                            - (_collisionsBounds.Width + 0.1f);
                        screenPosition.X =
                            (collisionObject.position.X * Engine.scale)
                            + collision.ContactLine.MinX
                            - (_collisionsBounds.Width + 0.1f);
                    }
                    else
                    {
                        position.X =
                            (collisionObject.position.X * Engine.scale)
                            + collision.ContactLine.MinX
                            + 0.1f;
                        screenPosition.X =
                            (collisionObject.position.X * Engine.scale)
                            + collision.ContactLine.MinX
                            + 0.1f;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void CheckWorldBounds()
    {
        if (position.X < 0)
        {
            position.X = 0;
            screenPosition.X = 0;
            ChangeDirection();
        }
        else if (position.X >= Engine.screenWidth - _collisionsBounds.Width)
        {
            position.X = Engine.screenWidth - _collisionsBounds.Width;
            screenPosition.X = Engine.screenWidth - _collisionsBounds.Width;
            ChangeDirection();
        }
    }

    private void UpdatePosition(GameTime gameTime)
    {
        float realSpeed = SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

        position += new Vector2(velocity.X * realSpeed, velocity.Y);

        screenPosition.X = Engine.screenWidth - World.worldWidth + position.X;

        if (position.Y >= World.worldHeight - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE)
            screenPosition.Y = Engine.screenHeight - World.worldHeight + position.Y;
        else if (position.Y < Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE)
            screenPosition.Y = position.Y + Engine.TILE_SIZE;
        else
            screenPosition.Y = Engine.SCREEN_CENTER_Y;

        _collisionsBounds.X = (int)position.X;
        _collisionsBounds.Y = (int)position.Y;
    }

    private void UpdateAnimations(GameTime gameTime)
    {
        if (velocityGoal == 0)
            animations.SetGroupAnimation("Idle");
        else
            animations.SetGroupAnimation("Walk");

        animations.Update(gameTime);
    }

    private void UpdateGravity(GameTime gameTime)
    {
        bool isSpaceBarPress = _inputManager.IsJumpPressed();
        if (!isGrounded)
        {
            velocity.Y -= GRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            jumpHeight = 0;
        }
        else
        {
            velocity.Y = 0.0f;

            HandleJumpHeight(gameTime, isSpaceBarPress);
        }
    }

    private void HandleJumpHeight(GameTime gameTime, bool isSpaceBarPress)
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
        }

        timeOfJumpHeight += (float)gameTime.ElapsedGameTime.TotalSeconds;
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
}
