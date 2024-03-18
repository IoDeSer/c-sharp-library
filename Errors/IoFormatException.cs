using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors
{
    /// <summary>
    /// Base exception class for all Io format exception
    /// </summary>
    public abstract class IoFormatException : Exception
    {
        public IoFormatException(string message) : base(message)
        {
            
        }

        public IoFormatException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
