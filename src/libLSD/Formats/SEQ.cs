using System.Collections.Generic;
using System.IO;
using libLSD.Audio.Sequence.Midi;

namespace libLSD.Formats
{
    public class SEQ
    {
        public SEQHeader Header;
        public List<BaseMidiEventData> TrackData;
        
        public SEQ(BinaryReader br)
        {
            Header = new SEQHeader(br);
            readTrackData(br);
        }

        protected void readTrackData(BinaryReader br)
        {
            var eventDataFactory = new MidiEventDataFactory();
            TrackData = eventDataFactory.ReadAllEventData(br);
        }
    }
    
    public struct SEQHeader
    {
        public int ID;
        public int Version;
        public int Resolution;
        public int Tempo;
        public int Rhythm;

        public const int MAGIC_NUMBER = 0x70514553; // 'pQES'

        public SEQHeader(BinaryReader br)
        {
            ID = br.ReadInt32();
            
            // TODO: throw format exception if ID doesn't match magic number
            
            Version = br.ReadInt32();
            Resolution = (br.ReadByte() << 8) + br.ReadByte();
            Tempo = (br.ReadByte() << 16) + (br.ReadByte() << 8) + br.ReadByte();
            Rhythm = br.ReadInt16();
        }
    }
}
