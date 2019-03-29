using GameOfMasterSnake.Enums;

namespace GameOfMasterSnake.Interfaces
{
    public interface ITile
    {
        int XPos { get; set; }
        int YPos { get; set; }
        TileValues Value { get; set; }
    }
}
