using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfMasterSnake.Snake
{
    public class SnakeGame
    {
        private Random rand = new Random();

        private enum Direction { Up, Right, Down, Left, None }

        public readonly Map.OldGameMap map;

        public int currentSnakeLength;
        private Direction currentSnakeDirection = Direction.None;

        private int currentSnakeXPosition = -1;
        private int currentSnakeYPosition = -1;

        bool foodIsPlaced = false;

        public SnakeGame(int height, int width, int initialSnakeLength)
        {
            map = new Map.OldGameMap(height, width);

            currentSnakeLength = initialSnakeLength;
        }

        public void BeginGame()
        {
            PlaceSnakeOnMap();
            PlaceFood();

            DrawMap();
            Thread.Sleep(1000);

            while (!PlayerGameOver())
            {
                PlaceFood();
                MoveSnake();
                DrawMap();
                CheckForDirectionChange();

                Thread.Sleep(200);
            }
        }

        private void PlaceFood()
        {
            if (foodIsPlaced)
            {
                return;
            }
            int yPos = rand.Next(0, map.height);
            int xPos = rand.Next(0, map.width);

            if(map.GetTileValueAtCoordinates(yPos, xPos) <= 0)
            {
                // Food has a Tile-Value of -2. Should switch these Values to Enumerators at some point
                map.SetTileValueAtCoordinates(yPos, xPos, -2);
                foodIsPlaced = true;
            }
            else
            {
                PlaceFood();
            }
        }

        private void CheckForDirectionChange()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if(currentSnakeDirection != Direction.Up && currentSnakeDirection != Direction.Down)
                        {
                            currentSnakeDirection = Direction.Up;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (currentSnakeDirection != Direction.Right && currentSnakeDirection != Direction.Left)
                        {
                            currentSnakeDirection = Direction.Right;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (currentSnakeDirection != Direction.Down && currentSnakeDirection != Direction.Up)
                        {
                            currentSnakeDirection = Direction.Down;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (currentSnakeDirection != Direction.Left && currentSnakeDirection != Direction.Right)
                        {
                            currentSnakeDirection = Direction.Left;
                        }
                        break;
                }
            }
        }

        private void MoveSnake()
        {
            // Decreases tileValue by one, if it is above 0
            // It is supposed to unset the tail of the snake, to simulate its movement
            for(int i = 0; i < map.tiles.Length; ++i)
            {
                if(map.tiles[i].value > 0)
                {
                    --map.tiles[i].value;
                }
            }

            // The new Snailpiece is being placed, valued the length of the snail, to simulate its length

            int xIncrement = 0;
            int yIncrement = 0;
            switch (currentSnakeDirection)
            {
                case Direction.Up:
                    yIncrement = -1;
                    break;
                case Direction.Right:
                    xIncrement = 1;
                    break;
                case Direction.Down:
                    yIncrement = 1;
                    break;
                case Direction.Left:
                    xIncrement = -1;
                    break;
            }

            currentSnakeXPosition += xIncrement;
            currentSnakeYPosition += yIncrement;

            // It is being determined, if there is food, and if, the Snake-Length gets increased by one and
            // The food is no longer Placed, forcing a new Food-Tile next turn
            if(map.GetTileValueAtCoordinates(currentSnakeYPosition, currentSnakeXPosition) == -2)
            {
                ++currentSnakeLength;
                foodIsPlaced = false;
            }

            map.SetTileValueAtCoordinates(currentSnakeYPosition, currentSnakeXPosition, currentSnakeLength);
        }

        /// <summary>
        /// Überprüft ob der Spieler verloren hat.
        /// Man verliert, wenn man die Wand berührt/Das Spielfeld verlässt,
        /// oder wenn man in sich selbst "kriecht"
        /// </summary>
        /// <returns></returns>
        private bool PlayerGameOver()
        {
            // If the Player is somewhere out of the map, he has lost
            if (
                currentSnakeXPosition <= -1
                || currentSnakeXPosition >= map.width
                || currentSnakeYPosition <= -1
                || currentSnakeYPosition >= map.height
                )
            {
                return true;
            }

            int tileValue = 0;
            switch (currentSnakeDirection)
            {
                case Direction.Up:
                    tileValue = map.GetTileValueAtCoordinates(currentSnakeYPosition - 1, currentSnakeXPosition);
                    break;
                case Direction.Right:
                    tileValue = map.GetTileValueAtCoordinates(currentSnakeYPosition, currentSnakeXPosition + 1);
                    break;
                case Direction.Down:
                    tileValue = map.GetTileValueAtCoordinates(currentSnakeYPosition + 1, currentSnakeXPosition);
                    break;
                case Direction.Left:
                    tileValue = map.GetTileValueAtCoordinates(currentSnakeYPosition, currentSnakeXPosition - 1);
                    break;
                default:
                    // If Direction is Null, there has to be an error, so game over.
                    return true;
            }

            // If the Tile could not be found for some reason or if there is still a piece of Snake, the player has lost
            if(tileValue == -1 || tileValue > 0)
            {
                return true;
            }

            return false;
        }

        private void DrawMap()
        {
            if(map.tiles == null || map.tiles.Length == 0)
            {
                return;
            }

            Console.Clear();

            int currentYPos = 0;
            int currentXPos = 0;

            for(int tileCounter = 0; tileCounter < map.tiles.Length; ++tileCounter)
            {
                for (int i = 0; i < map.tiles.Length; ++i)
                {
                    if (map.tiles[i].YPos == currentYPos && map.tiles[i].XPos == currentXPos)
                    {
                        //Console.Write(map.tiles[i].value);
                        if (map.tiles[i].value == -2)
                        {
                            Console.Write("F");
                        }
                        else if(map.tiles[i].value <= 0)
                        {
                            Console.Write(map.tiles[i].value);
                        }
                        else
                        {
                            Console.Write("S");
                        }

                        ++currentXPos;

                        if(currentXPos >= map.width)
                        {
                            Console.WriteLine();
                            ++currentYPos;
                            currentXPos = 0;
                        }
                        break;
                    }
                }
            }
        }

        private void PlaceSnakeOnMap()
        {
            int yPos = rand.Next(0, map.height);
            int xPos = rand.Next(0, map.width);

            currentSnakeYPosition = yPos;
            currentSnakeXPosition = xPos;

            switch(rand.Next(0, 4))
            {
                case 0:
                    currentSnakeDirection = Direction.Up;
                    break;
                case 1:
                    currentSnakeDirection = Direction.Right;
                    break;
                case 2:
                    currentSnakeDirection = Direction.Down;
                    break;
                case 3:
                    currentSnakeDirection = Direction.Left;
                    break;
            }
            
            for(int i = 0; i < map.tiles.Length; ++i)
            {
                if(map.tiles[i].YPos == yPos && map.tiles[i].XPos == xPos)
                {
                    map.tiles[i].value = currentSnakeLength;

                    break;
                }
            }
        }
    }
}
