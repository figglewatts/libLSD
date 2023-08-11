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

        protected int _sampleRate;

        public VABSoundbank(VAB vab)
        {
            Vab = vab;
        }
        
        public (ISampler, VoiceParameters) GetSampler(int programNumber, int note, int velocity)
        {
            var tone = Vab.Programs[programNumber].GetToneForNote(note);
            if (tone == null) tone = Vab.Programs[programNumber].Tones[0];
            var sample = tone.Sample;
            var sampler = new PCMSampler(sample.SampleData, 44100);
            sampler.PlayingNote = note;
            sampler.LoopSample = false;
            sampler.RootNote = tone.Attributes.CenterNote - 2;

            var voiceParams = new VoiceParameters
            {
                Volume = tone.Attributes.Volume / 127f,
                Pan = panToFloat(tone.Attributes.Pan),
                Pitch = tone.Attributes.PitchShift / 200f, // 2 semitones
                Velocity = velocity,
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
            var attackCycles = 1 << Math.Max(0, envelope.AttackShift - 11);
            var decayCycles = 1 << Math.Max(0, envelope.DecayShift - 11);
            var releaseCycles = 1 << Math.Max(0, envelope.ReleaseShift - 11);

            return new EnvelopeADSR
            {
                AttackTime = attackCycles / (float)_sampleRate,
                DecayTime = decayCycles / (float)_sampleRate,
                SustainLevel = 1,
                ReleaseTime = releaseCycles / (float)_sampleRate
            };
        }

        protected float panToFloat(byte pan)
        {
            return (pan - 64f) / 64f;
        }
    }
}
