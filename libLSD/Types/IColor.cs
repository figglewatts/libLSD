using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    public interface IColor
    {
        uint Red { get; }
        uint Green { get; }
        uint Blue { get; }
    }
}
