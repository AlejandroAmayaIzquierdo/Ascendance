using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace nx.UI;

/// <summary>
/// ImGui renderer for MonoGame that renders directly in the game window
/// </summary>
public class ImGuiRenderer
{
    private Game _game;
    private GraphicsDevice _graphicsDevice;
    private BasicEffect _effect;
    private RasterizerState _rasterizerState;

    private byte[] _vertexData = new byte[10000];
    private VertexBuffer _vertexBuffer;
    private int _vertexBufferSize = 10000;

    private byte[] _indexData = new byte[2000];
    private IndexBuffer _indexBuffer;
    private int _indexBufferSize = 2000;

    private Dictionary<IntPtr, Texture2D> _loadedTextures = new();
    private int _textureId = 1;
    private IntPtr _fontTextureId;

    private int _scrollWheelValue;
    private Keys[] _allKeys = Enum.GetValues<Keys>();

    public ImGuiRenderer(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _graphicsDevice = game.GraphicsDevice;

        _rasterizerState = new RasterizerState()
        {
            CullMode = CullMode.None,
            DepthBias = 0,
            FillMode = FillMode.Solid,
            MultiSampleAntiAlias = false,
            ScissorTestEnable = true,
            SlopeScaleDepthBias = 0,
        };
    }

    public void Initialize()
    {
        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        var io = ImGui.GetIO();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

        CreateDeviceResources();

        RebuildFontAtlas();
    }

    private void CreateDeviceResources()
    {
        _effect = new BasicEffect(_graphicsDevice);
        _effect.FogEnabled = false;
        _effect.TextureEnabled = true;
        _effect.LightingEnabled = false;
        _effect.VertexColorEnabled = true;
        _effect.World = Matrix.Identity;
        _effect.View = Matrix.Identity;
        _effect.Projection = Matrix.Identity;

        _vertexBuffer = new VertexBuffer(
            _graphicsDevice,
            VertexPositionColorTexture.VertexDeclaration,
            _vertexBufferSize,
            BufferUsage.None
        );

        _indexBuffer = new IndexBuffer(
            _graphicsDevice,
            IndexElementSize.SixteenBits,
            _indexBufferSize,
            BufferUsage.None
        );
    }

    private void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(
            out IntPtr pixels,
            out int width,
            out int height,
            out int bytesPerPixel
        );

        var texture = new Texture2D(_graphicsDevice, width, height, false, SurfaceFormat.Color);

        byte[] data = new byte[width * height * bytesPerPixel];
        Marshal.Copy(pixels, data, 0, data.Length);
        texture.SetData(data);

        if (_fontTextureId != IntPtr.Zero)
            UnbindTexture(_fontTextureId);

        _fontTextureId = BindTexture(texture);
        io.Fonts.SetTexID(_fontTextureId);
        io.Fonts.ClearTexData();
    }

    public IntPtr BindTexture(Texture2D texture)
    {
        var id = new IntPtr(_textureId++);
        _loadedTextures.Add(id, texture);
        return id;
    }

    public void UnbindTexture(IntPtr textureId)
    {
        _loadedTextures.Remove(textureId);
    }

    public void BeforeLayout(GameTime gameTime)
    {
        var io = ImGui.GetIO();

        io.DisplaySize = new System.Numerics.Vector2(
            _graphicsDevice.PresentationParameters.BackBufferWidth,
            _graphicsDevice.PresentationParameters.BackBufferHeight
        );

        io.DisplayFramebufferScale = System.Numerics.Vector2.One;
        io.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        UpdateInput();

        ImGui.NewFrame();
    }

    public void AfterLayout()
    {
        ImGui.Render();
        RenderDrawData(ImGui.GetDrawData());
    }

    private void UpdateInput()
    {
        var io = ImGui.GetIO();

        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        // Mouse
        io.AddMousePosEvent(mouse.X, mouse.Y);
        io.AddMouseButtonEvent(0, mouse.LeftButton == ButtonState.Pressed);
        io.AddMouseButtonEvent(1, mouse.RightButton == ButtonState.Pressed);
        io.AddMouseButtonEvent(2, mouse.MiddleButton == ButtonState.Pressed);

        var scrollDelta = mouse.ScrollWheelValue - _scrollWheelValue;
        io.AddMouseWheelEvent(0, scrollDelta / 120f);
        _scrollWheelValue = mouse.ScrollWheelValue;

        // Keyboard
        foreach (var key in _allKeys)
        {
            if (TryMapKeys(key, out ImGuiKey imguiKey))
            {
                io.AddKeyEvent(imguiKey, keyboard.IsKeyDown(key));
            }
        }

        // Modifiers
        io.AddKeyEvent(
            ImGuiKey.ModCtrl,
            keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl)
        );
        io.AddKeyEvent(
            ImGuiKey.ModShift,
            keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift)
        );
        io.AddKeyEvent(
            ImGuiKey.ModAlt,
            keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt)
        );
        io.AddKeyEvent(
            ImGuiKey.ModSuper,
            keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows)
        );
    }

    private bool TryMapKeys(Keys key, out ImGuiKey result)
    {
        result = key switch
        {
            Keys.Back => ImGuiKey.Backspace,
            Keys.Tab => ImGuiKey.Tab,
            Keys.Enter => ImGuiKey.Enter,
            Keys.CapsLock => ImGuiKey.CapsLock,
            Keys.Escape => ImGuiKey.Escape,
            Keys.Space => ImGuiKey.Space,
            Keys.PageUp => ImGuiKey.PageUp,
            Keys.PageDown => ImGuiKey.PageDown,
            Keys.End => ImGuiKey.End,
            Keys.Home => ImGuiKey.Home,
            Keys.Left => ImGuiKey.LeftArrow,
            Keys.Up => ImGuiKey.UpArrow,
            Keys.Right => ImGuiKey.RightArrow,
            Keys.Down => ImGuiKey.DownArrow,
            Keys.PrintScreen => ImGuiKey.PrintScreen,
            Keys.Insert => ImGuiKey.Insert,
            Keys.Delete => ImGuiKey.Delete,
            Keys.D0 => ImGuiKey._0,
            Keys.D1 => ImGuiKey._1,
            Keys.D2 => ImGuiKey._2,
            Keys.D3 => ImGuiKey._3,
            Keys.D4 => ImGuiKey._4,
            Keys.D5 => ImGuiKey._5,
            Keys.D6 => ImGuiKey._6,
            Keys.D7 => ImGuiKey._7,
            Keys.D8 => ImGuiKey._8,
            Keys.D9 => ImGuiKey._9,
            Keys.A => ImGuiKey.A,
            Keys.B => ImGuiKey.B,
            Keys.C => ImGuiKey.C,
            Keys.D => ImGuiKey.D,
            Keys.E => ImGuiKey.E,
            Keys.F => ImGuiKey.F,
            Keys.G => ImGuiKey.G,
            Keys.H => ImGuiKey.H,
            Keys.I => ImGuiKey.I,
            Keys.J => ImGuiKey.J,
            Keys.K => ImGuiKey.K,
            Keys.L => ImGuiKey.L,
            Keys.M => ImGuiKey.M,
            Keys.N => ImGuiKey.N,
            Keys.O => ImGuiKey.O,
            Keys.P => ImGuiKey.P,
            Keys.Q => ImGuiKey.Q,
            Keys.R => ImGuiKey.R,
            Keys.S => ImGuiKey.S,
            Keys.T => ImGuiKey.T,
            Keys.U => ImGuiKey.U,
            Keys.V => ImGuiKey.V,
            Keys.W => ImGuiKey.W,
            Keys.X => ImGuiKey.X,
            Keys.Y => ImGuiKey.Y,
            Keys.Z => ImGuiKey.Z,
            Keys.F1 => ImGuiKey.F1,
            Keys.F2 => ImGuiKey.F2,
            Keys.F3 => ImGuiKey.F3,
            Keys.F4 => ImGuiKey.F4,
            Keys.F5 => ImGuiKey.F5,
            Keys.F6 => ImGuiKey.F6,
            Keys.F7 => ImGuiKey.F7,
            Keys.F8 => ImGuiKey.F8,
            Keys.F9 => ImGuiKey.F9,
            Keys.F10 => ImGuiKey.F10,
            Keys.F11 => ImGuiKey.F11,
            Keys.F12 => ImGuiKey.F12,
            Keys.NumPad0 => ImGuiKey.Keypad0,
            Keys.NumPad1 => ImGuiKey.Keypad1,
            Keys.NumPad2 => ImGuiKey.Keypad2,
            Keys.NumPad3 => ImGuiKey.Keypad3,
            Keys.NumPad4 => ImGuiKey.Keypad4,
            Keys.NumPad5 => ImGuiKey.Keypad5,
            Keys.NumPad6 => ImGuiKey.Keypad6,
            Keys.NumPad7 => ImGuiKey.Keypad7,
            Keys.NumPad8 => ImGuiKey.Keypad8,
            Keys.NumPad9 => ImGuiKey.Keypad9,
            Keys.Multiply => ImGuiKey.KeypadMultiply,
            Keys.Add => ImGuiKey.KeypadAdd,
            Keys.Subtract => ImGuiKey.KeypadSubtract,
            Keys.Decimal => ImGuiKey.KeypadDecimal,
            Keys.Divide => ImGuiKey.KeypadDivide,
            _ => ImGuiKey.None,
        };

        return result != ImGuiKey.None;
    }

    private unsafe void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0)
            return;

        var viewport = _graphicsDevice.Viewport;

        // Setup orthographic projection
        _effect.Projection = Matrix.CreateOrthographicOffCenter(
            0,
            viewport.Width,
            viewport.Height,
            0,
            -1,
            1
        );

        // Save old state
        var oldBlendState = _graphicsDevice.BlendState;
        var oldDepthStencilState = _graphicsDevice.DepthStencilState;
        var oldRasterizerState = _graphicsDevice.RasterizerState;
        var oldSamplerState = _graphicsDevice.SamplerStates[0];

        // Apply ImGui state
        _graphicsDevice.BlendState = BlendState.NonPremultiplied;
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        _graphicsDevice.RasterizerState = _rasterizerState;
        _graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

        // Render command lists
        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];

            // Resize buffers if needed
            if (
                cmdList.VtxBuffer.Size * VertexPositionColorTexture.VertexDeclaration.VertexStride
                > _vertexBufferSize
            )
            {
                _vertexBufferSize =
                    cmdList.VtxBuffer.Size
                    * VertexPositionColorTexture.VertexDeclaration.VertexStride
                    * 2;
                _vertexBuffer.Dispose();
                _vertexBuffer = new VertexBuffer(
                    _graphicsDevice,
                    VertexPositionColorTexture.VertexDeclaration,
                    _vertexBufferSize / VertexPositionColorTexture.VertexDeclaration.VertexStride,
                    BufferUsage.None
                );
                _vertexData = new byte[_vertexBufferSize];
            }

            if (cmdList.IdxBuffer.Size * sizeof(ushort) > _indexBufferSize)
            {
                _indexBufferSize = cmdList.IdxBuffer.Size * sizeof(ushort) * 2;
                _indexBuffer.Dispose();
                _indexBuffer = new IndexBuffer(
                    _graphicsDevice,
                    IndexElementSize.SixteenBits,
                    _indexBufferSize / sizeof(ushort),
                    BufferUsage.None
                );
                _indexData = new byte[_indexBufferSize];
            }

            // Copy vertex data
            for (int vtxIdx = 0; vtxIdx < cmdList.VtxBuffer.Size; vtxIdx++)
            {
                var vtx = cmdList.VtxBuffer[vtxIdx];

                var vertex = new VertexPositionColorTexture(
                    new Vector3(vtx.pos.X, vtx.pos.Y, 0),
                    new Color(vtx.col),
                    new Vector2(vtx.uv.X, vtx.uv.Y)
                );

                int offset = vtxIdx * VertexPositionColorTexture.VertexDeclaration.VertexStride;

                // Copy position (12 bytes: 3 floats)
                Buffer.BlockCopy(
                    BitConverter.GetBytes(vertex.Position.X),
                    0,
                    _vertexData,
                    offset,
                    4
                );
                Buffer.BlockCopy(
                    BitConverter.GetBytes(vertex.Position.Y),
                    0,
                    _vertexData,
                    offset + 4,
                    4
                );
                Buffer.BlockCopy(
                    BitConverter.GetBytes(vertex.Position.Z),
                    0,
                    _vertexData,
                    offset + 8,
                    4
                );
                offset += 12;

                // Copy color (4 bytes: RGBA)
                _vertexData[offset++] = vertex.Color.R;
                _vertexData[offset++] = vertex.Color.G;
                _vertexData[offset++] = vertex.Color.B;
                _vertexData[offset++] = vertex.Color.A;

                // Copy texture coordinates (8 bytes: 2 floats)
                Buffer.BlockCopy(
                    BitConverter.GetBytes(vertex.TextureCoordinate.X),
                    0,
                    _vertexData,
                    offset,
                    4
                );
                Buffer.BlockCopy(
                    BitConverter.GetBytes(vertex.TextureCoordinate.Y),
                    0,
                    _vertexData,
                    offset + 4,
                    4
                );
            }

            _vertexBuffer.SetData(
                _vertexData,
                0,
                cmdList.VtxBuffer.Size * VertexPositionColorTexture.VertexDeclaration.VertexStride
            );

            // Copy index data
            for (int idxIdx = 0; idxIdx < cmdList.IdxBuffer.Size; idxIdx++)
            {
                var idx = cmdList.IdxBuffer[idxIdx];
                BitConverter.GetBytes(idx).CopyTo(_indexData, idxIdx * sizeof(ushort));
            }

            _indexBuffer.SetData(_indexData, 0, cmdList.IdxBuffer.Size * sizeof(ushort));

            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            int vtxOffset = 0;
            int idxOffset = 0;

            for (int cmdIdx = 0; cmdIdx < cmdList.CmdBuffer.Size; cmdIdx++)
            {
                var cmd = cmdList.CmdBuffer[cmdIdx];

                if (cmd.UserCallback != IntPtr.Zero)
                {
                    // User callback not supported
                }
                else
                {
                    var texture = _loadedTextures[cmd.TextureId];
                    _effect.Texture = texture;

                    var clip = cmd.ClipRect;
                    _graphicsDevice.ScissorRectangle = new Rectangle(
                        (int)clip.X,
                        (int)clip.Y,
                        (int)(clip.Z - clip.X),
                        (int)(clip.W - clip.Y)
                    );

                    foreach (var pass in _effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _graphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            vtxOffset,
                            idxOffset,
                            (int)cmd.ElemCount / 3
                        );
                    }
                }

                idxOffset += (int)cmd.ElemCount;
            }

            vtxOffset += cmdList.VtxBuffer.Size;
        }

        // Restore old state
        _graphicsDevice.BlendState = oldBlendState;
        _graphicsDevice.DepthStencilState = oldDepthStencilState;
        _graphicsDevice.RasterizerState = oldRasterizerState;
        _graphicsDevice.SamplerStates[0] = oldSamplerState;
    }
}
