using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors.Deserialization
{
    public class IoMissingPropertyException : IoDeserializationException
    {
        public IoMissingPropertyException(Type objectType, string propertyName) : base($"Object of type {objectType} does not have property named {propertyName}.") { }
    }
}
