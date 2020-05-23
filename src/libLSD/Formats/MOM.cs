using System.IO;
using libLSD.Exceptions;
using libLSD.Interfaces;

namespace libLSD.Formats
{
    /// <summary>
    /// A MOM file is an interactive object in LSD. It has a TMD, and an MOS file, which
    /// is a container for animation data.
    /// </summary>
    public struct MOM : IWriteable
    {
        /// <summary>
        /// The ID of this MOM file. Always "MOM ".
        /// </summary>
        public readonly uint ID;

        /// <summary>
        /// The length of this file in bytes.
        /// </summary>
        public readonly uint MOMLength;

        /// <summary>
        /// The offset of the TMD file from the top of the MOM.
        /// </summary>
        public readonly uint TMDOffset;

        /// <summary>
        /// The MOS file, which contains the TOD animations for this MOM.
        /// </summary>
        public readonly MOS MOS;

        /// <summary>
        /// The TMD file containing the 3D model data for this MOM.
        /// </summary>
        public readonly TMD TMD;

        private uint _momTop;

        private const int MAGIC_NUMBER = 0x204D4F4D;

        /// <summary>
        /// Create a new MOM file by reading from a binary stream.
        /// </summary>
        /// <param name="br">The binary stream.</param>
        /// <exception cref="BadFormatException">If the MOM file did not have the correct ID.</exception>
        public MOM(BinaryReader br)
        {
            _momTop = (uint)br.BaseStream.Position;

            ID = br.ReadUInt32();
            if (ID != MAGIC_NUMBER)
                throw new BadFormatException("MOM file did not have correct magic number!");

            MOMLength = br.ReadUInt32();
            TMDOffset = br.ReadUInt32();
            MOS = new MOS(br);

            br.BaseStream.Seek(_momTop + TMDOffset, SeekOrigin.Begin);
            TMD = new TMD(br);
        }

        /// <summary>
        /// Write this MOM file to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        public void Write(BinaryWriter bw)
        {
            uint momTop = (uint)bw.BaseStream.Position;

            bw.Write(ID);
            bw.Write(MOMLength);
            bw.Write(TMDOffset);
            MOS.Write(bw);

            bw.BaseStream.Seek(momTop + TMDOffset, SeekOrigin.Begin);
            TMD.Write(bw);
        }
    }
}
