using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfMasterSnake.Interfaces
{
    public interface IMapPrinter
    {
        bool IsPrintingMap { get; set; }
        void PrintMap(IGameMap map);
    }
}
