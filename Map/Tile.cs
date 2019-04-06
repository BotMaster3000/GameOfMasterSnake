using GameOfMasterSnake.Enums;
using GameOfMasterSnake.Interfaces;

namespace GameOfMasterSnake.Map
{
    public class Tile : ITile
    {
        public int value;

        public int XPos { get; }
        public int YPos { get; }
        public TileValues Value { get; private set; }
        public bool HasChanged { get; set; }
        public int SnakeLife { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public Tile(int xPos, int yPos)
        {
            XPos = xPos;
            YPos = yPos;
        }

        public void SetValue(TileValues value)
        {
            Value = value;
            HasChanged = true;
        }
    }
}
