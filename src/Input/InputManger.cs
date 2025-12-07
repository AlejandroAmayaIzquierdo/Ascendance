using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace nx.input;

public class InputManager
{
    private KeyboardState currentKeyboardState;
    private KeyboardState previousKeyboardState;

    private GamePadState currentGamePadState;
    private GamePadState previousGamePadState;

    public void Update()
    {
        previousGamePadState = currentGamePadState;
        previousKeyboardState = currentKeyboardState;

        currentGamePadState = GamePad.GetState(PlayerIndex.One);
        currentKeyboardState = Keyboard.GetState();
    }

    public bool IsMovingRight() =>
        currentKeyboardState.IsKeyDown(Keys.D) || currentGamePadState.ThumbSticks.Left.X > 0.3f;

    public bool IsMovingLeft() =>
        currentKeyboardState.IsKeyDown(Keys.A) || currentGamePadState.ThumbSticks.Left.X < -0.3f;

    public bool IsJumpPressed() =>
        currentKeyboardState.IsKeyDown(Keys.Space) || currentGamePadState.IsButtonDown(Buttons.A);

    public bool IsJumpJustPressed() =>
        (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
        || (
            currentGamePadState.IsButtonDown(Buttons.A)
            && previousGamePadState.IsButtonUp(Buttons.A)
        );

    public bool IsJumpJustReleased() =>
        (currentKeyboardState.IsKeyUp(Keys.Space) && previousKeyboardState.IsKeyDown(Keys.Space))
        || (
            currentGamePadState.IsButtonUp(Buttons.A)
            && previousGamePadState.IsButtonDown(Buttons.A)
        );
}
