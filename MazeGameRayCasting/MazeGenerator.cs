namespace MazeGame_v2
{
    //This class contains a recursive algorithm to generate a maze using paths, as well as setting start and end points

    public class MazeGenerator
    {
        private int[,] maze; //declaring 2d array maze, width and height, random, and endpoint as coordinates
        private int width, height;
        private Random rand = new Random();
        private (int x, int y) endPoint;

        // Path width parameter - adjust as needed (2 for wider paths, 1 for normal)
        private int pathWidth = 3;

        public MazeGenerator(int width, int height)
        {
            this.width = width;
            this.height = height;
            maze = new int[width, height]; //initialising the maze array
            GenerateMaze(); //call GenerateMaze
        }

        private void GenerateMaze()
        {
            for (int x = 0; x < width; x++) //Fill the maze array with 0s (walls)
                for (int y = 0; y < height; y++)
                    maze[x, y] = 0;

            CreatePath(1, 1); //calling createpath method passing 1,1 position
            SetStartEndPoints();
        }

        private void CreatePath(int x, int y) //Recursive backtracking method to create a maze path
        {
            //note - dx/dy = direction x/direction y & nx/ny = next x/next y
            maze[x, y] = 1; //Set the current position in the maze to 1 to turn it into a path

            // Make the path wider by filling adjacent cells based on pathWidth
            for (int wx = 0; wx < pathWidth; wx++)
            {
                for (int wy = 0; wy < pathWidth; wy++)
                {
                    int widthX = x + wx;
                    int widthY = y + wy;
                    // Ensure we're not going out of bounds
                    if (widthX > 0 && widthY > 0 && widthX < width - 1 && widthY < height - 1)
                    {
                        maze[widthX, widthY] = 1;
                    }
                }
            }

            var directions = new (int dx, int dy)[] { (0, -1), (1, 0), (0, 1), (-1, 0) }; //define possible directions
            directions = directions.OrderBy(d => rand.Next()).ToArray(); //order them randomly into an array

            foreach (var (dx, dy) in directions) //loop through each random direction
            {
                // Calculate next cell position with spacing for wider paths
                int nx = x + dx * (pathWidth + 1);
                int ny = y + dy * (pathWidth + 1);

                if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[nx, ny] == 0) //check if valid position
                {
                    // Fill in the path between current position and next position
                    for (int step = 1; step <= pathWidth + 1; step++)
                    {
                        int px = x + dx * step;
                        int py = y + dy * step;

                        // Create wider corridor by filling adjacent cells
                        for (int wx = 0; wx < pathWidth; wx++)
                        {
                            for (int wy = 0; wy < pathWidth; wy++)
                            {
                                int widthX = px + wx;
                                int widthY = py + wy;
                                // Ensure we're not going out of bounds
                                if (widthX > 0 && widthY > 0 && widthX < width - 1 && widthY < height - 1)
                                {
                                    maze[widthX, widthY] = 1;
                                }
                            }
                        }
                    }

                    CreatePath(nx, ny); //Recursive call that repeats the method
                }
            }
        }

        private void SetStartEndPoints() //Method to set start point and end point of the maze
        {
            //Starting to search for maze start position
            for (int x = 1; x < width; x++) //Go through every position
            {
                for (int y = 1; y < height; y++)
                {
                    if (maze[x, y] == 1) //The first available path position will be the start point
                    {
                        maze[x, y] = 2; //Set current position in maze array to 2 to represent start point
                        break; //Exit and move on to finding end point
                    }
                }
                break;
            }

            //This completes the previous search in reverse order to find the furthest point from the start
            for (int x = width - 2; x > 0; x--) //Searching array in opposite direction
            {
                for (int y = height - 2; y > 0; y--)
                {
                    if (maze[x, y] == 1) //If current position is available path
                    {
                        maze[x, y] = 3; //Set current position to 3 to represent end point
                        endPoint = (x, y); //store coordinates for end point
                        return; //end
                    }
                }
            }
        }

        public int[,] GetMaze() => maze; //Returning the final maze
        public (int x, int y) GetEndPoint() => endPoint; //return end point
    }

}
