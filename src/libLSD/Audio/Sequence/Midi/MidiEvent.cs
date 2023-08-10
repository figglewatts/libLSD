using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public struct MidiEvent
    {
        public VariableLength DeltaTime;
        public int Status;

        public MidiEvent(BinaryReader br)
        {
            DeltaTime = new VariableLength(br);
            Status = br.ReadByte();
        }

        public bool ValidStatus => (Status & 0x80) > 0;
    }
}
