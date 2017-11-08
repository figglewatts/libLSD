using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libLSD.Util
{
    public static class BitTwiddling
    {
        /// <summary>
        /// Take bits of <code>(<paramref name="overwrite"/> & <paramref name="mask"/>) &lt;&lt; <paramref name="shift"/></code>
        /// and overwrite the same bits of <paramref name="original"/>, returning the new value.
        /// </summary>
        /// <typeparam name="T">The type to merge.</typeparam>
        /// <param name="original">The value to merge onto.</param>
        /// <param name="overwrite">The value to overwrite with.</param>
        /// <param name="mask">Which bits to overwrite.</param>
        /// <param name="shift">How far to shift the value to overwrite.</param>
        /// <returns>The overwritten (merged) value.</returns>
        public static T Merge<T>(dynamic original, dynamic overwrite, uint mask, int shift = 0)
        {
            return (T)(original ^ ((original ^ (overwrite << shift)) & mask));
        }
    }
}
