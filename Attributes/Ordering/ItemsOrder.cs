using System;
using System.Collections.Generic;
using System.Text;

namespace IoDeSer.Attributes.Ordering
{
    /// <summary>
    /// Indicates how all the items should be ordered
    /// </summary>
    public enum ItemsOrder
    {
        /// <summary>
        /// Orders items by their default sequence.
        /// </summary>
        DEFAULT,
        /// <summary>
        /// Orders items A-Z
        /// </summary>
        ALPHABETICAL,
        /// <summary>
        /// Orders items Z-A
        /// </summary>
        ALPHABETICAL_REVERSE,
        /// <summary>
        /// Orders the longest item name first.
        /// </summary>
        LONGEST_FIRST,
        /// <summary>
        /// Orders the shortest item name first.
        /// </summary>
        SHORTEST_FIRST
    }
}
