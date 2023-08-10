using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    /// <summary>
    /// Handles midi events for note off, note on, aftertouch
    /// </summary>
    public class NoteEventData : MidiChannelMessageEventData
    {
        public int Note;
        public int Value;
        
        public NoteEventData(BinaryReader br, MidiEvent midiEvent) : base(midiEvent)
        {
            Note = br.ReadByte() & 0x7F;
            Value = br.ReadByte() & 0x7F;
        }

        public override string ToString()
        {
            return $"{Event.DeltaTime}: NoteOn (Channel: {Channel}, Note: {Note}, Value: {Value})";
        }
    }
}
