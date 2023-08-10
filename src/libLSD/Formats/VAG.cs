using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using libLSD.Util;

namespace libLSD.Formats
{
    public class VAG
    {
        public short[] SampleData { get; protected set; }
        
        protected VagBlock[] _blocks;

        public VAG(BinaryReader br)
        {
            var blockCount = scanForBlockCount(br);
            
            // read 16 empty bytes
            br.ReadBytes(16);

            _blocks = new VagBlock[blockCount];
            for (int i = 0; i < blockCount; i++)
            {
                _blocks[i] = new VagBlock(br);
            }
            
            SampleData = loadSample();
            
            // deal with strange cases where we get 0x000777...
            // first see if we have any more data
            if (br.PeekChar() == -1) return;
            
            // then see if this data is the weird stuff
            if (br.ReadInt16() == 0x0007)
                br.ReadBytes(14); // skip it if so
            else br.BaseStream.Seek(-2, SeekOrigin.Current); // otherwise undo the read
        }

        protected short[] loadSample()
        {
            float p1 = 0, p2 = 0;
            return _blocks.SelectMany(b => b.BlockAdpcmToLinear(ref p1, ref p2)).ToArray();
        }

        protected int scanForBlockCount(BinaryReader br)
        {
            var beginPos = br.BaseStream.Position;
            br.ReadBytes(17);
            int i = 1;
            while ((br.ReadByte() & 1) == 0)
            {
                i++;
                br.BaseStream.Seek(15, SeekOrigin.Current);
            }

            br.BaseStream.Seek(beginPos, SeekOrigin.Begin);
            return i;
        }

        public struct VagBlock
        {
            public int Range => _filterRange & 0xF;
            public int Filter => (_filterRange & 0xF0) >> 4;
            public bool End => (_keyFlag & 1) == 1;

            private byte _filterRange;
            private byte _keyFlag;
            private byte[] _blockData;

            // based on: https://github.com/vgmtrans/vgmtrans/blob/master/src/main/formats/PSXSPU.cpp#L361
            public short[] BlockAdpcmToLinear(ref float prev1, ref float prev2)
            {
                short[] deltas = new short[14 * 2]; // will also be reused for PCM sample return
                var shift = Range + 16;
                
                // first load the ADPCM deltas
                for (int i = 0; i < 14; i++)
                {
                    deltas[i * 2] = (short)(AdpcmUtil.AdpcmByteToNibble(_blockData[i], AdpcmNibble.LSB) >> shift);
                    deltas[i * 2 + 1] = (short)(AdpcmUtil.AdpcmByteToNibble(_blockData[i], AdpcmNibble.MSB) >> shift);
                }
                
                // now perform the ADPCM decompression to put PCM sample data in the return array
                if (Filter > 0)
                {
                    float f1 = AdpcmUtil.Coefficients[Filter][0];
                    float f2 = AdpcmUtil.Coefficients[Filter][1];
                    float p1 = prev1;
                    float p2 = prev2;

                    for (int i = 0; i < 28; i++)
                    {
                        float t = deltas[i] + (p1 * f1) - (p2 * f2);
                        deltas[i] = (short)t;
                        p2 = p1;
                        p1 = t;
                    }

                    prev1 = p1;
                    prev2 = p2;
                }
                else
                {
                    prev2 = deltas[26];
                    prev1 = deltas[27];
                }

                return deltas;
            }

            public VagBlock(BinaryReader br)
            {
                _filterRange = br.ReadByte();
                _keyFlag = br.ReadByte();
                _blockData = br.ReadBytes(14);
            }
        }
    }
}
