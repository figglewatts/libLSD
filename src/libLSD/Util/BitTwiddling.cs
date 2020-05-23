namespace libLSD.Util
{
    /// <summary>
    /// Contains utility methods for twiddling bits!
    /// </summary>
    public static class BitTwiddling
    {
        /// <summary>
        /// Take bits of <code>(<paramref name="overwrite"/> <![CDATA[&]]> <paramref name="mask"/>) <![CDATA[<<]]> <paramref name="shift"/></code>
        /// and overwrite the same bits of <paramref name="original"/>, returning the new value.
        /// </summary>
        /// <typeparam name="T">The type to merge.</typeparam>
        /// <param name="original">The value to merge onto.</param>
        /// <param name="overwrite">The value to overwrite with.</param>
        /// <param name="mask">Which bits to overwrite.</param>
        /// <param name="shift">How far to shift the value to overwrite.</param>
        /// <returns>The overwritten (merged) value.</returns>
        public static T Merge<T>(dynamic original, dynamic overwrite, long mask, int shift = 0)
        {
            return (T)(original ^ ((original ^ (overwrite << shift)) & mask));
        }
    }
}
