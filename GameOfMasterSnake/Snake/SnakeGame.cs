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
        public IGameMap Map { get; private set; }
        public IMapPrinter Printer { get; private set; }

        public int SnakeXPos { get; private set; } = -1;
        public int SnakeYPos { get; private set; } = -1;
        public int SnakeLength { get; private set; }
        public Direction Direction { get; private set; }

        public int FoodXPos { get; set; }
        public int FoodYPos { get; set; }

        public bool IsFoodPlaced { get; private set; }

        public int TotalMoves { get; private set; }

        private readonly int height;
        private readonly int width;
        private readonly int initialSnakeLength;

        public static object RandomNumberLock = new object();

        public SnakeGame(int height, int width, int initialSnakeLength)
        {
            this.height = height;
            this.width = width;
            this.initialSnakeLength = initialSnakeLength;
            InitializeGame();
            Printer = new Output.ConsolePrinter();
        }

        public void InitializeGame()
        {
            Map = new Map.GameMap(height, width);

            SnakeLength = initialSnakeLength;

            SnakeXPos = -1;
            SnakeYPos = -1;

            FoodXPos = -1;
            FoodYPos = -1;
            IsFoodPlaced = false;
            TotalMoves = 0;

            PlaceFood();
        }

        public void BeginGame()
        {
            PlaceSnakeOnMap();
            PlaceFood();

            DrawMap();
            Thread.Sleep(1000);

            while (!IsPlayerGameOver())
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
        }

        private void PlaceFood()
        {
            while (!IsFoodPlaced)
            {
                int yPos;
                int xPos;
                lock (RandomNumberLock)
                {
                    yPos = RandomNumberGenerator.GetNextNumber(0, Map.Height - 1);
                    xPos = RandomNumberGenerator.GetNextNumber(0, Map.Width - 1);
                }

                ITile tile = Map.GetTile(xPos, yPos);
                if (tile.Value == TileValues.Empty)
                {
                    tile.SetValue(TileValues.Food);
                    IsFoodPlaced = true;

                    FoodXPos = xPos;
                    FoodYPos = yPos;
                }
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
            DecreaseSnakeLife();

            // The new Snailpiece is being placed, valued the length of the snail, to simulate its length
            int xIncrement = 0;
            int yIncrement = 0;
            GetIncremention(ref xIncrement, ref yIncrement);

            SnakeXPos += xIncrement;
            SnakeYPos += yIncrement;

            ITile tile = Map.GetTile(SnakeXPos, SnakeYPos);
            if (tile != null)
            {
                if (TileContainsFood(tile))
                {
                    EatFood(tile);
                }

                tile.SetValue(TileValues.Snake);
                tile.SnakeLife = SnakeLength;
            }

            ++TotalMoves;
        }

        private void DecreaseSnakeLife()
        {
            for (int i = 0; i < Map.Tiles.Length; ++i)
            {
                ITile currentTile = Map.Tiles[i];
                if (currentTile.SnakeLife > 0)
                {
                    --currentTile.SnakeLife;
                    if (currentTile.SnakeLife <= 0)
                    {
                        currentTile.SetValue(TileValues.Empty);
                    }
                }
            }
        }

        private void GetIncremention(ref int xIncrement, ref int yIncrement)
        {
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
        }

        private bool TileContainsFood(ITile tile)
        {
            return tile.Value == TileValues.Food;
        }

        private void EatFood(ITile tile)
        {
            ++SnakeLength;
            tile.SetValue(TileValues.Empty);
            IsFoodPlaced = false;
        }

        /// <summary>
        /// Überprüft ob der Spieler verloren hat.
        /// Man verliert, wenn man die Wand berührt/Das Spielfeld verlässt,
        /// oder wenn man in sich selbst "kriecht"
        /// </summary>
        /// <returns></returns>
        public bool IsPlayerGameOver()
        {
            if (!IsPlayerOutOfBound())
            {
                TileValues tileValue = GetNextTileValue();
                return tileValue == TileValues.None || tileValue == TileValues.Snake;
            }
            return true;
        }

        private bool IsPlayerOutOfBound()
        {
            return SnakeXPos <= -1
                || SnakeXPos >= Map.Width
                || SnakeYPos <= -1
                || SnakeYPos >= Map.Height;
        }

        private TileValues GetNextTileValue()
        {
            switch (Direction)
            {
                case Direction.Up:
                    return Map.GetTile(SnakeXPos, SnakeYPos - 1)?.Value ?? TileValues.None;
                case Direction.Right:
                    return Map.GetTile(SnakeXPos + 1, SnakeYPos)?.Value ?? TileValues.None;
                case Direction.Down:
                    return Map.GetTile(SnakeXPos, SnakeYPos + 1)?.Value ?? TileValues.None;
                case Direction.Left:
                    return Map.GetTile(SnakeXPos - 1, SnakeYPos)?.Value ?? TileValues.None;
                default:
                    return TileValues.None;
            }
        }

        private void DrawMap()
        {
            Printer.PrintMap(Map);
        }

        public void PlaceSnakeOnMap()
        {
            int yPos;
            int xPos;

            lock (RandomNumberLock)
            {
                yPos = RandomNumberGenerator.GetNextNumber(0, Map.Height - 1);
                xPos = RandomNumberGenerator.GetNextNumber(0, Map.Width - 1);

                Direction = (Direction)RandomNumberGenerator.GetNextNumber(0, 3);
            }

            SnakeYPos = yPos;
            SnakeXPos = xPos;


            for (int i = 0; i < Map.Tiles.Length; ++i)
            {
                ITile tile = Map.Tiles[i];
                if (tile.YPos == yPos && tile.XPos == xPos)
                {
                    tile.SnakeLife = SnakeLength;
                    tile.SetValue(TileValues.Snake);
                    break;
                }
            }
        }

        public void SetSnakeDirection(Direction newDirection)
        {
            Direction = (Direction == Direction.Up && (newDirection == Direction.Left || newDirection == Direction.Right))
                ? newDirection
                : (Direction == Direction.Right && (newDirection == Direction.Up || newDirection == Direction.Down))
                ? newDirection
                : (Direction == Direction.Down && (newDirection == Direction.Left || newDirection == Direction.Right))
                ? newDirection
                : (Direction == Direction.Left && (newDirection == Direction.Up || newDirection == Direction.Down))
                ? newDirection
                : Direction;

            //int directionFactor = newDirection - Direction;
            //directionFactor = directionFactor <= 0
            //    ? -directionFactor
            //    : directionFactor;
            //Direction = directionFactor == 1
            //    ? newDirection
            //    : Direction;
        }
    }
}
