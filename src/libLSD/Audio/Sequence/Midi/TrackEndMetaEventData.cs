using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public class TrackEndMetaEventData : MetaEventData
    {
        public TrackEndMetaEventData(BinaryReader br, MidiEvent midiEvent) : base(br, midiEvent) { }
    }
}
