using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

using ImGuiNET;
using System.Runtime.InteropServices;

using Tracer.Stats;
using Tracer.Common;
using Tracer.SceneDesc;
using Tracer.UserInterface;
using static Tracer.Common.Framebuffers;
using static Tracer.SceneDesc.Scene;

namespace Tracer
{
    class Window : GameWindow
    {
        public Window(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Title = title,
                Size = new 
                Vector2i(width, height),
                WindowBorder = WindowBorder.Resizable,
                StartVisible = false,
                StartFocused = true,
                WindowState = WindowState.Normal,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 3),
            })
        {
            CenterWindow();
            viewportSize = this.Size;
            FBOshader = new("Shaders/fbo.vert", "Shaders/fbo.frag");
            RaytracingShader = new("Shaders/raytracer.comp");
        }

        StatCounter stats = new StatCounter();
        private ImGuiController ImGuiController;
        Vector2i viewportSize;

        Shader FBOshader;
        ComputeShader RaytracingShader;
        int numBounces = 1;
        int framebufferTexture;
        int FBO;

        System.Numerics.Vector3 lightPos = new(0);

        Scene scene = new();

        private static void OnDebugMessage(
            DebugSource source,     // Source of the debugging message.
            DebugType type,         // Type of the debugging message.
            int id,                 // ID associated with the message.
            DebugSeverity severity, // Severity of the message.
            int length,             // Length of the string in pMessage.
            IntPtr pMessage,        // Pointer to message string.
            IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
        {
            // In order to access the string pointed to by pMessage, you can use Marshal
            // class to copy its contents to a C# string without unsafe code. You can
            // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
            string message = Marshal.PtrToStringAnsi(pMessage, length);

            // The rest of the function is up to you to implement, however a debug output
            // is always useful.
            Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);

            // Potentially, you may want to throw from the function for certain severity
            // messages.
            if (type == DebugType.DebugTypeError)
            {
                throw new Exception(message);
            }
        }

        private static DebugProc DebugMessageDelegate = OnDebugMessage;

        protected override void OnLoad()
        {
            base.OnLoad();

            MakeCurrent();
            IsVisible = true;
            GL.ClearColor(new Color4(0.0f, 0.0f, 0.0f, 0.0f));

            VSync = VSyncMode.Off;

            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);

            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            SetupFBO(ref framebufferTexture, viewportSize);
            CreateResourceMemory(scene);
            // CreateNoiseTexture(viewportSize);

            ImGuiController = new ImGuiController(viewportSize.X, viewportSize.Y);
            UI.LoadTheme();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Viewport(0, 0, viewportSize.X, viewportSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (IsMouseButtonDown(MouseButton.Button2))
            {
                CursorState = CursorState.Grabbed;
                scene.camera.UpdateCamera(MouseState, viewportSize);
            }
            else CursorState = CursorState.Normal;

            if (IsKeyDown(Keys.W)) scene.camera.ForwardBackward(1, (float)args.Time);
            if (IsKeyDown(Keys.S)) scene.camera.ForwardBackward(-1, (float)args.Time);
            if (IsKeyDown(Keys.A)) scene.camera.RightLeft(-1, (float)args.Time);
            if (IsKeyDown(Keys.D)) scene.camera.RightLeft(1, (float)args.Time);
            if (IsKeyDown(Keys.E)) scene.camera.UpDown(1, (float)args.Time);
            if (IsKeyDown(Keys.Q)) scene.camera.UpDown(-1, (float)args.Time);

            prepareScene(scene, ref RaytracingShader);
            renderScene(framebufferTexture, viewportSize);
            
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);

            ImGuiController.Update(this, (float)args.Time);
            ImGui.DockSpaceOverViewport();

            ImGui.Begin("Scene");
            viewportSize = new(Convert.ToInt32(ImGui.GetContentRegionAvail().X), Convert.ToInt32(ImGui.GetContentRegionAvail().Y));
            ResizeFBO(viewportSize, ref framebufferTexture);
            ImGui.Image((IntPtr)framebufferTexture, new(viewportSize.X, viewportSize.Y), new(0, 1), new(1, 0), new(1, 1, 1, 1), new(0));
            ImGui.End();

            ImGui.Begin("Settings");
            if (ImGui.SliderInt("Bounces", ref numBounces, 1, 64)) RaytracingShader.SetInt("numBounces", numBounces);
            if (ImGui.SliderFloat3("LightPos", ref lightPos, -10, 10)) RaytracingShader.SetVector3("lightPos", new(lightPos.X, lightPos.Y, lightPos.Z));
            ImGui.End();

            ImGuiController.Render();

            stats.Count(args);
            Title = "FPS: " + Convert.ToInt32(stats.fps);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            ImGuiController.WindowResized(e.Width, e.Height);
            
            viewportSize = e.Size;
            ResizeFBO(viewportSize, ref framebufferTexture);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            ImGuiController.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            ImGuiController.MouseScroll(e.Offset);
        }
    }
}