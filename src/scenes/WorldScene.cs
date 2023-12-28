using Microsoft.Xna.Framework;
using nx.entity;
using nx.world;

namespace nx.scene;

public class WorldScene : IScene
{

    public readonly World world;
    private readonly Camera2D mainCamera;

    public WorldScene(Game game, WorldData worldData = null)
    {
        mainCamera = new(Vector2.Zero);
        if (worldData != null)
            world = new(game, mainCamera, worldData);
        else
            world = new(game, mainCamera, null, WORLD_GENERATION.WAVE_FUNCTION_COLLAPSE);

        mainCamera.Initialize();
    }

    public Camera2D getMainCamera() { return mainCamera; }

    public void Draw(GameTime gameTime)
    {
        world.Draw(gameTime);
    }

    public void Update(GameTime gameTime)
    {

        world.Update(gameTime);
        mainCamera.setPosition(Player.GetInstance().position);
    }
}