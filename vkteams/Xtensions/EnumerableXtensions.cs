using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkteams.Xtensions
{
    public static class EnumerableXtensions
    {
        public static IEnumerable<T> OrderIf<T, TKey>(this IEnumerable<T> enumerable, Func<bool> func, Func<T, TKey> selector)
        {
            if (!func())
            {
                return enumerable;
            }

            return enumerable.OrderBy(selector);
        }
    }
}
