using GameOfMasterSnake.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfMasterSnake.Interfaces
{
    public interface ISnakeGame
    {
        IGameMap Map { get; }
        IMapPrinter Printer { get; }

        int SnakeXPos { get; }
        int SnakeYPos { get; }
        int SnakeLength { get; }
        Direction Direction { get; }

        int FoodXPos { get; }
        int FoodYPos { get; }
        bool IsFoodPlaced { get; }

        int TotalMoves { get; }

        void NextRound();
        void IsPlayerGameOver();
        void PlaceSnakeOnMap();
        void SetSnakeDirection(Direction direction);
    }
}
