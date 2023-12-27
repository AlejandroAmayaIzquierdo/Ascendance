namespace nx.scene;


using Microsoft.Xna.Framework;

public interface IScene
{

    /**
     * Updates the scene
     * @param delta Frame delta time
     */
    void Update(GameTime gameTime);

    /**
     * Draws the scene
     * @param gc Graphics
     */
    void Draw(GameTime gameTime);

}