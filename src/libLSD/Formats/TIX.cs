using System.Collections.Generic;
using System.IO;
using libLSD.Interfaces;

namespace libLSD.Formats
{
    /// <summary>
    /// A TIX file is a collection of TIM texture data, organised into VRAM pages. Each level has TIX files containing
    /// every texture used in the level, so they can all be loaded into PSX VRAM at once.
    ///
    /// A TIX file is essentially a collection of 'chunks' 
    /// </summary>
    public struct TIX : IWriteable
    {
        /// <summary>
        /// The header of this TIX file.
        /// </summary>
        public readonly TIXHeader Header;

        /// <summary>
        /// The chunks contained within this TIX file.
        /// </summary>
        public readonly TIXChunk[] Chunks;

        /// <summary>
        /// Get a list of every TIM contained within this TIX file, regardless of which chunk they're in.
        /// </summary>
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

        /// <summary>
        /// Read a TIX file from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
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

        /// <summary>
        /// Write this TIX file to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
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

    /// <summary>
    /// The header of a TIX file.
    /// </summary>
    public struct TIXHeader : IWriteable
    {
        /// <summary>
        /// The number of chunks in this TIX file.
        /// </summary>
        public readonly uint NumberOfChunks;

        /// <summary>
        /// The offsets to the chunks, from the top of the TIX file.
        /// </summary>
        public readonly uint[] ChunkOffsets;

        /// <summary>
        /// The lengths of the chunks in bytes.
        /// </summary>
        public readonly uint[] ChunkLengths;

        /// <summary>
        /// Read a TIX file from a binary stream.
        /// </summary>
        /// <param name="br"></param>
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

        /// <summary>
        /// Write this TIX file to a binary stream.
        /// </summary>
        /// <param name="bw"></param>
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

    /// <summary>
    /// A TIX chunk, containing a number of TIM textures.
    /// </summary>
    public struct TIXChunk : IWriteable
    {
        /// <summary>
        /// The number of TIM textures contained within this chunk.
        /// </summary>
        public readonly uint NumberOfTIMs;

        /// <summary>
        /// The offsets to the TIM textures, from the top of this chunk.
        /// </summary>
        public readonly uint[] TIMOffsets;

        /// <summary>
        /// The TIM data.
        /// </summary>
        public readonly TIM[] TIMs;

        private readonly uint _chunkTop;

        /// <summary>
        /// Create a new TIX chunk by reading it from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
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

        /// <summary>
        /// Write this TIX chunk to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
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
