using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nx.tile;
using TiledSharp;

namespace nx.world;

public class LevelManager
{
    private Game _game;
    private Camera2D _camera;
    private int _worldIndex = 0;
    private TileManager? _previousLevel;
    private TileManager? _currentLevel;
    private TileManager? _nextLevel;

    public TileManager? CurrentLevel => _currentLevel;

    public LevelManager(Game game, Camera2D camera)
    {
        _game = game;
        _camera = camera;

        _currentLevel = LoadLevel(_worldIndex);
    }

    public void NextLevel()
    {
        // 1. Mover current a previous
        _previousLevel = _currentLevel;

        // 2. Si ya teníamos el siguiente nivel pre-cargado, úsalo
        if (_nextLevel is not null)
        {
            _currentLevel = _nextLevel;
            _nextLevel = null; // Limpiamos para cargar uno nuevo después
        }
        else
        {
            // 3. Si no había siguiente nivel, cargar el actual
            _worldIndex++;
            _currentLevel = LoadLevel(_worldIndex);
        }

        // 4. Pre-cargar el siguiente nivel para la próxima vez
        _nextLevel = LoadLevel(_worldIndex + 1);
    }

    public TileManager? LoadLevel(int index)
    {
        WorldData? world = WorldData.GetWorld(index);

        if (world is null)
            return null;

        TmxMap map = new(world.mapUri);

        var tileset = _game.Content.Load<Texture2D>(world.tileSetUri);

        var tileWidth = map.Tilesets[0].TileWidth;
        var tileHeight = map.Tilesets[0].TileHeight;
        var TileSetTilesWide = tileset.Width / tileWidth;

        TileManager level = new(
            Engine.SpriteBatch!,
            map,
            tileset,
            TileSetTilesWide,
            tileWidth,
            tileHeight,
            _camera
        );

        return level;
    }
}
