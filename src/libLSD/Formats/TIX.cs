using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Interfaces;

namespace libLSD.Formats
{
    public struct TIX : IWriteable
    {
        public readonly TIXHeader Header;
        public readonly TIXChunk[] Chunks;

        public List<TIM> AllTIMs
        {
            get
            {
                List<TIM> tims = new List<TIM>();
                foreach (var chunk in Chunks)
                {
                    foreach (var tim in chunk.TIMs)
                    {
                        tims.Add(tim);
                    }
                }
                return tims;
            }
        }

        public TIX(BinaryReader br)
        {
            Header = new TIXHeader(br);
            Chunks = new TIXChunk[Header.NumberOfChunks];

            int chunk = 0;
            foreach (uint offset in Header.ChunkOffsets)
            {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                Chunks[chunk] = new TIXChunk(br);

                chunk++;
            }
        }

        public void Write(BinaryWriter bw)
        {
            Header.Write(bw);

            int chunk = 0;
            foreach (uint offset in Header.ChunkOffsets)
            {
                bw.BaseStream.Seek(offset, SeekOrigin.Begin);
                Chunks[chunk].Write(bw);

                chunk++;
            }
        }
    }

    public struct TIXHeader : IWriteable
    {
        public readonly uint NumberOfChunks;
        public readonly uint[] ChunkOffsets;
        public readonly uint[] ChunkLengths;

        public TIXHeader(BinaryReader br)
        {
            NumberOfChunks = br.ReadUInt32();
            ChunkOffsets = new uint[NumberOfChunks];
            ChunkLengths = new uint[NumberOfChunks];

            for (int i = 0; i < NumberOfChunks; i++)
            {
                ChunkOffsets[i] = br.ReadUInt32();
            }

            for (int i = 0; i < NumberOfChunks; i++)
            {
                ChunkLengths[i] = br.ReadUInt32();
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(NumberOfChunks);
            foreach (uint offset in ChunkOffsets)
            {
                bw.Write(offset);
            }
            foreach (uint length in ChunkLengths)
            {
                bw.Write(length);
            }
        }
    }

    public struct TIXChunk : IWriteable
    {
        public readonly uint NumberOfTIMs;
        public readonly uint[] TIMOffsets;
        public readonly TIM[] TIMs;

        private readonly uint _chunkTop;

        public TIXChunk(BinaryReader br)
        {
            _chunkTop = (uint)br.BaseStream.Position;

            NumberOfTIMs = br.ReadUInt32();
            TIMOffsets = new uint[NumberOfTIMs];
            TIMs = new TIM[NumberOfTIMs];

            for (int i = 0; i < NumberOfTIMs; i++)
            {
                TIMOffsets[i] = br.ReadUInt32();
            }

            int tim = 0;
            foreach (uint offset in TIMOffsets)
            {
                br.BaseStream.Seek(_chunkTop + offset, SeekOrigin.Begin);
                TIMs[tim] = new TIM(br);

                tim++;
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(NumberOfTIMs);
            foreach (uint offset in TIMOffsets)
            {
                bw.Write(offset);
            }

            int tim = 0;
            foreach (uint offset in TIMOffsets)
            {
                bw.BaseStream.Seek(_chunkTop + offset, SeekOrigin.Begin);
                TIMs[tim].Write(bw);

                tim++;
            }
        }
    }
}
