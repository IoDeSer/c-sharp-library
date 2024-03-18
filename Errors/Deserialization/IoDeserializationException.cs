using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors.Deserialization
{
    /// <summary>
    /// Base exception for all deserialization errors in Io format.
    /// </summary>
    public class IoDeserializationException : IoFormatException
    {
        public IoDeserializationException(string message) : base(message)
        {
        }

        public IoDeserializationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
