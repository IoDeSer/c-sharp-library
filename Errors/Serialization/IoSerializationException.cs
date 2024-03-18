using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors.Serialization
{
    public class IoSerializationException : IoFormatException
    {
        public IoSerializationException(string message) : base(message)
        {
        }

        public IoSerializationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
