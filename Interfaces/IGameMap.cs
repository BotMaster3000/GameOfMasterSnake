using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Enums;

namespace GameOfMasterSnake.Interfaces
{
    public interface IGameMap
    {
        ITile[] Tiles { get; }
        int Width { get; }
        int Height { get; }

        ITile GetTile(int xPos, int yPos);
        void SetTileValue(TileValues value, int xPos, int yPos);
    }
}
