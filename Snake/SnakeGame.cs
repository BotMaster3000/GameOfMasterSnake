using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameOfMasterSnake.Enums;
using GameOfMasterSnake.Interfaces;
using NeuralBotMasterFramework.Helper;

namespace GameOfMasterSnake.Snake
{
    public class SnakeGame
    {
        public readonly IGameMap map;

        public int currentSnakeLength;
        public Direction currentSnakeDirection = Direction.None;

        public int TotalMoves;

        public int currentSnakeXPosition = -1;
        public int currentSnakeYPosition = -1;

        public int FoodXPosition { get; set; }
        public int FoodYPosition { get; set; }

        bool foodIsPlaced = false;

        public SnakeGame(int height, int width, int initialSnakeLength)
        {
            map = new Map.GameMap(height, width);

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

                MoveSnake();
                DrawMap();
                CheckForDirectionChange();

                Thread.Sleep(200);
            }
        }

        public void NextRound()
        {
            PlaceFood();
            MoveSnake();
            DrawMap();
            //CheckForDirectionChange();

            Thread.Sleep(10);
        }

        private void PlaceFood()
        {
            if (foodIsPlaced)
            {
                return;
            }
            int yPos = RandomNumberGenerator.GetNextNumber(0, map.Height - 1);
            int xPos = RandomNumberGenerator.GetNextNumber(0, map.Width - 1);

            ITile tile = map.GetTile(xPos, yPos);
            if (tile.Value == TileValues.Empty)
            {
                // Food has a Tile-Value of -2. Should switch these Values to Enumerators at some point
                tile.SetValue(TileValues.Food);
                foodIsPlaced = true;

                FoodXPosition = xPos;
                FoodYPosition = yPos;
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
                        if (currentSnakeDirection != Direction.Up && currentSnakeDirection != Direction.Down)
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
            for (int i = 0; i < map.Tiles.Length; ++i)
            {
                if (map.Tiles[i].SnakeLife > 0)
                {
                    --map.Tiles[i].SnakeLife;
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

            ITile tile = map.GetTile(currentSnakeXPosition, currentSnakeYPosition);

            // It is being determined, if there is food, and if, the Snake-Length gets increased by one and
            // The food is no longer Placed, forcing a new Food-Tile next turn
            if (tile.Value == TileValues.Food)
            {
                ++currentSnakeLength;
                foodIsPlaced = false;
            }

            tile.SetValue(TileValues.Snake);
            tile.SnakeLife = currentSnakeLength;

            ++TotalMoves;
        }

        /// <summary>
        /// Überprüft ob der Spieler verloren hat.
        /// Man verliert, wenn man die Wand berührt/Das Spielfeld verlässt,
        /// oder wenn man in sich selbst "kriecht"
        /// </summary>
        /// <returns></returns>
        public bool PlayerGameOver()
        {
            // If the Player is somewhere out of the map, he has lost
            if (
                currentSnakeXPosition <= -1
                || currentSnakeXPosition >= map.Width
                || currentSnakeYPosition <= -1
                || currentSnakeYPosition >= map.Height
                )
            {
                return true;
            }

            TileValues tileValue = TileValues.None;
            switch (currentSnakeDirection)
            {
                case Direction.Up:
                    tileValue = map.GetTile(currentSnakeXPosition, currentSnakeYPosition - 1).Value;
                    break;
                case Direction.Right:
                    tileValue = map.GetTile(currentSnakeXPosition + 1, currentSnakeYPosition).Value;
                    break;
                case Direction.Down:
                    tileValue = map.GetTile(currentSnakeXPosition, currentSnakeYPosition + 1).Value;
                    break;
                case Direction.Left:
                    tileValue = map.GetTile(currentSnakeXPosition - 1, currentSnakeYPosition).Value;
                    break;
                default:
                    // If Direction is Null, there has to be an error, so game over.
                    return true;
            }

            // If the Tile could not be found for some reason or if there is still a piece of Snake, the player has lost
            return tileValue == TileValues.None || tileValue == TileValues.Snake;
        }

        private void DrawMap()
        {
            if (map.Tiles == null || map.Tiles.Length == 0)
            {
                return;
            }

            Console.Clear();

            int currentYPos = 0;
            int currentXPos = 0;

            for (int tileCounter = 0; tileCounter < map.Tiles.Length; ++tileCounter)
            {
                for (int i = 0; i < map.Tiles.Length; ++i)
                {
                    if (map.Tiles[i].YPos == currentYPos && map.Tiles[i].XPos == currentXPos)
                    {
                        //Console.Write(map.tiles[i].value);
                        if (map.Tiles[i].Value == TileValues.Food)
                        {
                            Console.Write("F");
                        }
                        else if (map.Tiles[i].Value == TileValues.Empty)
                        {
                            Console.Write(TileValues.Empty);
                        }
                        else
                        {
                            Console.Write("S");
                        }

                        ++currentXPos;

                        if (currentXPos >= map.Width)
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

        public void PlaceSnakeOnMap()
        {
            int yPos = RandomNumberGenerator.GetNextNumber(0, map.Height - 1);
            int xPos = RandomNumberGenerator.GetNextNumber(0, map.Width - 1);

            currentSnakeYPosition = yPos;
            currentSnakeXPosition = xPos;

            switch (RandomNumberGenerator.GetNextNumber(0, 3))
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

            for (int i = 0; i < map.Tiles.Length; ++i)
            {
                if (map.Tiles[i].YPos == yPos && map.Tiles[i].XPos == xPos)
                {
                    map.Tiles[i].SnakeLife = currentSnakeLength;
                    map.Tiles[i].SetValue(TileValues.Snake);
                    break;
                }
            }
        }

        public void SetSnakeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    if (currentSnakeDirection != Direction.Up && currentSnakeDirection != Direction.Down)
                    {
                        currentSnakeDirection = Direction.Up;
                    }
                    break;
                case Direction.Right:
                    if (currentSnakeDirection != Direction.Right && currentSnakeDirection != Direction.Left)
                    {
                        currentSnakeDirection = Direction.Right;
                    }
                    break;
                case Direction.Down:
                    if (currentSnakeDirection != Direction.Down && currentSnakeDirection != Direction.Up)
                    {
                        currentSnakeDirection = Direction.Down;
                    }
                    break;
                case Direction.Left:
                    if (currentSnakeDirection != Direction.Left && currentSnakeDirection != Direction.Right)
                    {
                        currentSnakeDirection = Direction.Left;
                    }
                    break;
            }
        }
    }
}
