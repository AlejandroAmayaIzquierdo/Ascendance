using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.animation;
using nx.input;
using nx.tile;
using nx.util;
using nx.world;

namespace nx.entity;

public class Player : Entity, ICollider
{
    public Engine engine;

    private static Player? _instance;

    public static Player Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("Player not initialized");
            return _instance;
        }
    }

    public static bool IsInitialized => _instance is not null;

    private static readonly Lock _lock = new();

    private const float SPEED = 350.0f;
    private const float GRAVITY = -9.81f * 2;
    private const float MAX_GRAVITY_VELOCITY = 25f;
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

    public Vector2 ScreenPosition;

    private readonly SpriteGroupAnimation animations;

    public Rectangle CollisionsBounds =>
        new(
            (int)Position.X + _collisionsBounds.X,
            (int)Position.Y + _collisionsBounds.Y,
            _collisionsBounds.Width,
            _collisionsBounds.Height
        );
    private Rectangle _collisionsBounds;

    private InputManager _inputManager;

    private Player(Game game, Vector2 position)
        : base(game, position)
    {
        velocity = new(0, 0);
        velocityGoal = 0.0f;
        _collisionsBounds = new(Engine.TILE_SIZE / 4, 16, Engine.TILE_SIZE / 2, 16);
        ScreenPosition = new Vector2(
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

    public static Player Initialize(Game game, Vector2? position)
    {
        lock (_lock)
        {
            if (_instance is not null)
                throw new InvalidOperationException("Player already initialize");

            _instance = new Player(game, position ?? Vector2.Zero);
            return _instance;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Rectangle destinationRectangle = new(
            (int)ScreenPosition.X,
            (int)ScreenPosition.Y,
            Engine.TILE_SIZE,
            Engine.TILE_SIZE
        );

        animations.Draw(destinationRectangle);
    }

    public void DrawCollision(GameTime gameTime)
    {
        Rectangle hitBox = new(
            (int)ScreenPosition.X + _collisionsBounds.X,
            (int)ScreenPosition.Y + _collisionsBounds.Y,
            _collisionsBounds.Width,
            _collisionsBounds.Height
        );

        Engine.SpriteBatch.Draw(
            engine.Content.Load<Texture2D>("assets/textures/collision_entities_box"),
            hitBox,
            Color.Red * 0.5f
        );
    }

    public override void Update(GameTime gameTime)
    {
        _inputManager.Update();

        HandleMovementInput();
        ApplyPhysics(gameTime);
        UpdatePosition(gameTime);
        CheckWorldBounds();
        UpdateAnimations(gameTime);

        if (Position.Y < 0)
        {
            Engine.World!.NextLevel();
        }
        else if (Position.Y > World.worldHeight)
        {
            Engine.World!.PreviousLevel();
        }

        isGrounded = false;

        base.Update(gameTime);
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
                    Position.Y =
                        (collisionObject.position.Y * Engine.scale)
                        - _collisionsBounds.Height
                        - _collisionsBounds.Y;
                    break;
                case CollisionType.HEAD:
                    velocity.Y = 0;
                    Position += collision.Normal;
                    break;
                case CollisionType.WALL:
                    ChangeDirection();
                    Position += collision.Normal;
                    break;
                default:
                    break;
            }
        }
    }

    private void CheckWorldBounds()
    {
        if (Position.X < 0)
        {
            Position.X = 0;
            ScreenPosition.X = 0;
            ChangeDirection();
        }
        else if (Position.X >= Engine.screenWidth - _collisionsBounds.Width)
        {
            Position.X = Engine.screenWidth - _collisionsBounds.Width;
            ScreenPosition.X = Engine.screenWidth - _collisionsBounds.Width;
            ChangeDirection();
        }
    }

    private void UpdatePosition(GameTime gameTime)
    {
        float realSpeed = SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

        Position += new Vector2(velocity.X * realSpeed, velocity.Y);

        ScreenPosition.X = Engine.screenWidth - World.worldWidth + Position.X;

        if (Position.Y >= World.worldHeight - Engine.SCREEN_CENTER_Y - Engine.TILE_SIZE)
            ScreenPosition.Y = Engine.screenHeight - World.worldHeight + Position.Y;
        else if (Position.Y < Engine.SCREEN_CENTER_Y)
            ScreenPosition.Y = Position.Y;
        else
            ScreenPosition.Y = Engine.SCREEN_CENTER_Y;
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
            if (velocity.Y < MAX_GRAVITY_VELOCITY)
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
