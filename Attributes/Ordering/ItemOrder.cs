namespace IoDeSer.Attributes.Ordering
{
    /// <summary>
    /// Easy way to indicate order (<see cref="FIRST"/>, <see cref="LAST"/>) of items.
    /// </summary>
    public enum ItemOrder
    {
        /// <summary>
        /// Indicates that this item should be first.
        /// </summary>
        FIRST = int.MinValue,
        /// <summary>
        /// Indicates that this item should be last.
        /// </summary>
        LAST = int.MaxValue
    }
}
