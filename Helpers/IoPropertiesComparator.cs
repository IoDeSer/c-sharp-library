using IoDeSer.Attributes.Ordering;
using IoDeSer.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IoDeSer.Helpers
{
    internal class IoPropertiesComparator : IComparer<PropertyInfo>
    {
        readonly IoItemsOrderAttribute itemsOrder;

        public IoPropertiesComparator(Type objectType)
        {
            itemsOrder = objectType.GetCustomAttribute<IoItemsOrderAttribute>();
        }

        public int Compare(PropertyInfo x, PropertyInfo y)
        {
            if (itemsOrder == null)
            {
                var xOrder = x.GetCustomAttribute<IoItemOrderAttribute>() == null ? sbyte.MaxValue : x.GetCustomAttribute<IoItemOrderAttribute>().Order;
                var yOrder = y.GetCustomAttribute<IoItemOrderAttribute>() == null ? sbyte.MaxValue : y.GetCustomAttribute<IoItemOrderAttribute>().Order;
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
