using System;
using System.IO;

namespace libLSD.Audio.Soundbank
{
    [Serializable]
    public struct ToneAttribute
    {
        public byte Priority; // 0-15
        public byte Mode; // 0: normal, 4: reverb
        public byte Volume; // 0-127
        public byte Pan; // 0-127, 63: center
        public byte CenterNote; // 0-127
        public byte PitchShift; // 0-127, cents
        public byte MinNote; // 0-127
        public byte MaxNote; // 0-127
        public byte VibratoWidth; // 0-127 over one octave
        public byte VibratoPeriod; // in ticks
        public byte PortamentoWidth;
        public byte PortamentoPeriod; // in ticks
        public byte MinPitchBend;
        public byte MaxPitchBend;
        public byte Reserved1;
        public byte Reserved2;
        public AdsrEnvelope Adsr;
        public short ProgramIdx; // master program containing this sample (VAG)
        public short VagId; // this VAG's ID number
        public short[] Reserved3;

        public ToneAttribute(BinaryReader br)
        {
            Priority = br.ReadByte();
            Mode = br.ReadByte();
            Volume = br.ReadByte();
            Pan = br.ReadByte();
            CenterNote = br.ReadByte();
            PitchShift = br.ReadByte();
            MinNote = br.ReadByte();
            MaxNote = br.ReadByte();
            VibratoWidth = br.ReadByte();
            VibratoPeriod = br.ReadByte();
            PortamentoWidth = br.ReadByte();
            PortamentoPeriod = br.ReadByte();
            MinPitchBend = br.ReadByte();
            MaxPitchBend = br.ReadByte();
            Reserved1 = br.ReadByte();
            Reserved2 = br.ReadByte();
            Adsr = new AdsrEnvelope(br);
            ProgramIdx = br.ReadInt16();
            VagId = br.ReadInt16();
            Reserved3 = new short[4];
            for (int i = 0; i < 4; i++)
            {
                Reserved3[i] = br.ReadInt16();
            }
        }
    }
}
