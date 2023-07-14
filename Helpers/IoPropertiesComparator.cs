using IoDeSer.Attributes.Ordering;
using IoDeSer.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IoDeSer.Helpers
{
    internal class IoPropertiesComparator<T> : IComparer<PropertyInfo>
    {

        public int Compare(PropertyInfo x, PropertyInfo y)
        {
            Type objectType = typeof(T);
            IoItemsOrderAttribute itemsOrder = objectType.GetCustomAttribute<IoItemsOrderAttribute>();

            if (itemsOrder == null)
            {
                var xOrder = x.GetCustomAttribute<IoItemOrderAttribute>() == null ? int.MaxValue : x.GetCustomAttribute<IoItemOrderAttribute>().Order;
                var yOrder = y.GetCustomAttribute<IoItemOrderAttribute>() == null ? int.MaxValue : y.GetCustomAttribute<IoItemOrderAttribute>().Order;
                return xOrder.CompareTo(yOrder);
            }
            else
            {
                switch (itemsOrder.Order)
                {
                    case ItemsOrder.ALPHABETICAL:
                        return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    case ItemsOrder.ALPHABETICAL_REVERSE:
                        return String.Compare(y.Name, x.Name, StringComparison.Ordinal);
                    case ItemsOrder.LONGEST_FIRST:
                        return y.Name.Length.CompareTo(x.Name.Length);
                    case ItemsOrder.SHORTEST_FIRST:
                        return x.Name.Length.CompareTo(y.Name.Length);
                    default:
                        return 0;
                }
            }
        }
    }
}