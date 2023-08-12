using System;
using System.IO;

namespace libLSD.Audio.Soundbank
{
    [Serializable]
    public struct AdsrEnvelope
    {
        public int SustainLevel
        {
            get => Adsr1 & SUSTAIN_LEVEL_MASK;
            set => Adsr1 = (short)((Adsr1 & ~SUSTAIN_LEVEL_MASK) | value);
        }

        public int DecayShift
        {
            get => (Adsr1 & DECAY_SHIFT_MASK) >> 5;
            set => Adsr1 = (short)((Adsr1 & ~DECAY_SHIFT_MASK) | (value << 5));
        }

        public int AttackShift
        {
            get => (Adsr1 & ATTACK_SHIFT_MASK) >> 9;
            set => Adsr1 = (short)((Adsr1 & ~ATTACK_SHIFT_MASK) | (value << 9));
        }

        public RateMode AttackRateMode
        {
            get => (RateMode)((Adsr1 & ATTACK_MODE_MASK) >> 15);
            set => Adsr1 = (short)((Adsr1 & ~ATTACK_MODE_MASK) | ((int)value << 15));
        }

        public int ReleaseShift
        {
            get => Adsr2 & RELEASE_SHIFT_MASK;
            set => Adsr2 = (short)((Adsr2 & ~RELEASE_SHIFT_MASK) | value);
        }

        public RateMode ReleaseRateMode
        {
            get => (RateMode)((Adsr2 & RELEASE_MODE_MASK) >> 5);
            set => Adsr2 = (short)((Adsr2 & ~RELEASE_MODE_MASK) | (int)value);
        }

        public int SustainShift
        {
            get => (Adsr2 & SUSTAIN_SHIFT_MASK) >> 6;
            set => Adsr2 = (short)((Adsr2 & ~SUSTAIN_SHIFT_MASK) | (value << 6));
        }

        public bool SustainNegative
        {
            get => (Adsr2 & SUSTAIN_SIGN_MASK) >> 14 == 1;
            set => Adsr2 = (short)((Adsr2 & ~SUSTAIN_SIGN_MASK) | ((value ? 1 : 0) << 14));
        }

        public RateMode SustainMode
        {
            get => (RateMode)((Adsr2 & SUSTAIN_MODE_MASK) >> 15);
            set => Adsr2 = (short)((Adsr2 & ~SUSTAIN_MODE_MASK) | (int)value);
        }
        
        public short Adsr1;
        public short Adsr2;

        private const int SUSTAIN_LEVEL_MASK = 0b11111;
        private const int DECAY_SHIFT_MASK = 0b1111 << 5;
        private const int ATTACK_SHIFT_MASK = 0b11111 << 9;
        private const int ATTACK_MODE_MASK = 1 << 15;
        private const int RELEASE_SHIFT_MASK = 0b11111;
        private const int RELEASE_MODE_MASK = 1 << 5;
        private const int SUSTAIN_SHIFT_MASK = 0b111111 << 6;
        private const int SUSTAIN_SIGN_MASK = 1 << 14;
        private const int SUSTAIN_MODE_MASK = 1 << 15;
        
        public AdsrEnvelope(BinaryReader br)
        {
            Adsr1 = br.ReadInt16();
            Adsr2 = br.ReadInt16();
        }

        [Serializable]
        public enum RateMode
        {
            Linear = 0,
            Exponential = 1
        }
    }
}
