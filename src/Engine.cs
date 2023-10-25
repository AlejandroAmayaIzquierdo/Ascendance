using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using nx.world;
using nx.util;


namespace nx;

public class Engine : Game
{

    public static readonly DEBUG_TYPE[] DEBUG = { DEBUG_TYPE.SOFTWARE };
    public const int originalTileSize = 16; //16 x 16 tile
    public const int scale = 2;

    public const int TILE_SIZE = originalTileSize * scale; //48 x 48 tile
    public const int maxScreenCol = 18;
    public const int maxScreenRow = 28;
    public static int screenWidth = TILE_SIZE * maxScreenCol; // 768 pixels
    public static int screenheigth = TILE_SIZE * maxScreenRow; // 576 pixels

    public static int SCREEN_CENTER_X = Engine.screenWidth / 2 - (Engine.TILE_SIZE / 2);
    public static int SCREEN_CENTER_Y = Engine.screenheigth / 2 - (Engine.TILE_SIZE / 2);

    public static int FULSCREEN_OFFSET_X = Engine.screenWidth / 2 - (Engine.TILE_SIZE / 2);



    public static Point viewport;
    public GraphicsDeviceManager _graphics;
    public static SpriteBatch SpriteBatch;
    private Matrix matrix;
    public World world;

    private Effect _firstShader;


    private SpriteFont font;
    private FrameCounter frameCounter;


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
        loadScreenByResolution();
        frameCounter = new();


        base.Initialize();
    }
    private void loadScreenByResolution(int _screenWidth = TILE_SIZE * maxScreenCol, int _screenheigth = TILE_SIZE * maxScreenRow, bool isFullScreen = false)
    {
        screenWidth = _screenWidth;
        screenheigth = _screenheigth;
        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenheigth;


        SCREEN_CENTER_X = screenWidth / 2 - (TILE_SIZE / 2);
        SCREEN_CENTER_Y = screenheigth / 2 - (TILE_SIZE / 2);

        Debug.WriteLine(SCREEN_CENTER_X + " " + SCREEN_CENTER_Y);

        FULSCREEN_OFFSET_X = isFullScreen ? screenWidth / 2 - World.worldWidth / 2 : 0;

        viewport = new(screenWidth, screenheigth);

        var Width = _graphics.PreferredBackBufferWidth;
        var Height = _graphics.PreferredBackBufferHeight;
        var WindowSize = new Vector2(Width, Height);
        var mapSize = new Vector2(screenWidth, screenheigth);
        matrix = Matrix.CreateScale(new Vector3(WindowSize / mapSize, 1));



        _graphics.ApplyChanges();

    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        _firstShader = Content.Load<Effect>("assets/Effects/blackAndWhite");
        font = Content.Load<SpriteFont>("assets/Fonts/Minecraft");


        world = new World(this, WorldData.GetWorld(Worlds.START_LEVEL));

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardUtil.GetState();

        if (KeyboardUtil.IsKeyPressed(Keys.F, true))
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;

            if (_graphics.IsFullScreen)
            {
                GraphicsAdapter adapter = GraphicsAdapter.DefaultAdapter;
                DisplayMode displayMode = adapter.CurrentDisplayMode;

                loadScreenByResolution(displayMode.Width, displayMode.Height, true);

            }
            else
                loadScreenByResolution();


            world.getMainCamera().Initialize();

            Debug.WriteLine(SCREEN_CENTER_X);
        }

        world.Update(gameTime);

        frameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

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


        world.Draw(gameTime);

        if (DEBUG.Contains(DEBUG_TYPE.SOFTWARE))
        {
            SpriteBatch.DrawString(font, "FPS: " + frameCounter.CurrentFramesPerSecond.ToString("0.0") + " | " + "AVG: " + frameCounter.AverageFramesPerSecond.ToString("0.0"), Vector2.Zero, Color.White);
            SpriteBatch.DrawString(font, "RAM: " + (Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024)).ToString("0.0 MB"), new Vector2(0, 20), Color.White);
            SpriteBatch.DrawString(font, "Resolution: " + viewport.ToString() + " " + new Vector2(SCREEN_CENTER_X, SCREEN_CENTER_Y).ToString(), new Vector2(0, 40), Color.White);
        }


        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
