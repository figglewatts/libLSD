using System.IO;
using libLSD.Exceptions;
using libLSD.Interfaces;

namespace libLSD.Formats
{
    /// <summary>
    /// An MOS file is a container for TOD animation data. It's found in MOM files.
    /// </summary>
    public struct MOS : IWriteable
    {
        /// <summary>
        /// The ID of this MOS file. Always "MOS ".
        /// </summary>
        public readonly uint ID;

        /// <summary>
        /// The number of TOD files in this MOS.
        /// </summary>
        public readonly uint NumberOfTODs;

        /// <summary>
        /// The offsets to TOD files from the top of this MOS file.
        /// </summary>
        public readonly uint[] TODOffsets;

        /// <summary>
        /// The TOD files.
        /// </summary>
        public readonly TOD[] TODs;

        private uint _mosTop;

        private const int MAGIC_NUMBER = 0x20534F4D;

        /// <summary>
        /// Create a new MOS file by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <exception cref="BadFormatException">If the MOS file did not have the correct ID.</exception>
        public MOS(BinaryReader br)
        {
            _mosTop = (uint)br.BaseStream.Position;

            ID = br.ReadUInt32();

            if (ID != 0x20534F4D)
                throw new BadFormatException("MOS did not have correct magic number!");

            NumberOfTODs = br.ReadUInt32();

            TODOffsets = new uint[NumberOfTODs];
            for (int i = 0; i < NumberOfTODs; i++)
            {
                TODOffsets[i] = br.ReadUInt32();
            }

            TODs = new TOD[NumberOfTODs];
            for (int i = 0; i < NumberOfTODs; i++)
            {
                br.BaseStream.Seek(_mosTop + TODOffsets[i], SeekOrigin.Begin);
                TODs[i] = new TOD(br);
            }
        }

        /// <summary>
        /// Write this MOS file to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            uint mosTop = (uint)bw.BaseStream.Position;

            bw.Write(ID);
            bw.Write(NumberOfTODs);
            foreach (uint offset in TODOffsets)
            {
                bw.Write(offset);
            }

            int i = 0;
            foreach (var tod in TODs)
            {
                bw.BaseStream.Seek(mosTop + TODOffsets[i], SeekOrigin.Begin);
                tod.Write(bw);

                i++;
            }
        }
    }
}
