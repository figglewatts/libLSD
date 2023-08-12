using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using libLSD.Audio.Soundbank;

namespace libLSD.Formats
{
    /// <summary>
    /// A VAB is basically a soundfont.
    /// It's weird in that it comes in two files in LSD, the header (VH) and the body (VB). They have to be loaded
    /// together in order to use it as a soundfont.
    ///
    /// A VAB can contain up to 254 samples. A sample is also known as a VAG file.
    ///
    /// A VAB can contain up to 128 programs. A program is sort of like an instrument. I think.
    ///
    /// A program can contain up to 16 tones. A tone is a sample along with data about how to play it.
    /// </summary>
    [Serializable]
    public class VAB
    {
        public VABHeader Header;
        public List<VABProgram> Programs;

        public List<VAG> Samples => _vags.ToList();

        protected readonly VAG[] _vags;
        protected readonly int[] _vagOffsets;
        protected readonly ProgramAttribute[] _programTable;
        protected readonly ToneAttribute[] _toneTable;
        
        protected const int MAX_PROGRAMS = 128;
        protected const int MAX_TONES = 16;

        public VAB(BinaryReader vabBr, BinaryReader vagBr)
        {
            Header = new VABHeader(vabBr);
            _programTable = new ProgramAttribute[MAX_PROGRAMS];
            for (int i = 0; i < MAX_PROGRAMS; i++)
            {
                _programTable[i] = new ProgramAttribute(vabBr);
            }

            _toneTable = new ToneAttribute[MAX_PROGRAMS * MAX_TONES];
            for (int i = 0; i < Header.ProgramCount * MAX_TONES; i++)
            {
                _toneTable[i] = new ToneAttribute(vabBr);
            }

            _vagOffsets = new int[Header.VagCount];
            for (int i = 0; i < Header.VagCount; i++)
            {
                int rawOffset = vabBr.ReadUInt16() << 3;
                if (i > 0) rawOffset += _vagOffsets[i - 1];
                _vagOffsets[i] = rawOffset;
            }

            _vags = new VAG[Header.VagCount];
            for (int i = 0; i < _vagOffsets.Length; i++)
            {
                vagBr.BaseStream.Seek(_vagOffsets[i], SeekOrigin.Begin);
                _vags[i] = new VAG(vagBr);
            }
            
            setupData();
        }

        protected void setupData()
        {
            Programs = new List<VABProgram>();
            int toneBase = 0;
            foreach (var program in _programTable)
            {
                if (program.MasterVolume <= 0) continue;
                
                var vabProgram = new VABProgram
                {
                    Attributes = program,
                    Tones = new List<VABTone>()
                };
                for (int i = 0; i < program.ToneCount; i++)
                {
                    var toneAttributes = _toneTable[toneBase + i];
                    if (toneAttributes.VagId == 0) continue;
                    
                    vabProgram.Tones.Add(new VABTone
                    {
                        Attributes = toneAttributes,
                        Sample = _vags[toneAttributes.VagId - 1]
                    });
                }
                Programs.Add(vabProgram);

                toneBase += MAX_TONES;
            }
        }
        
        [Serializable]
        public class VABHeader
        {
            public uint Magic;
            public uint Version;
            public uint VabId;
            public uint WaveformSize;
            public ushort Reserved1;
            public ushort ProgramCount;
            public ushort ToneCount;
            public ushort VagCount;
            public byte MasterVolume;
            public byte MasterPan;
            public byte BankAttr1;
            public byte BankAttr2;
            public uint Reserved2;

            private const int MAGIC_NUMBER = 0x70424156;

            public VABHeader(BinaryReader br)
            {
                Magic = br.ReadUInt32();
                if (Magic != MAGIC_NUMBER)
                {
                    // TODO: throw bad format exception
                }

                Version = br.ReadUInt32();
                VabId = br.ReadUInt32();
                WaveformSize = br.ReadUInt32();
                Reserved1 = br.ReadUInt16();
                ProgramCount = br.ReadUInt16();
                ToneCount = br.ReadUInt16();
                VagCount = br.ReadUInt16();
                MasterVolume = br.ReadByte();
                MasterPan = br.ReadByte();
                BankAttr1 = br.ReadByte();
                BankAttr2 = br.ReadByte();
                Reserved2 = br.ReadUInt32();
            }
        }
    }
}
