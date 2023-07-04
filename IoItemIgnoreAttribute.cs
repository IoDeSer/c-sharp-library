using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer
{
    /// <summary>
    /// Indicates, that property with this attribute should not be serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IoItemIgnoreAttribute : Attribute
    {
    }
}
