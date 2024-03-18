using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Errors
{
    public class IoTypeNotSupportedException : IoFormatException
    {
        public IoTypeNotSupportedException(Type notSupportedType) : base($"The type {notSupportedType.Name} is not supported for Io format.")
        {
            
        }
    }
}
