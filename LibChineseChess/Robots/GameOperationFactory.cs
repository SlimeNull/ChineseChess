using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChineseChess.Robots
{
    public delegate IEnumerable<GameOperation> GameOperationFactory(Board Board, int CurrentDepth);
}
