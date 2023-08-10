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
        /// <param name="original">The value to merge onto.</param>
        /// <param name="overwrite">The value to overwrite with.</param>
        /// <param name="mask">Which bits to overwrite.</param>
        /// <param name="shift">How far to shift the value to overwrite.</param>
        /// <returns>The overwritten (merged) value.</returns>
        public static int Merge(int original, int overwrite, long mask, int shift = 0)
        {
            return (int)(original ^ ((original ^ (overwrite << shift)) & mask));
        }
        
        /// <summary>
        /// Take bits of <code>(<paramref name="overwrite"/> <![CDATA[&]]> <paramref name="mask"/>) <![CDATA[<<]]> <paramref name="shift"/></code>
        /// and overwrite the same bits of <paramref name="original"/>, returning the new value.
        /// </summary>
        /// <param name="original">The value to merge onto.</param>
        /// <param name="overwrite">The value to overwrite with.</param>
        /// <param name="mask">Which bits to overwrite.</param>
        /// <param name="shift">How far to shift the value to overwrite.</param>
        /// <returns>The overwritten (merged) value.</returns>
        public static long Merge(long original, long overwrite, long mask, int shift = 0)
        {
            return (long)(original ^ ((original ^ (overwrite << shift)) & mask));
        }
        
        /// <summary>
        /// Take bits of <code>(<paramref name="overwrite"/> <![CDATA[&]]> <paramref name="mask"/>) <![CDATA[<<]]> <paramref name="shift"/></code>
        /// and overwrite the same bits of <paramref name="original"/>, returning the new value.
        /// </summary>
        /// <param name="original">The value to merge onto.</param>
        /// <param name="overwrite">The value to overwrite with.</param>
        /// <param name="mask">Which bits to overwrite.</param>
        /// <param name="shift">How far to shift the value to overwrite.</param>
        /// <returns>The overwritten (merged) value.</returns>
        public static byte Merge(byte original, byte overwrite, long mask, int shift = 0)
        {
            return (byte)(original ^ ((original ^ (overwrite << shift)) & mask));
        }
        
        /// <summary>
        /// Take bits of <code>(<paramref name="overwrite"/> <![CDATA[&]]> <paramref name="mask"/>) <![CDATA[<<]]> <paramref name="shift"/></code>
        /// and overwrite the same bits of <paramref name="original"/>, returning the new value.
        /// </summary>
        /// <param name="original">The value to merge onto.</param>
        /// <param name="overwrite">The value to overwrite with.</param>
        /// <param name="mask">Which bits to overwrite.</param>
        /// <param name="shift">How far to shift the value to overwrite.</param>
        /// <returns>The overwritten (merged) value.</returns>
        public static ushort Merge(ushort original, ushort overwrite, long mask, int shift = 0)
        {
            return (ushort)(original ^ ((original ^ (overwrite << shift)) & mask));
        }
    }
}
