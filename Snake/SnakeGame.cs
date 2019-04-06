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
    public class SnakeGame : ISnakeGame
    {
        public IGameMap Map { get; }
        public IMapPrinter Printer { get; }

        public int SnakeXPos { get; private set; } = -1;
        public int SnakeYPos { get; private set; } = -1;
        public int SnakeLength { get; private set; }
        public Direction Direction { get; private set; }

        public int FoodXPos { get; set; }
        public int FoodYPos { get; set; }

        public bool IsFoodPlaced { get; private set; }

        public int TotalMoves { get; private set; }

        public SnakeGame(int height, int width, int initialSnakeLength)
        {
            Map = new Map.GameMap(height, width);
            Printer = new Output.ConsolePrinter();

            SnakeLength = initialSnakeLength;
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
            if (IsFoodPlaced)
            {
                return;
            }
            int yPos = RandomNumberGenerator.GetNextNumber(0, Map.Height - 1);
            int xPos = RandomNumberGenerator.GetNextNumber(0, Map.Width - 1);

            ITile tile = Map.GetTile(xPos, yPos);
            if (tile.Value == TileValues.Empty)
            {
                // Food has a Tile-Value of -2. Should switch these Values to Enumerators at some point
                tile.SetValue(TileValues.Food);
                IsFoodPlaced = true;

                FoodXPos = xPos;
                FoodYPos = yPos;
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
                        if (Direction != Direction.Up && Direction != Direction.Down)
                        {
                            Direction = Direction.Up;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (Direction != Direction.Right && Direction != Direction.Left)
                        {
                            Direction = Direction.Right;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (Direction != Direction.Down && Direction != Direction.Up)
                        {
                            Direction = Direction.Down;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (Direction != Direction.Left && Direction != Direction.Right)
                        {
                            Direction = Direction.Left;
                        }
                        break;
                }
            }
        }

        private void MoveSnake()
        {
            // Decreases tileValue by one, if it is above 0
            // It is supposed to unset the tail of the snake, to simulate its movement
            for (int i = 0; i < Map.Tiles.Length; ++i)
            {
                ITile currentTile = Map.Tiles[i];
                if (currentTile.SnakeLife > 0)
                {
                    --currentTile.SnakeLife;
                    if(currentTile.SnakeLife <= 0)
                    {
                        currentTile.SetValue(TileValues.Empty);
                    }
                }
            }

            // The new Snailpiece is being placed, valued the length of the snail, to simulate its length

            int xIncrement = 0;
            int yIncrement = 0;
            switch (Direction)
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

            SnakeXPos += xIncrement;
            SnakeYPos += yIncrement;

            ITile tile = Map.GetTile(SnakeXPos, SnakeYPos);
            if (tile != null)
            {
                // It is being determined, if there is food, and if, the Snake-Length gets increased by one and
                // The food is no longer Placed, forcing a new Food-Tile next turn
                if (tile.Value == TileValues.Food)
                {
                    ++SnakeLength;
                    IsFoodPlaced = false;
                }

                tile.SetValue(TileValues.Snake);
                tile.SnakeLife = SnakeLength;
            }

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
                SnakeXPos <= -1
                || SnakeXPos >= Map.Width
                || SnakeYPos <= -1
                || SnakeYPos >= Map.Height
                )
            {
                return true;
            }

            TileValues tileValue = TileValues.None;
            switch (Direction)
            {
                case Direction.Up:
                    tileValue = Map.GetTile(SnakeXPos, SnakeYPos - 1)?.Value ?? TileValues.None;
                    break;
                case Direction.Right:
                    tileValue = Map.GetTile(SnakeXPos + 1, SnakeYPos)?.Value ?? TileValues.None;
                    break;
                case Direction.Down:
                    tileValue = Map.GetTile(SnakeXPos, SnakeYPos + 1)?.Value ?? TileValues.None;
                    break;
                case Direction.Left:
                    tileValue = Map.GetTile(SnakeXPos - 1, SnakeYPos)?.Value ?? TileValues.None;
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
            Printer.DrawMap(Map);
        }

        public void PlaceSnakeOnMap()
        {
            int yPos = RandomNumberGenerator.GetNextNumber(0, Map.Height - 1);
            int xPos = RandomNumberGenerator.GetNextNumber(0, Map.Width - 1);

            SnakeYPos = yPos;
            SnakeXPos = xPos;

            switch (RandomNumberGenerator.GetNextNumber(0, 3))
            {
                case 0:
                    Direction = Direction.Up;
                    break;
                case 1:
                    Direction = Direction.Right;
                    break;
                case 2:
                    Direction = Direction.Down;
                    break;
                case 3:
                    Direction = Direction.Left;
                    break;
            }

            for (int i = 0; i < Map.Tiles.Length; ++i)
            {
                if (Map.Tiles[i].YPos == yPos && Map.Tiles[i].XPos == xPos)
                {
                    Map.Tiles[i].SnakeLife = SnakeLength;
                    Map.Tiles[i].SetValue(TileValues.Snake);
                    break;
                }
            }
        }

        public void SetSnakeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    if (Direction != Direction.Up && Direction != Direction.Down)
                    {
                        Direction = Direction.Up;
                    }
                    break;
                case Direction.Right:
                    if (Direction != Direction.Right && Direction != Direction.Left)
                    {
                        Direction = Direction.Right;
                    }
                    break;
                case Direction.Down:
                    if (Direction != Direction.Down && Direction != Direction.Up)
                    {
                        Direction = Direction.Down;
                    }
                    break;
                case Direction.Left:
                    if (Direction != Direction.Left && Direction != Direction.Right)
                    {
                        Direction = Direction.Left;
                    }
                    break;
            }
        }

        public void IsPlayerGameOver()
        {
            throw new NotImplementedException();
        }
    }
}
