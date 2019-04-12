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
        public bool IsPrintingMap { get; set; } = true;

        public void PrintMap(IGameMap map)
        {
            if (IsPrintingMap && map.Tiles?.Length > 0)
            {
                foreach (ITile tile in map.Tiles)
                {
                    if (tile.HasChanged)
                    {
                        Console.CursorLeft = tile.XPos;
                        Console.CursorTop = tile.YPos;
                        switch (tile.Value)
                        {
                            case TileValues.Empty:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write("0");
                                break;
                            case TileValues.Food:
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.Write("F");
                                break;
                            case TileValues.Snake:
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.Write("S");
                                break;
                        }
                        Console.ResetColor();

                        tile.HasChanged = false;
                    }
                }
            }
        }
    }
}
