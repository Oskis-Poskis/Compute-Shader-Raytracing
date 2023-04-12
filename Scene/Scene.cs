using OpenTK.Mathematics;
using Tracer.Common;

namespace Tracer.SceneDesc
{
    public class Scene
    {
        public Sphere[] spheres;
        public Camera camera;

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
                raytracingShader.SetVector3($"spheres[{i}].center", scene.spheres[i].center);
                raytracingShader.SetFloat($"spheres[{i}].radius", scene.spheres[i].radius);
                raytracingShader.SetVector3($"spheres[{i}].color", scene.spheres[i].color);
            }
        }

        private static double RandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
