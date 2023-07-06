using System;

namespace IoDeSer.Attributes
{
    /// <summary>
    /// Indicates, that property with this attribute should not be serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IoItemIgnoreAttribute : Attribute
    {
    }
}
