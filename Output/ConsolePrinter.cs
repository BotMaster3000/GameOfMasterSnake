using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Enums;
using GameOfMasterSnake.Interfaces;

namespace GameOfMasterSnake.Output
{
    public class ConsolePrinter : IMapPrinter
    {
        public void DrawMap(IGameMap map)
        {
            if (map.Tiles == null || map.Tiles.Length == 0)
            {
                return;
            }

            Console.Clear();

            int currentYPos = 0;
            int currentXPos = 0;

            for (int tileCounter = 0; tileCounter < map.Tiles.Length; ++tileCounter)
            {
                for (int i = 0; i < map.Tiles.Length; ++i)
                {
                    if (map.Tiles[i].YPos == currentYPos && map.Tiles[i].XPos == currentXPos)
                    {
                        if (map.Tiles[i].Value == TileValues.Food)
                        {
                            Console.Write("F");
                        }
                        else if (map.Tiles[i].Value == TileValues.Empty)
                        {
                            Console.Write("0");
                        }
                        else
                        {
                            Console.Write("S");
                        }

                        ++currentXPos;

                        if (currentXPos >= map.Width)
                        {
                            Console.WriteLine();
                            ++currentYPos;
                            currentXPos = 0;
                        }
                        break;
                    }
                }
            }
        }
    }
}
