namespace libLSD.Audio.Sequence.Midi
{
    public abstract class MidiChannelMessageEventData : BaseMidiEventData
    {
        public int Channel => Event.Status & 0xF;
        
        protected MidiChannelMessageEventData(MidiEvent midiEvent) : base(midiEvent) { }
    }
}
