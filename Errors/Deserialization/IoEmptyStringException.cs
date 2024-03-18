using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors.Deserialization
{
    public class IoEmptyStringException : IoDeserializationException
    {
        public IoEmptyStringException() : base("Provided io string was null or empty.") { }
    }
}
