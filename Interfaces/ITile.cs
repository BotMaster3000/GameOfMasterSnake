using GameOfMasterSnake.Enums;

namespace GameOfMasterSnake.Interfaces
{
    public interface ITile
    {
        int XPos { get; }
        int YPos { get; }
        TileValues Value { get; }
        bool HasChanged { get; set; }
        int SnakeLife { get; set; }

        void SetValue(TileValues value);
    }
}
