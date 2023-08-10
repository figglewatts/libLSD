using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public class ControlChangeEventData : MidiChannelMessageEventData
    {
        public int Controller;
        public int Value;

        public ControlChangeEventData(BinaryReader br, MidiEvent midiEvent) : base(midiEvent)
        {
            Controller = br.ReadByte() & 0x7F;
            Value = br.ReadByte() & 0x7F;
        }

        public override string ToString()
        {
            return $"{Event.DeltaTime}: ControlChange (Channel: {Channel}, Controller: {Controller}, Value: {Value})";
        }
    }
}
