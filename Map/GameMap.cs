using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfMasterSnake.Map
{
    public class GameMap
    {
        public readonly Tile[] tiles;

        public int height;
        public int width;

        public GameMap(int height, int width, int defaultTileValue = 0)
        {
            tiles = new Tile[height * width];

            this.height = height;
            this.width = width;

            int counter = 0;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    tiles[counter] = new Tile() { yPos = y, xPos = x, value = defaultTileValue };

                    ++counter;
                }
            }
        }

        private Tile GetTile(int yPos, int xPos)
        {
            if (tiles == null || tiles.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < tiles.Length; ++i)
            {
                if (tiles[i].yPos == yPos && tiles[i].xPos == xPos)
                {
                    return tiles[i];
                }
            }
            return null;
        }

        public int GetTileValueAtCoordinates(int yPos, int xPos)
        {
            Tile tile = GetTile(yPos, xPos);
            if (tile == null)
            {
                return -1;
            }
            else
            {
                return tile.value;
            }
        }

        public void SetTileValueAtCoordinates(int yPos, int xPos, int newValue)
        {
            Tile tile = GetTile(yPos, xPos);

            if (tile != null)
            {
                tile.value = newValue;
            }
        }
    }
}
