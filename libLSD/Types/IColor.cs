using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Types
{
    public interface IColor
    {
        float Red { get; }
        float Green { get; }
        float Blue { get; }
        float Alpha { get; }
        bool TransparencyControl { get; }
        bool IsBlack { get; }
    }
}
