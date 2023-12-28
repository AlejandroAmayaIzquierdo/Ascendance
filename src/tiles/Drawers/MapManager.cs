

using Microsoft.Xna.Framework.Graphics;

public class MapManager : ITileDrawer
{
    private SpriteBatch spriteBatch;

    public MapManager(SpriteBatch _spriteBatch)
    {
        spriteBatch = _spriteBatch;
    }


    public void Draw()
    {
        throw new System.NotImplementedException();
    }
}