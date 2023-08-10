using System.IO;

namespace libLSD.Audio.Soundbank
{
    public struct AdsrEnvelope
    {
        public int SustainLevel
        {
            get => _adsr1 & SUSTAIN_LEVEL_MASK;
            set => _adsr1 = (short)((_adsr1 & ~SUSTAIN_LEVEL_MASK) | value);
        }

        public int DecayShift
        {
            get => (_adsr1 & DECAY_SHIFT_MASK) >> 5;
            set => _adsr1 = (short)((_adsr1 & ~DECAY_SHIFT_MASK) | (value << 5));
        }

        public int AttackShift
        {
            get => (_adsr1 & ATTACK_SHIFT_MASK) >> 9;
            set => _adsr1 = (short)((_adsr1 & ~ATTACK_SHIFT_MASK) | (value << 9));
        }

        public RateMode AttackRateMode
        {
            get => (RateMode)((_adsr1 & ATTACK_MODE_MASK) >> 15);
            set => _adsr1 = (short)((_adsr1 & ~ATTACK_MODE_MASK) | ((int)value << 15));
        }

        public int ReleaseShift
        {
            get => _adsr2 & RELEASE_SHIFT_MASK;
            set => _adsr2 = (short)((_adsr2 & ~RELEASE_SHIFT_MASK) | value);
        }

        public RateMode ReleaseRateMode
        {
            get => (RateMode)((_adsr2 & RELEASE_MODE_MASK) >> 5);
            set => _adsr2 = (short)((_adsr2 & ~RELEASE_MODE_MASK) | (int)value);
        }

        public int SustainShift
        {
            get => (_adsr2 & SUSTAIN_SHIFT_MASK) >> 6;
            set => _adsr2 = (short)((_adsr2 & ~SUSTAIN_SHIFT_MASK) | (value << 6));
        }

        public bool SustainNegative
        {
            get => (_adsr2 & SUSTAIN_SIGN_MASK) >> 14 == 1;
            set => _adsr2 = (short)((_adsr2 & ~SUSTAIN_SIGN_MASK) | ((value ? 1 : 0) << 14));
        }

        public RateMode SustainMode
        {
            get => (RateMode)((_adsr2 & SUSTAIN_MODE_MASK) >> 15);
            set => _adsr2 = (short)((_adsr2 & ~SUSTAIN_MODE_MASK) | (int)value);
        }
        
        private short _adsr1;
        private short _adsr2;

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
            _adsr1 = br.ReadInt16();
            _adsr2 = br.ReadInt16();
        }

        public enum RateMode
        {
            Linear = 0,
            Exponential = 1
        }
    }
}
