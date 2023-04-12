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

        float sensitivity = 0.5f;
        float movementSpeed = 10;

        public float theta = 0;
        public float phi = 0;

        public Camera(Vector3 startPosition)
        {
            position = startPosition;
        }

        public void UpdateCamera(MouseState state, Vector2i viewportSize)
        {
            float deltaX = state.Delta.X;
            float deltaY = state.Delta.Y;
            theta -= deltaX * sensitivity;
            phi -= deltaY * sensitivity;

            if (theta < 0) theta += 360;
            else if (theta > 360) theta -= 360;

            phi = Math.Clamp(phi, -89, 89);

            Console.WriteLine("Theta: " + theta + " - " + "Phi: " + phi);

            this.forward = new Vector3(MathF.Cos((float)MathHelper.DegreesToRadians(theta)) * MathF.Cos((float)MathHelper.DegreesToRadians(phi)), 
                                       MathF.Sin((float)MathHelper.DegreesToRadians(theta)) * MathF.Cos((float)MathHelper.DegreesToRadians(phi)), 
                                       MathF.Sin((float)MathHelper.DegreesToRadians(phi)));

            this.right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitZ));
            this.up = Vector3.Normalize(Vector3.Cross(right, forward));
        }

        public void ForwardBackward(float direction, float delta)
        {
            this.position += movementSpeed * forward * direction * delta;
        }

        public void RightLeft(float direction, float delta)
        {
            this.position += movementSpeed * right * direction * delta;
        }

        public void UpDown(float direction, float delta)
        {
            this.position += movementSpeed * Vector3.UnitZ * direction * delta;
        }
    }
}