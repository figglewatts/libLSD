using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Exceptions
{
    /// <summary>
    /// An exception thrown when a file format cannot be written to disk.
    /// </summary>
    public class UnwriteableException : Exception
    {
        public UnwriteableException(string message) : base(message) { }

        public UnwriteableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
