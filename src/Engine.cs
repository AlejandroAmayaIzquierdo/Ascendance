using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using nx.entity;
using nx.tile;
using TiledSharp;

namespace nx;

public class Engine : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Matrix matrix;
    private TmxMap map;
    private TileManager mapManager;
    private List<Rectangle> collisionObjects;

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
        _graphics.PreferredBackBufferWidth = 464 * 2;//Making the window size twice our tilemap size
        _graphics.PreferredBackBufferHeight = 304 * 2;
        _graphics.ApplyChanges();

        var Width = _graphics.PreferredBackBufferWidth;
        var Height = _graphics.PreferredBackBufferHeight;
        var WindowSize = new Vector2(Width, Height);
        var mapSize = new Vector2(464, 304);//Our tile map size
        matrix = Matrix.CreateScale(new Vector3(WindowSize / mapSize, 1));

        Debug.WriteLine(matrix.ToString());
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        map = new TmxMap("Content/map.tmx");


        FileStream fileStream = new("Content/sheet.png", FileMode.Open);
        var tileset = Texture2D.FromStream(GraphicsDevice, fileStream);
        fileStream.Dispose();
        var tileWidth = map.Tilesets[0].TileWidth;
        var tileHeight = map.Tilesets[0].TileHeight;
        var TileSetTilesWide = tileset.Width / tileWidth;
        mapManager = new TileManager(_spriteBatch, map, tileset, TileSetTilesWide, tileWidth, tileHeight);

        collisionObjects = new List<Rectangle>();
        foreach (var o in map.ObjectGroups["Platforms"].Objects)
        {
            collisionObjects.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
        }

        player = new(this, new Vector2(0, 0), "Content/kevin.png");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        player.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(//All of these need to be here :(
            SpriteSortMode.Deferred,
            samplerState: SamplerState.PointClamp,
            effect: null,
            blendState: null,
            rasterizerState: null,
            depthStencilState: null,
            transformMatrix: matrix/*<-This is the main thing*/);

        mapManager.Draw();

        player.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
