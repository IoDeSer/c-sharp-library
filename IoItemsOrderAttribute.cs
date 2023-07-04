using IoDeSer.Ordering;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer
{
    /// <summary>
    /// Describes how all the properties in this calss should be ordered.
    /// <para>
    /// Overrides <see cref="IoItemOrderAttribute"/> for all properties.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IoItemsOrderAttribute : Attribute
    {
        /// <value>
        /// Property order.
        /// </value>
        public ItemsOrder Order { get; }

        /// <summary>
        /// Initializes property ordering using <seealso cref="ItemsOrder"/>.
        /// </summary>
        public IoItemsOrderAttribute(ItemsOrder order)
        {
            Order = order;
        }


        /// <summary>
        /// Initializes default properties ordering.
        /// </summary>
        public IoItemsOrderAttribute()
        {
            Order = ItemsOrder.DEFAULT;
        }
    }
}
