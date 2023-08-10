using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public struct VariableLength
    {
        public int Length { get; private set; }

        private int value;

        public VariableLength(BinaryReader br)
        {
            value = 0;
            Length = 0;
            
            for (int i = 0; i < 4; i++)
            {
                var b = br.ReadByte();
                Length++;
                if ((b & 0x80) == 0) break;
            }

            // rewind so we can read the bytes
            br.BaseStream.Seek(-Length, SeekOrigin.Current);

            for (int i = Length - 1; i >= 0; i--)
            {
                int b = br.ReadByte();
                int maskedByte = b & 0x7F;
                int shiftedByte = maskedByte << (7 * i);
                value += shiftedByte;
                Length++;
                if ((b & 0x80) == 0) break;
            }
        }

        public static implicit operator int(VariableLength v) => v.value;
        
        public static implicit operator VariableLength(int i) => new VariableLength { value = i };

        public override string ToString()
        {
            return $"{value:000}";
        }
    }
}
