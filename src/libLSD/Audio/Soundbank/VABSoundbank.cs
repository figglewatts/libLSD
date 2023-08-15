using System;
using EZSynth.Sampler;
using EZSynth.Soundbank;
using EZSynth.Synthesizer;
using libLSD.Formats;

namespace libLSD.Audio.Soundbank
{
    public class VABSoundbank : ISoundbank
    {
        public VAB Vab;

        public bool LongRelease = false;

        protected int _sampleRate;

        public VABSoundbank(VAB vab)
        {
            Vab = vab;
        }
        
        public (ISampler, VoiceParameters) GetSampler(int programNumber, int note, int velocity)
        {
            var program = Vab.Programs[programNumber];
            var tone = program.GetToneForNote(note);
            if (tone == null) return (null, new VoiceParameters());
            var sample = tone.Sample;
            var sampler = new PCMSampler(sample.SampleData, 44100);
            sampler.PlayingNote = note;
            sampler.LoopSample = false;
            sampler.RootNote = tone.Attributes.CenterNote - 2;

            byte pan = program.Attributes.Pan == 63 ? tone.Attributes.Pan : program.Attributes.Pan;

            var voiceParams = new VoiceParameters
            {
                Volume = Math.Min(tone.Attributes.Volume, program.Attributes.MasterVolume) / 127f,
                Pan = panToFloat(pan),
                Pitch = tone.Attributes.PitchShift / (float)(tone.Attributes.MaxPitchBend),
                Velocity = velocity / 127f,
                VolumeEnvelope = vabAdsrToEnvelopeADSR(tone.Attributes.Adsr)
            };

            return (sampler, voiceParams);
        }

        public void SetSampleRate(int sampleRateHz)
        {
            _sampleRate = sampleRateHz;
        }
        
        protected EnvelopeADSR vabAdsrToEnvelopeADSR(AdsrEnvelope envelope)
        {
            int shiftToCycles(double factor, int shift)
            {
                return (int)(factor * (1 << shift));
            }

            var attackCycles = shiftToCycles(0.0023d, envelope.AttackShift) >> 5;
            var decayCycles = shiftToCycles(0.0015d, envelope.DecayShift);
            var releaseCycles = shiftToCycles(0.0023d, envelope.ReleaseShift);

            if (LongRelease) releaseCycles = 3000;

            var step = 11025f / 20f;
            return new EnvelopeADSR
            {
                AttackTime = attackCycles / step,
                DecayTime = decayCycles / step,
                SustainLevel = 1,
                ReleaseTime = releaseCycles / step
            };
        }

        protected float panToFloat(byte pan)
        {
            return (pan - 64f) / 64f;
        }
    }
}
