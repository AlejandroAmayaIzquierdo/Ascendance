using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nx.animation;

public interface IAnimation
{
    public void Update(GameTime gameTime);

    public void Draw(Rectangle rectangle);
}