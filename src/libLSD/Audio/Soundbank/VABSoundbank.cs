using EZSynth.Sampler;
using EZSynth.Soundbank;
using EZSynth.Synthesizer;
using libLSD.Formats;

namespace AdpcmCs.Render
{
    public class VABSoundbank : ISoundbank
    {
        public VAB Vab;

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

                // TODO: implement proper ADSR
                VolumeEnvelope = new EnvelopeADSR
                {
                    AttackTime = 0,
                    DecayTime = 0,
                    SustainLevel = 1,
                    ReleaseTime = 0.5f
                }
            };

            return (sampler, voiceParams);
        }

        public void SetSampleRate(int sampleRateHz)
        {
            
        }

        protected float panToFloat(byte pan)
        {
            return (pan - 64f) / 64f;
        }
    }
}
