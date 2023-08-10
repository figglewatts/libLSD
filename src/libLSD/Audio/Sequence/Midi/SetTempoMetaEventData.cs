using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    
    public class SetTempoMetaEventData : MetaEventData
    {
        public int Tempo;

        public SetTempoMetaEventData(BinaryReader br, MidiEvent midiEvent) : base(br, midiEvent)
        {
            Tempo = (br.ReadByte() << 16) + (br.ReadByte() << 8) + br.ReadByte();
        }

        public override string ToString()
        {
            return $"{Event.DeltaTime}: SetTempo (Tempo: {Tempo})";
        }
    }
}
