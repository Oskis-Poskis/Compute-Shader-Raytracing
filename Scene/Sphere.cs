using OpenTK.Mathematics;

namespace Tracer.SceneDesc
{
    public class Sphere 
    {
        public Vector3 center = Vector3.Zero;
        public float radius = 1;
        public Vector3 color = Vector3.One;

        public Sphere(Vector3 c, float r, Vector3 col)
        {
            this.center = c;
            this.radius = r;
            this.color = col;
        }
    }
}