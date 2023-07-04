using IoDeSer.Ordering;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer
{
    /// <summary>
    /// Changes the default order of properties in .io file. The ones with the lower value will precede those of higher value.
    /// <para>
    /// Accepted value are from range of 1 to <see cref="uint.MaxValue"/>.
    /// </para>
    /// <para>
    /// In the case of two or more properties having the same value, the default order prevails.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IoItemOrderAttribute : Attribute
    {
        /// <value>
        /// Property order by integer.
        /// </value>
        public int Order { get; }

        /// <summary>
        /// Initializes ordering of property by <seealso cref="ItemOrder"/>.
        /// </summary>
        public IoItemOrderAttribute(ItemOrder orderInFile)
        {
            Order = (int)orderInFile;
        }

        /// <summary>
        /// Initializes ordering of property using any number from 1 - <see cref="uint.MaxValue"/>.
        /// <para>
        /// Number '0' is also allowed, but is causing error due to ambigous call with constructor <see cref="IoItemOrderAttribute(ItemOrder)"/>.
        /// </para>
        /// </summary>
        public IoItemOrderAttribute(uint orderInFile)
        {
            Order = (int)orderInFile;
        }
    }
}
