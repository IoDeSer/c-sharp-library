using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors.Deserialization
{
    public class IoVerticalBarsException : IoDeserializationException
    {
        public IoVerticalBarsException() : base("Provided io string lacks vertical bars at the beggining or end.") { }
    }
}
