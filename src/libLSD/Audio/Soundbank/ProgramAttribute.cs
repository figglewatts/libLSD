using System.IO;

namespace libLSD.Audio.Soundbank
{
    public struct ProgramAttribute
    {
        public byte ToneCount; // number of ToneAttributes in this program
        public byte MasterVolume; // 0-127 volume of this program
        public byte Priority; // 0-15
        public byte Mode; // 0: normal, 4: reverb
        public byte Pan; // 0-127, 63: center
        public byte Reserved1;
        public short Attribute; // program attribute?
        public uint Reserved2;
        public uint Reserved3;
        
        public ProgramAttribute(BinaryReader br)
        {
            ToneCount = br.ReadByte();
            MasterVolume = br.ReadByte();
            Priority = br.ReadByte();
            Mode = br.ReadByte();
            Pan = br.ReadByte();
            Reserved1 = br.ReadByte();
            Attribute = br.ReadInt16();
            Reserved2 = br.ReadUInt32();
            Reserved3 = br.ReadUInt32();
        }
    }
}
