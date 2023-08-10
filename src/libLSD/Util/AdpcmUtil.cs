namespace libLSD.Util
{
    public enum AdpcmNibble
    {
        MSB,
        LSB
    }
    
    public static class AdpcmUtil
    {
        public static readonly float[][] Coefficients =
        {
            new[] { 0.0f, 0.0f },
            new[] { 60.0f / 64.0f, 0.0f },
            new[] { 115.0f / 64.0f, 52.0f / 64.0f },
            new[] { 98.0f / 64.0f, 55.0f / 64.0f },
            new[] { 122.0f / 64.0f, 60.0f / 64.0f }
        };
        
        public static int AdpcmByteToNibble(byte adpcm, AdpcmNibble nibble)
        {
            int adpcmNibble = nibble == AdpcmNibble.MSB ? (adpcm & 0xF0) << 24 : adpcm << 28;
            bool negative = (adpcm & 0x8) == 1;
            if (negative) adpcmNibble *= -1;
            return adpcmNibble;
        }
    }
}
