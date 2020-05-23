﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Exceptions
{
    /// <summary>
    /// Thrown when a file that was loaded was in an incorrect format,
    /// i.e. an incorrect magic number in the header
    /// </summary>
    public class BadFormatException : Exception
    {
        public BadFormatException() { }

        public BadFormatException(string message) : base(message) { }

        public BadFormatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
