using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public class PitchWheelEventData : MidiChannelMessageEventData
    {
        public int MSB;
        public int LSB;
        
        public PitchWheelEventData(BinaryReader br, MidiEvent midiEvent) : base(midiEvent)
        {
            MSB = br.ReadByte() & 0x7F;
            LSB = br.ReadByte() & 0x7F;
        }

        public override string ToString()
        {
            return $"{Event.DeltaTime}: PitchWheel (Channel: {Channel}, MSB: {MSB}, LSB: {LSB})";
        }
    }
}
