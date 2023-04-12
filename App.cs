using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

using Tracer.Stats;
using Tracer.Common;
using Tracer.SceneDesc;
using static Tracer.Common.Framebuffers;
using static Tracer.SceneDesc.Scene;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
        Vector2i viewportSize;

        Shader FBOshader;
        ComputeShader RaytracingShader;
        int framebufferTexture;
        int FBO;

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
            CreateResourceMemory();
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

            prepareScene(scene, ref RaytracingShader);
            renderScene(framebufferTexture, viewportSize);            

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            RenderFramebuffer(ref FBOshader, framebufferTexture);

            stats.Count(args);
            Title = "FPS: " + Convert.ToInt32(stats.fps);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            viewportSize = e.Size;
            ResizeFBO(viewportSize, ref framebufferTexture);
        }
    }
}