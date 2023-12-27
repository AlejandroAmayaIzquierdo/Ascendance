using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace nx.scene;

public class MenuScene : IScene
{

    private Engine engine;
    private List<string> options = (new string[] { "New Game", "FullScreen", "Quit" }).ToList<string>();

    private int optionSelected = 0;

    public MenuScene(Game game)
    {
        engine = (Engine)game;
    }

    public void Draw(GameTime gameTime)
    {
        var position = Engine.SCREEN_CENTER_Y;
        var optionIndex = 0;
        options.ForEach(e =>
        {
            Engine.SpriteBatch.DrawString(engine.font, e, new Vector2(Engine.SCREEN_CENTER_X - 20, position), optionIndex == optionSelected ? Color.White : Color.Gray);
            position += 20;
            optionIndex++;
        });
    }

    public void Update(GameTime gameTime)
    {
        if (KeyboardUtil.IsKeyPressed(Keys.S, true))
        {

            if (optionSelected < options.Count - 1)
                optionSelected++;
        }
        else if (KeyboardUtil.IsKeyPressed(Keys.W, true))
        {
            if (optionSelected > 0)
                optionSelected--;
        }

        if (KeyboardUtil.IsKeyPressed(Keys.Space, true))
        {
            switch (options[optionSelected])
            {
                case "New Game":
                    engine.initializeWorld();
                    break;
                case "Quit":
                    engine.Exit();
                    break;
                case "FullScreen":
                    engine._graphics.IsFullScreen = !engine._graphics.IsFullScreen;
                    if (engine._graphics.IsFullScreen)
                    {
                        GraphicsAdapter adapter = GraphicsAdapter.DefaultAdapter;
                        DisplayMode displayMode = adapter.CurrentDisplayMode;

                        engine.loadScreenByResolution(displayMode.Width, displayMode.Height, true);
                    }
                    else
                        engine.loadScreenByResolution();


                    break;
                default:
                    break;
            }
        }
    }
}