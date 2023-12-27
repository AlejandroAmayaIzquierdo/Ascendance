using Microsoft.Xna.Framework;

namespace nx.scene.components;


public interface IComponent
{
    public void Draw(GameTime gameTime);

    public void Execute();
}