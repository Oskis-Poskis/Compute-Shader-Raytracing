namespace Tracer
{
    class Program
    {
        static void Main()
        {
            using Window game = new Window(1024, 1024, "Window");
            game.Run();
        }
    }
}