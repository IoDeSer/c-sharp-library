﻿using System;

namespace IoDeSer.Attributes
{
    /// <summary>
    /// Overrides the name of a property for serialize and deserialize purposes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IoItemNameAttribute : Attribute
    {
        public string customPropertyName { get; }

        public IoItemNameAttribute(string customPropertyName)
        {
            this.customPropertyName = customPropertyName;
        }
    }
}
