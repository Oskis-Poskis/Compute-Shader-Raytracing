using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Tracer.SceneDesc
{
    public class Camera
    {
        public Vector3 position = Vector3.Zero;
        public Vector3 forward = Vector3.UnitX;
        public Vector3 right = Vector3.UnitY;
        public Vector3 up = Vector3.UnitZ;
        float sensitivity = 1;

        public float theta = 0;
        public float phi = 0;

        public Camera(Vector3 startPosition)
        {
            position = startPosition;
        }

        public void UpdateCamera(MouseState state)
        {
            this.forward = new Vector3(MathF.Cos((float)MathHelper.DegreesToRadians(theta)) * MathF.Cos((float)MathHelper.DegreesToRadians(phi)), 
                                       MathF.Sin((float)MathHelper.DegreesToRadians(theta)) * MathF.Cos((float)MathHelper.DegreesToRadians(phi)), 
                                       MathF.Sin((float)MathHelper.DegreesToRadians(theta)));

            this.right = Vector3.Cross(forward, Vector3.UnitZ);
            this.up = Vector3.Cross(right, forward);
        }
    }
}