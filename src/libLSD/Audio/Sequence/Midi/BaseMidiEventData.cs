namespace libLSD.Audio.Sequence.Midi
{
    public abstract class BaseMidiEventData
    {
        public MidiEvent Event;

        protected BaseMidiEventData(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }
    }
}
