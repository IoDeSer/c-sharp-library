using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors.Deserialization
{
    public class IoMissingConstructorException : IoDeserializationException
    {
        public IoMissingConstructorException(Type objectType, System.MissingMethodException inner) 
            : base($"Object of type {objectType} must have parameterless constructor.", inner) { }
    }
}
