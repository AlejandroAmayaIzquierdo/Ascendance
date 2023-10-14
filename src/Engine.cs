using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.entity;
using nx.tile;
using nx.world;
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
    public World world;

    //private Effect _firstShader;


    public Engine()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Content.RootDirectory = "Content";


        IsMouseVisible = true;
        Initialize();
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

        //_firstShader = Content.Load<Effect>("assets/Effects/first-shader");


        world = new World(this, WorldData.GetWorld(Worlds.START_LEVEL));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        world.Update(gameTime);


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        /*


        Matrix view = Matrix.Identity;

        int width = GraphicsDevice.Viewport.Width;
        int height = GraphicsDevice.Viewport.Height;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);

        _firstShader.Parameters["view_projection"].SetValue(view * projection);

        */


        SpriteBatch.Begin(//All of these need to be here :(
            SpriteSortMode.Deferred,
            samplerState: SamplerState.PointClamp,
            effect: null,
            blendState: null,
            rasterizerState: null,
            depthStencilState: null,
            transformMatrix: matrix/*<-This is the main thing*/);

        world.Draw(gameTime);

        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
