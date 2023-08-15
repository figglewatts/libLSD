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
            double length = 0;
            secondsPerTick = microSecondsPerQuarterNoteToSecondsPerBeat(500000) / Resolution;

            int lastTick = 0;
            foreach (var kvp in _sequenceData)
            {
                int tickNum = kvp.Key;
                var events = kvp.Value;

                bool hasTempo = events.Count(e => e is SetTempoEvent) > 0;
                if (hasTempo && events.Last(e => e is SetTempoEvent) is SetTempoEvent tempoEvent)
                {
                    secondsPerTick = tempoEvent.SecondsPerBeat / Resolution;
                }

                length += secondsPerTick * (tickNum - lastTick);
                lastTick = tickNum;
            }
            if (length == 0) length = 0.1;
            return length;
        }

        public IEnumerable<BaseSequenceEvent> GetEvents(int tickNumber)
        {
            _events.Clear();

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
            
            // add tempo event at first tick to represent file tempo
            addToResult(0, new SetTempoEvent
            {
                SecondsPerBeat = microSecondsPerQuarterNoteToSecondsPerBeat(Seq.Header.Tempo)
            });
            
            foreach (BaseMidiEventData seqEvent in seq.TrackData)
            {
                tickNumber += seqEvent.Event.DeltaTime;
                
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
