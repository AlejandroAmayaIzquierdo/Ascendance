using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

public class KeyboardUtil
{
    private static KeyboardState currentKeyState;
    private static KeyboardState previousKeyState;

    public KeyboardUtil()
    {
        currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        GetState();
    }

    public static KeyboardState GetState()
    {
        previousKeyState = currentKeyState;
        currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        return currentKeyState;
    }

    public static bool IsPressed(Keys key)
    {
        return currentKeyState.IsKeyDown(key);
    }

    public static bool HasBeenPressed(Keys key)
    {
        return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
    }
    public static bool IsKeyPressed(Keys key, bool oneShot = false)
    {
        if (!oneShot) return currentKeyState.IsKeyDown(key);
        return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
    }
}