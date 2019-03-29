using GameOfMasterSnake.Enums;
using GameOfMasterSnake.Interfaces;

namespace GameOfMasterSnake.Map
{
    public class Tile : ITile
    {
        public int value;

        public int XPos { get; set; }
        public int YPos { get; set; }
        public TileValues Value { get; set; }
    }
}
