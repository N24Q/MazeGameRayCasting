using MazeGame_v2;
using MazeGameRayCasting;
using System;
using System.Threading;

class Program
{
    static void Main()
    {
        //Console.WriteLine("ENTER FULL SCREEN BEFORE YOU BEGIN");
        //Console.Read();
        //Console.Clear();
        //Rendering2D render2D = new Rendering2D();
        //MazeGenerator maze = new MazeGenerator(53, 53);
        //render2D.RenderMaze2D(maze.GetMaze());
        Console.Title = "3D Maze Game with CP437 Shading";
        try
        {
            Console.WindowWidth = 120;
            Console.WindowHeight = 40;
            Console.BufferWidth = 120;
            Console.BufferHeight = 40;
        }
        catch (Exception)
        {
            Console.WriteLine("Unable to resize console window. Game will continue with current size.");
            Thread.Sleep(2000);
        }
        // Intro message with CP437 characters demonstration
        Console.Clear();
        Console.WriteLine("Welcome to 3D Maze Game!");
        Console.WriteLine("Using CP437 Block Characters for Enhanced Visuals: ░ ▒ ▓ █");
        Console.WriteLine("\nControls:");
        Console.WriteLine("  W - Move forward");
        Console.WriteLine("  S - Move backward");
        Console.WriteLine("  A - Turn left");
        Console.WriteLine("  D - Turn right");
        Console.WriteLine("  ESC - Exit game");
        Console.WriteLine("\nPress any key to start the game...");
        Console.ReadKey(true);
        // Generate maze
        int mazeSize = 29;
        MazeGenerator mazeGenerator = new MazeGenerator(mazeSize, mazeSize);
        int[,] maze = mazeGenerator.GetMaze();
        // Create ray caster with console dimensions
        int screenWidth = Console.WindowWidth;
        int screenHeight = Console.WindowHeight - 1; // Leave one row for potential messages
        Rendering3D rayCaster = new Rendering3D(maze, screenWidth, screenHeight);

        // Initialize input processing - THIS IS THE MISSING LINE
        rayCaster.InitializeInputProcessing();

        // Game loop
        DateTime lastUpdate = DateTime.Now;
        int frameCount = 0;
        double fps = 0;
        bool isRunning = true;
        while (isRunning)
        {
            // Check for Escape key to exit
            if (KeyboardInput.IsKeyPressed(KeyboardInput.VK_ESCAPE))
            {
                isRunning = false;
            }
            // Render the scene (movement is handled by the input processing task)
            rayCaster.Render();
            // Small delay to prevent maxing out CPU
            Thread.Sleep(16); // ~60 FPS

            // FPS calculation (optional)
            frameCount++;
            TimeSpan elapsed = DateTime.Now - lastUpdate;
            if (elapsed.TotalSeconds >= 1.0)
            {
                fps = frameCount / elapsed.TotalSeconds;
                frameCount = 0;
                lastUpdate = DateTime.Now;

                // You could uncomment this to show FPS in the console title
                // Console.Title = $"3D Maze Game - FPS: {fps:F2}";
            }
        }
        // Clean up when exiting
        rayCaster.StopInputProcessing();
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine("Thanks for playing!");
        Thread.Sleep(1500);
    }
}

