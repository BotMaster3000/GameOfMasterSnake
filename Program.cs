using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Snake;

namespace GameOfMasterSnake
{
    class Program
    {
        static void Main(string[] args)
        {
            SnakeGame x = new SnakeGame(10, 10, 5);
            x.BeginGame();
        }
    }
}
