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
        private static List<float> noiseData = new List<float>();
        static int sphereDataTexture;
        static int noiseTexture;

        public Scene()
        {
            this.spheres = new Sphere[32];
            for (int i = 0; i < 32; i++)
            {
                Vector3 center = new Vector3((float)RandomNumber(3, 10), (float)RandomNumber(-5, 5), (float)RandomNumber(-5, 5));
                float radius = (float)RandomNumber(0.4, 1.5);
                Vector3 color = new Vector3((float)RandomNumber(0.3, 1), (float)RandomNumber(0.3, 1), (float)RandomNumber(0.3, 1));
                float roughness = (float)RandomNumber(0.1, 0.9);

                this.spheres[i] = new Sphere(center, radius, color, roughness);
            }

            this.camera = new Camera(Vector3.UnitX * -5);
        }

        public static void prepareScene(Scene scene, ref ComputeShader raytracingShader)
        {
            raytracingShader.Use();

            raytracingShader.SetVector3("viewer.position", scene.camera.position);
            raytracingShader.SetVector3("viewer.forward", scene.camera.forward); 
            raytracingShader.SetVector3("viewer.right", scene.camera.right);
            raytracingShader.SetVector3("viewer.up", scene.camera.up);
            raytracingShader.SetFloat("sphereCount", scene.spheres.Length);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindImageTexture(1, sphereDataTexture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindImageTexture(2, noiseTexture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
        }

        public static void renderScene(int framebufferTexture, Vector2i viewportSize)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindImageTexture(0, framebufferTexture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.DispatchCompute(viewportSize.X, viewportSize.Y, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            GL.BindImageTexture(0, 0, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
        }

        public static void CreateResourceMemory(Scene scene)
        {
            // (cx cy cz r) (r g b roughness)
            for (int i = 0; i < 32; i++)
            {
                for (int attribute = 0; attribute < 8; attribute++)
                {
                    sphereData.Add(0.0f);
                }

                recordSphere(i, scene.spheres[i]);
            }

            sphereDataTexture = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, sphereDataTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 2, 32, 0, PixelFormat.Rgba, PixelType.Float, sphereData.ToArray());
        }

        public static void CreateNoiseTexture(Vector2i viewportSize)
        {
            for (int y = 0; y < viewportSize.Y; y++)
            {
                for (int x = 0; x < 4 * viewportSize.X; x++)
                {
                    float radius = (float)RandomNumber(0.0, 0.99);
                    float theta = (float)RandomNumber(0.0, 2 * Math.PI);
                    float phi = (float)RandomNumber(0.0, Math.PI);
                    noiseData.Add(radius * (float)Math.Cos(theta) * (float)Math.Cos(phi));
                    noiseData.Add(radius * (float)Math.Sin(theta) * (float)Math.Cos(phi));
                    noiseData.Add(radius * (float)Math.Cos(phi));
                    noiseData.Add(0);
                }
            }

            noiseTexture = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, noiseTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 4 * viewportSize.X, viewportSize.Y, 0, PixelFormat.Rgba, PixelType.Float, noiseData.ToArray());
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
            sphereData[8 * i + 7] = _sphere.roughness;  
        }

        private static double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
