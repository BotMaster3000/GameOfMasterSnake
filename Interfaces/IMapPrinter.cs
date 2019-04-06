using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfMasterSnake.Interfaces
{
    public interface IMapPrinter
    {
        void DrawMap(IGameMap map);
    }
}
