using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.entity;
using nx.tile;
using SharpMath2;
using TiledSharp;

namespace nx;

public class Engine : Game
{

    public const int originalTileSize = 16; //16 x 16 tile
    public const int scale = 2;

    public const int TILE_SIZE = originalTileSize * scale; //48 x 48 tile
    public const int maxScreenCol = 29;
    public const int maxScreenRow = 19;
    public const int screenWidth = TILE_SIZE * maxScreenCol; // 768 pixels
    public const int screenheigth = TILE_SIZE * maxScreenRow; // 576 pixels

    public const int SCREEN_CENTER_X = Engine.screenWidth / 2 - (Engine.TILE_SIZE / 2);
    public const int SCREEN_CENTER_Y = Engine.screenheigth / 2 - (Engine.TILE_SIZE / 2);
    public static Point viewport;

    public GraphicsDeviceManager _graphics;
    public static SpriteBatch SpriteBatch;

    private Matrix matrix;
    private TmxMap map;
    private TileManager mapManager;

    private CollisionManager collisionManager;

    private Player player;



    public Engine()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";


        IsMouseVisible = true;
        this.Initialize();
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenheigth;
        _graphics.ApplyChanges();


        viewport = new(screenWidth, screenheigth);

        var Width = _graphics.PreferredBackBufferWidth;
        var Height = _graphics.PreferredBackBufferHeight;
        var WindowSize = new Vector2(Width, Height);
        var mapSize = new Vector2(screenWidth + Engine.TILE_SIZE, screenheigth);
        matrix = Matrix.CreateScale(new Vector3(WindowSize / mapSize, 1));


        base.Initialize();
    }
    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        map = new TmxMap("Content/map.tmx");


        FileStream fileStream = new("Content/sheet.png", FileMode.Open);
        var tileset = Texture2D.FromStream(GraphicsDevice, fileStream);
        fileStream.Dispose();
        var tileWidth = map.Tilesets[0].TileWidth;
        var tileHeight = map.Tilesets[0].TileHeight;
        var TileSetTilesWide = tileset.Width / tileWidth;
        mapManager = new TileManager(SpriteBatch, map, tileset, TileSetTilesWide, tileWidth, tileHeight);

        collisionManager = new CollisionManager(map.ObjectGroups["Platforms"]);

        player = Player.GetInstance(this, new Vector2(0, 100), "Content/kevin.png");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();




        //collisionManager.Update();


        Vector2 initPos = player.position;
        player.Update(gameTime);
        bool isColliding = collisionManager.Update();
        if (isColliding)
        {
            player.position = initPos;
            player.isGrounded = true;
        }
        else
        {
            player.isGrounded = false;
        }
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin(//All of these need to be here :(
            SpriteSortMode.Deferred,
            samplerState: SamplerState.PointClamp,
            effect: null,
            blendState: null,
            rasterizerState: null,
            depthStencilState: null,
            transformMatrix: matrix/*<-This is the main thing*/);

        mapManager.Draw();

        player.Draw(gameTime);

        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
