using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Enums;
using GameOfMasterSnake.Interfaces;

namespace GameOfMasterSnake.Map
{
    public class GameMap : IGameMap
    {
        public ITile[] Tiles { get; private set; }
        public int Width { get; }
        public int Height { get; }

        public GameMap(int width, int height)
        {
            Width = width;
            Height = height;
            InitializeGameMap();
        }

        private void InitializeGameMap()
        {
            InitializeTilesArray();
            FillTilesArray();
        }

        private void InitializeTilesArray()
        {
            Tiles = new ITile[Width * Height];
        }

        private void FillTilesArray()
        {
            int counter = 0;
            for (int xPos = 0; xPos < Width; ++xPos)
            {
                for (int yPos = 0; yPos < Height; ++yPos)
                {
                    ITile tile = new Tile(xPos, yPos);
                    tile.SetValue(TileValues.Empty);
                    Tiles[counter] = tile;
                    ++counter;
                }
            }
        }

        public ITile GetTile(int xPos, int yPos)
        {
            return Array.Find(Tiles, x => x.XPos == xPos && x.YPos == yPos);
        }

        public void SetTileValue(TileValues value, int xPos, int yPos)
        {
            GetTile(xPos, yPos).SetValue(value);
        }
    }
}
