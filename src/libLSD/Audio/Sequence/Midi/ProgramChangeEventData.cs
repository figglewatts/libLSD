using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public class ProgramChangeEventData : MidiChannelMessageEventData
    {
        public int Program;
        
        public ProgramChangeEventData(BinaryReader br, MidiEvent midiEvent) : base(midiEvent)
        {
            Program = br.ReadByte() & 0x7F;
        }

        public override string ToString()
        {
            return $"{Event.DeltaTime}: ProgramChange (Channel: {Channel}, Program: {Program})";
        }
    }
}
