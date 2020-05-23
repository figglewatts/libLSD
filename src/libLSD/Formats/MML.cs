using System.IO;
using libLSD.Exceptions;
using libLSD.Interfaces;

namespace libLSD.Formats
{
    /// <summary>
    /// An MML file is a container for MOM interactive object files, found in LBD files.
    /// </summary>
    public struct MML : IWriteable
    {
        /// <summary>
        /// The magic number of the file. Always "MML ".
        /// </summary>
        public readonly uint ID;

        /// <summary>
        /// The number of MOMs in this MML file.
        /// </summary>
        public readonly uint NumberOfMOMs;

        /// <summary>
        /// The offsets to each MOM from the top of the MML file.
        /// </summary>
        public readonly uint[] MOMOffsets;

        /// <summary>
        /// The MOM files.
        /// </summary>
        public readonly MOM[] MOMs;

        private uint _mmlTop;

        private const int MAGIC_NUMBER = 0x204C4D4D;

        /// <summary>
        /// Create a new MML file from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <exception cref="BadFormatException">If the MML file did not have the correct ID.</exception>
        public MML(BinaryReader br)
        {
            _mmlTop = (uint)br.BaseStream.Position;

            ID = br.ReadUInt32();
            if (ID != MAGIC_NUMBER)
                throw new BadFormatException("MML file did not have correct magic number!");

            NumberOfMOMs = br.ReadUInt32();

            MOMOffsets = new uint[NumberOfMOMs];
            for (int i = 0; i < NumberOfMOMs; i++)
            {
                MOMOffsets[i] = br.ReadUInt32();
            }

            MOMs = new MOM[NumberOfMOMs];
            for (int i = 0; i < NumberOfMOMs; i++)
            {
                br.BaseStream.Seek(_mmlTop + MOMOffsets[i], SeekOrigin.Begin);
                MOMs[i] = new MOM(br);
            }
        }

        /// <summary>
        /// Write this MML file to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            uint mmlTop = (uint)bw.BaseStream.Position;

            bw.Write(ID);
            bw.Write(NumberOfMOMs);
            foreach (uint offset in MOMOffsets)
            {
                bw.Write(offset);
            }

            int i = 0;
            foreach (var mom in MOMs)
            {
                bw.BaseStream.Seek(mmlTop + MOMOffsets[i], SeekOrigin.Begin);
                mom.Write(bw);

                i++;
            }
        }
    }
}
