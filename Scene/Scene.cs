using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Tracer.Common;
using System;

namespace Tracer.SceneDesc
{
    public class Scene
    {
        public Sphere[] spheres;
        public Camera camera;
        private static List<float> sphereData = new List<float>();
        static int sphereDataTexture;

        public Scene()
        {
            this.spheres = new Sphere[32];
            for (int i = 0; i < 32; i++)
            {
                Vector3 center = new Vector3((float)RandomNumber(3, 10), (float)RandomNumber(-5, 5), (float)RandomNumber(-5, 5));
                float radius = (float)RandomNumber(0.1, 2.0);
                Vector3 color = new Vector3((float)RandomNumber(0, 1), (float)RandomNumber(0, 1), (float)RandomNumber(0, 1));

                this.spheres[i] = new Sphere(center, radius, color);
            }

            this.camera = new Camera(Vector3.Zero);
        }

        public static void prepareScene(Scene scene, ref ComputeShader raytracingShader)
        {
            raytracingShader.Use();

            raytracingShader.SetVector3("viewer.position", scene.camera.position);
            raytracingShader.SetVector3("viewer.forward", scene.camera.forward);
            raytracingShader.SetVector3("viewer.right", scene.camera.right);
            raytracingShader.SetVector3("viewer.up", scene.camera.up);

            raytracingShader.SetFloat("sphereCount", scene.spheres.Length);

            for (int i = 0; i < 32; i++)
            {
                recordSphere(i, scene.spheres[i]);
            }

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, sphereDataTexture);
            byte[] byteArray = new byte[sphereData.Count * sizeof(float)];
            System.Buffer.BlockCopy(sphereData.ToArray(), 0, byteArray, 0, byteArray.Length);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 2, 1024, 0, PixelFormat.Rgba, PixelType.Float, byteArray);
            GL.BindImageTexture(1, sphereDataTexture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
        }

        public static void renderScene(int framebufferTexture, Vector2i viewportSize)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindImageTexture(0, framebufferTexture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.DispatchCompute(viewportSize.X, viewportSize.Y, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            GL.BindImageTexture(0, 0, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
        }

        public static void CreateResourceMemory()
        {
            // (cx cy cz r) (r g b _)
            for (int i = 0; i < 1024; i++)
            {
                for (int attribute = 0; attribute < 8; attribute++)
                {
                    sphereData.Add(0.0f);
                }
            }

            sphereDataTexture = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, sphereDataTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            byte[] byteArray = new byte[sphereData.Count * sizeof(float)];
            System.Buffer.BlockCopy(sphereData.ToArray(), 0, byteArray, 0, byteArray.Length);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 2, 1024, 0, PixelFormat.Rgba, PixelType.Float, byteArray);
        }

        private static void recordSphere(int i, Sphere _sphere)
        {
            sphereData[8 * i] =     _sphere.center[0];
            sphereData[8 * i + 1] = _sphere.center[1];
            sphereData[8 * i + 2] = _sphere.center[2];

            sphereData[8 * i + 3] = _sphere.radius;

            sphereData[8 * i + 4] = _sphere.color[0];
            sphereData[8 * i + 5] = _sphere.color[1];
            sphereData[8 * i + 6] = _sphere.color[2];            
        }

        private static double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
