using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public abstract class MetaEventData : BaseMidiEventData
    {
        public int Type;
        public int Length;

        protected MetaEventData(BinaryReader br, MidiEvent midiEvent) : base(midiEvent)
        {
            Type = br.ReadByte();
            Length = br.ReadByte();
        }
        
        public static int PeekType(BinaryReader br)
        {
            byte peekedType = br.ReadByte();

            // seek back to start of event, so we can read it in full
            br.BaseStream.Seek(-1, SeekOrigin.Current);

            // return the status
            return peekedType;
        }
    }
}
