using System.IO;

namespace libLSD.Interfaces
{
    /// <summary>
    /// Something that can be written to a given binary stream.
    /// </summary>
    public interface IWriteable
    {
        /// <summary>
        /// Write to a binary stream.
        /// </summary>
        /// <param name="bw">The binary stream.</param>
        void Write(BinaryWriter bw);
    }
}
