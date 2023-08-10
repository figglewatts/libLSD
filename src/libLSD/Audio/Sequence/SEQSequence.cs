using System.Collections.Generic;
using System.Linq;
using EZSynth.Sequencer;
using EZSynth.Sequencer.Event;
using libLSD.Audio.Sequence.Midi;
using libLSD.Formats;

namespace libLSD.Audio.Sequence
{
    public class SEQSequence : ISequence
    {
        protected readonly Dictionary<int, List<BaseSequenceEvent>> _sequenceData;
        protected readonly List<BaseSequenceEvent> _events;
        public SEQ Seq;

        public SEQSequence(SEQ seq)
        {
            Seq = seq;
            _sequenceData = loadSequenceData(Seq);
            _events = new List<BaseSequenceEvent>();
        }

        public int Resolution => Seq.Header.Resolution;

        public double GetLengthSeconds(double secondsPerTick)
        {
            return _sequenceData.Keys.Last() * secondsPerTick;
        }

        public IEnumerable<BaseSequenceEvent> GetEvents(int tickNumber)
        {
            _events.Clear();
            
            // add dummy tempo event at first tick
            if (tickNumber == 0)
            {
                _events.Add(new SetTempoEvent { SecondsPerBeat = microSecondsPerQuarterNoteToSecondsPerBeat(Seq.Header.Tempo)});
            }

            // get events for sequence for this tick
            if (_sequenceData.TryGetValue(tickNumber, out List<BaseSequenceEvent> value))
            {
                _events.AddRange(value);
            }

            return _events;
        }

        protected Dictionary<int, List<BaseSequenceEvent>> loadSequenceData(SEQ seq)
        {
            var result = new Dictionary<int, List<BaseSequenceEvent>>();

            void addToResult(int tick, BaseSequenceEvent sequenceEvent)
            {
                if (result.TryGetValue(tick, out List<BaseSequenceEvent> value))
                {
                    value.Add(sequenceEvent);
                }
                else
                {
                    result[tick] = new List<BaseSequenceEvent>
                        { sequenceEvent };
                }
            }

            int tickNumber = 0;
            foreach (BaseMidiEventData seqEvent in seq.TrackData)
            {
                BaseSequenceEvent toAdd;
                switch (seqEvent)
                {
                    case ControlChangeEventData controlChangeEventData:
                        toAdd = new ControlChangeEvent
                        {
                            InstrumentID = controlChangeEventData.Channel,
                            Controller = controlChangeEventData.Controller,
                            Value = controlChangeEventData.Value
                        };
                        break;
                    case NoteEventData noteEventData:
                        toAdd = new NoteEvent
                        {
                            InstrumentID = noteEventData.Channel,
                            Note = noteEventData.Note,
                            Velocity = noteEventData.Value
                        };
                        break;
                    case PitchWheelEventData pitchWheelEventData:
                        toAdd = new PitchBendEvent
                        {
                            InstrumentID = pitchWheelEventData.Channel,
                            PitchBendAmount =
                                pitchBendToFloat(pitchWheelEventData.MSB << (8 + pitchWheelEventData.LSB))
                        };
                        break;
                    case ProgramChangeEventData programChangeEventData:
                        toAdd = new ProgramChangeEvent
                        {
                            InstrumentID = programChangeEventData.Channel,
                            Program = programChangeEventData.Program
                        };
                        break;
                    case SetTempoMetaEventData setTempoMetaEventData:
                        toAdd = new SetTempoEvent
                        {
                            SecondsPerBeat = microSecondsPerQuarterNoteToSecondsPerBeat(setTempoMetaEventData.Tempo)
                        };
                        break;
                    default:
                        toAdd = null;
                        break;
                }
                tickNumber += seqEvent.Event.DeltaTime;

                if (toAdd == null) continue;
                addToResult(tickNumber, toAdd);
            }

            return result;
        }

        protected float pitchBendToFloat(int pitchBendValue)
        {
            return (pitchBendValue - 8192f) / 8192f;
        }

        protected float microSecondsPerQuarterNoteToSecondsPerBeat(float tempo)
        {
            return tempo / 1000000.0f;
        }
    }
}
