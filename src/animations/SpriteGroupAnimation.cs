using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nx.animation;


public class SpriteGroupAnimation
{
    private readonly List<SpriteSheetAnimation> groups = new();

    private SpriteSheetAnimation _currentAnimation;

    private SpriteEffects FlipDirection;

    public SpriteGroupAnimation(string defaultGroup, params SpriteSheetAnimation[] animations)
    {
        groups = animations.OfType<SpriteSheetAnimation>().ToList();
        SetGroupAnimation(defaultGroup);
    }
    public void Update(GameTime gameTime)
    {
        _currentAnimation.Update(gameTime);
    }

    public void Draw(Rectangle rectangle)
    {
        _currentAnimation.Draw(rectangle);
    }

    public void Flip(SpriteEffects flipDirection)
    {
        groups.ForEach(e =>
        {
            e.Flip(flipDirection);
        });

        FlipDirection = flipDirection;
    }

    public bool isFlip()
    {
        return FlipDirection == SpriteEffects.FlipHorizontally;
    }

    public void SetGroupAnimation(string key)
    {
        _currentAnimation = groups.First(t => t.SpriteSheetName == key);
    }
    public SpriteSheetAnimation getCurrentAnimation() { return _currentAnimation; }


}