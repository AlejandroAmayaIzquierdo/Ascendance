using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.animation;
using nx.world;

namespace nx.entity;

public class BotMaster : Entity
{
    public Vector2 screenPosition;

    private readonly SpriteGroupAnimation _animations;

    private const string IDLE_ANIMATION_PATH = "assets/textures/BotMaster/BotMaster_idle";

    private const int CELL_SPRITE_SHEET_SIZE = 18;
    private const int IDLE_ANIMATION_FRAMERATE = 3;

    public BotMaster(Game game, Vector2 position)
        : base(game, position)
    {
        screenPosition = new Vector2(
            position.X,
            Engine.screenHeight - World.worldHeight + position.Y
        );

        var IdleAnimation = new SpriteSheetAnimation(
            this,
            "Idle",
            game.Content.Load<Texture2D>(IDLE_ANIMATION_PATH),
            CELL_SPRITE_SHEET_SIZE,
            CELL_SPRITE_SHEET_SIZE,
            IDLE_ANIMATION_FRAMERATE
        );
        _animations = new("Idle", IdleAnimation);
    }

    public override void Draw(GameTime gameTime)
    {
        Rectangle destinationRectangle = new(
            (int)screenPosition.X,
            (int)screenPosition.Y,
            Engine.TILE_SIZE,
            Engine.TILE_SIZE
        );

        _animations.Draw(destinationRectangle);
    }

    public override void Update(GameTime gameTime)
    {
        _animations.Update(gameTime);
    }
}
