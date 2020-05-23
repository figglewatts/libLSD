using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Interfaces
{
    public interface IWriteable
    {
        void Write(BinaryWriter bw);
    }
}
