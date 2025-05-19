using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGameRayCasting
{
    public class Rendering3D
    {
        private int[,] maze;
        private int screenWidth;
        private int screenHeight;
        private double posX, posY;      // Player position
        private double dirX, dirY;      // Player direction vector
        private double planeX, planeY;  // Camera plane
        private double moveSpeed;       // Movement speed
        private double rotSpeed;        // Rotation speed
        private char[,] screen;         // Screen buffer for rendering
        private ConsoleColor[,] foreColors; // Foreground color buffer
        private ConsoleColor[,] backColors; // Background color buffer

        // CP437 block characters for shading (from lightest to darkest)
        private char[] shadeChars = { '░', '▒', '▓', '█' };
        private char[] floorChars = { '·', ':', '·', '.' };

        // Constructor
        public Rendering3D(int[,] maze, int screenWidth, int screenHeight)
        {
            this.maze = maze;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            screen = new char[screenWidth, screenHeight];
            foreColors = new ConsoleColor[screenWidth, screenHeight];
            backColors = new ConsoleColor[screenWidth, screenHeight];

            // Initialize player position - find the starting point (value 2 in maze)
            FindPlayerStart();

            // Set initial direction and camera plane (facing East)
            dirX = 1.0;
            dirY = 0.0;
            planeX = 0.0;
            planeY = 0.66; // Field of view is about 66 degrees

            moveSpeed = 0.05;
            rotSpeed = 0.03;
        }

        // Find the starting position (value 2 in maze)
        private void FindPlayerStart()
        {
            for (int x = 0; x < maze.GetLength(0); x++)
            {
                for (int y = 0; y < maze.GetLength(1); y++)
                {
                    if (maze[x, y] == 2) // Starting point
                    {
                        posX = x + 0.5; // Start in middle of the cell
                        posY = y + 0.5;
                        return;
                    }
                }
            }

            // Default starting position if no start point found
            posX = 1.5;
            posY = 1.5;
        }

        // Main rendering method
        public void Render()
        {
            ClearScreen();

            // Cast rays for each column of the screen
            for (int x = 0; x < screenWidth; x++)
            {
                // Calculate ray position and direction
                double cameraX = 2 * x / (double)screenWidth - 1; // X-coordinate in camera space
                double rayDirX = dirX + planeX * cameraX;
                double rayDirY = dirY + planeY * cameraX;

                // Grid position
                int mapX = (int)posX;
                int mapY = (int)posY;

                // Length of ray from current position to next x or y-side
                double sideDistX;
                double sideDistY;

                // Length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Abs(1 / rayDirX);
                double deltaDistY = Math.Abs(1 / rayDirY);
                double perpWallDist;

                // Direction to step in x or y direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; // Wall hit flag
                int side = 0; // NS or EW wall hit

                // Calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (posX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - posX) * deltaDistX;
                }

                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (posY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - posY) * deltaDistY;
                }

                // Perform DDA (Digital Differential Analysis)
                while (hit == 0)
                {
                    // Jump to next map square, either in x-direction, or in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }

                    // Check if ray has hit a wall
                    if (mapX >= 0 && mapY >= 0 && mapX < maze.GetLength(0) && mapY < maze.GetLength(1))
                    {
                        if (maze[mapX, mapY] == 0) // Wall
                            hit = 1;
                        else if (maze[mapX, mapY] == 3) // End point
                            hit = 2;
                    }
                    else
                    {
                        hit = 1; // Hit boundary of map
                    }
                }

                // Calculate distance projected on camera direction
                if (side == 0)
                    perpWallDist = (mapX - posX + (1 - stepX) / 2) / rayDirX;
                else
                    perpWallDist = (mapY - posY + (1 - stepY) / 2) / rayDirY;

                // Calculate height of line to draw on screen
                int lineHeight = (int)(screenHeight / perpWallDist);

                // Calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + screenHeight / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + screenHeight / 2;
                if (drawEnd >= screenHeight) drawEnd = screenHeight - 1;

                // Choose wall color and character based on distance and side
                ConsoleColor wallForeColor, wallBackColor;
                int shadeIndex;

                // Use distance to determine shade (farther = darker)
                double distanceFactor = Math.Min(perpWallDist, 10) / 10.0; // Normalize to 0-1
                shadeIndex = (int)(distanceFactor * (shadeChars.Length - 1));

                if (hit == 2) // End point
                {
                    wallForeColor = ConsoleColor.Green;
                    wallBackColor = ConsoleColor.DarkGreen;
                }
                else if (side == 1) // NS walls
                {
                    // Darker shade for NS walls
                    wallForeColor = ConsoleColor.Gray;
                    wallBackColor = ConsoleColor.DarkGray;
                    // Make shade one step darker for NS walls
                    shadeIndex = Math.Min(shadeIndex + 1, shadeChars.Length - 1);
                }
                else // EW walls
                {
                    wallForeColor = ConsoleColor.White;
                    wallBackColor = ConsoleColor.Gray;
                }

                // Get the shade character based on index
                char wallChar = shadeChars[shadeIndex];

                // Draw the vertical wall line
                for (int y = drawStart; y <= drawEnd; y++)
                {
                    screen[x, y] = wallChar;
                    foreColors[x, y] = wallForeColor;
                    backColors[x, y] = wallBackColor;
                }

                // Draw ceiling with gradient
                for (int y = 0; y < drawStart; y++)
                {
                    // Calculate ceiling shade based on y position
                    double ceilingFactor = (double)y / drawStart;
                    int ceilingShade = Math.Min((int)(ceilingFactor * 3), 2);

                    screen[x, y] = shadeChars[ceilingShade];
                    foreColors[x, y] = ConsoleColor.DarkBlue;
                    backColors[x, y] = ConsoleColor.Black;
                }

                // Draw floor with gradient
                for (int y = drawEnd + 1; y < screenHeight; y++)
                {
                    // Calculate floor shade based on y position (farther = darker)
                    double floorFactor = (double)(y - drawEnd) / (screenHeight - drawEnd);
                    int floorShade = Math.Min((int)(floorFactor * floorChars.Length), floorChars.Length - 1);

                    screen[x, y] = floorChars[floorShade];
                    foreColors[x, y] = ConsoleColor.DarkGreen;
                    backColors[x, y] = ConsoleColor.Black;
                }
            }

            // Draw the screen buffer
            DrawScreen();
        }

        // Clear the screen buffer
        private void ClearScreen()
        {
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    screen[x, y] = ' ';
                    foreColors[x, y] = ConsoleColor.Black;
                    backColors[x, y] = ConsoleColor.Black;
                }
            }
        }

        // Draw the screen buffer to console
        private void DrawScreen()
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                {
                    // Only change colors when needed to improve performance
                    if (x == 0 || foreColors[x, y] != foreColors[x - 1, y] || backColors[x, y] != backColors[x - 1, y])
                    {
                        Console.ForegroundColor = foreColors[x, y];
                        Console.BackgroundColor = backColors[x, y];
                    }
                    Console.Write(screen[x, y]);
                }
                // Add a line break at the end of each row
                if (y < screenHeight - 1)
                {
                    Console.WriteLine();
                }
            }
        }

        // Handle user input for movement
        private bool isMovingForward = false;
        private bool isMovingBackward = false;
        private bool isRotatingLeft = false;
        private bool isRotatingRight = false;
        private bool isInputProcessingActive = false;
        private CancellationTokenSource inputCancellationSource;

        // Initialize input processing in the constructor or init method
        public void InitializeInputProcessing()
        {
            if (!isInputProcessingActive)
            {
                isInputProcessingActive = true;
                inputCancellationSource = new CancellationTokenSource();
                Task.Run(() => ProcessInputAsync(inputCancellationSource.Token), inputCancellationSource.Token);
            }
        }

        // Clean up when game ends
        public void StopInputProcessing()
        {
            if (isInputProcessingActive)
            {
                isInputProcessingActive = false;
                inputCancellationSource?.Cancel();
                inputCancellationSource?.Dispose();
            }
        }

        // Process input at a controlled rate
        private async Task ProcessInputAsync(CancellationToken cancellationToken)
        {
            const int inputProcessingInterval = 50; // Milliseconds between processing inputs

            while (!cancellationToken.IsCancellationRequested)
            {
                // Clear all movement flags
                isMovingForward = false;
                isMovingBackward = false;
                isRotatingLeft = false;
                isRotatingRight = false;

                // Check current key states using the Win32 API
                if (KeyboardInput.IsKeyPressed(KeyboardInput.VK_W)) isMovingForward = true;
                if (KeyboardInput.IsKeyPressed(KeyboardInput.VK_S)) isMovingBackward = true;
                if (KeyboardInput.IsKeyPressed(KeyboardInput.VK_A)) isRotatingLeft = true;
                if (KeyboardInput.IsKeyPressed(KeyboardInput.VK_D)) isRotatingRight = true;

                // Process movement based on current key states
                if (isMovingForward) Move(1);
                if (isMovingBackward) Move(-1);
                if (isRotatingLeft) Rotate(-1);
                if (isRotatingRight) Rotate(1);

                // Wait before checking again
                await Task.Delay(inputProcessingInterval, cancellationToken);
            }
        }

        // Helper method to check if a specific key is currently pressed
        private bool IsKeyPressed(ConsoleKey key)
        {
            if (!Console.KeyAvailable) return false;

            // Peek at the key without removing it from the buffer
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            // Push the key back to the buffer (not actually possible in .NET)
            // So we need to check all keys we're interested in at once

            return keyInfo.Key == key;
        }

        // Move forward or backward
        private void Move(int direction)
        {
            // Calculate potential new position
            double newPosX = posX + dirX * moveSpeed * direction;
            double newPosY = posY + dirY * moveSpeed * direction;

            // Check X movement - only move if not a wall
            if (newPosX >= 0 && newPosX < maze.GetLength(0) &&
                maze[(int)newPosX, (int)posY] != 0)  // Non-zero means walkable
            {
                posX = newPosX;
            }

            // Check Y movement - only move if not a wall
            if (newPosY >= 0 && newPosY < maze.GetLength(1) &&
                maze[(int)posX, (int)newPosY] != 0)  // Non-zero means walkable
            {
                posY = newPosY;
            }

            // Check if at the end point
            if (maze[(int)posX, (int)posY] == 3)
            {
                Console.Clear();
                Console.WriteLine("Congratulations! You've reached the end of the maze!");
                Thread.Sleep(3000);
            }
        }

        // Rotate left or right
        private void Rotate(int direction)
        {
            // Rotate direction vector
            double oldDirX = dirX;
            dirX = dirX * Math.Cos(rotSpeed * direction) - dirY * Math.Sin(rotSpeed * direction);
            dirY = oldDirX * Math.Sin(rotSpeed * direction) + dirY * Math.Cos(rotSpeed * direction);
            // Rotate camera plane
            double oldPlaneX = planeX;
            planeX = planeX * Math.Cos(rotSpeed * direction) - planeY * Math.Sin(rotSpeed * direction);
            planeY = oldPlaneX * Math.Sin(rotSpeed * direction) + planeY * Math.Cos(rotSpeed * direction);
        }
    }
}