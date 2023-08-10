using System;
using System.Collections.Generic;
using System.IO;

namespace libLSD.Audio.Sequence.Midi
{
    public class MidiEventDataFactory
    {
        protected int _lastStatus = -1;

        /// <summary>
        /// Reads midi event data from br until we get a track end event.
        /// The track end event is included in the result list.
        /// </summary>
        /// <param name="br">The BinaryReader to read midi events from.</param>
        /// <returns>The list of midi events loaded.</returns>
        public List<BaseMidiEventData> ReadAllEventData(BinaryReader br)
        {
            List<BaseMidiEventData> result = new List<BaseMidiEventData>();
            while (true)
            {
                var eventData = ReadEventData(br);
                //Console.WriteLine($"{br.BaseStream.Position:X} / {br.BaseStream.Length:X}\t\t{eventData}");
                result.Add(eventData);
                if (eventData is TrackEndMetaEventData) break;
            }
            return result;
        }

        public BaseMidiEventData ReadEventData(BinaryReader br)
        {
            MidiEvent midiEvent = new MidiEvent(br);
            
            // handle running status
            if (!midiEvent.ValidStatus)
            {
                // reuse the last status, if we have one
                if (_lastStatus == -1) throw new InvalidOperationException("midi event did not have status");
                midiEvent.Status = _lastStatus;
                
                // rewind the binary reader one byte, as the status we read was data
                br.BaseStream.Seek(-1, SeekOrigin.Current);
            }
            
            try
            {
                if (midiEvent.Status == 0xFF)
                {
                    return readMetaEvent(br, midiEvent);
                }
                else if (midiEvent.Status < 0xF0 && midiEvent.Status >= 0x80)
                {
                    return readChannelMessageEvent(br, midiEvent);
                }
                else
                {
                    throw new InvalidOperationException($"invalid midi event status 0x{midiEvent.Status:X}");
                }
            }
            finally
            {
                _lastStatus = midiEvent.Status;
            }
        }

        private MetaEventData readMetaEvent(BinaryReader br, MidiEvent midiEvent)
        {
            int peekedType = MetaEventData.PeekType(br);
            
            // from the peeked type we can create the meta event accordingly
            if (peekedType == 0x2F)
            {
                return new TrackEndMetaEventData(br, midiEvent);
            }
            else if (peekedType == 0x51)
            {
                return new SetTempoMetaEventData(br, midiEvent);
            }
            else
            {
                throw new InvalidOperationException($"invalid meta event type 0x{peekedType:X}, status 0x{midiEvent.Status:X}");
            }
        }

        private MidiChannelMessageEventData readChannelMessageEvent(BinaryReader br, MidiEvent midiEvent)
        {
            int statusMsNibble = midiEvent.Status >> 4;
            if (statusMsNibble == 0x8 || statusMsNibble == 0x9 || statusMsNibble == 0xA)
            {
                return new NoteEventData(br, midiEvent);
            }
            else if (statusMsNibble == 0xC)
            {
                return new ProgramChangeEventData(br, midiEvent);
            }
            else if (statusMsNibble == 0xE)
            {
                return new PitchWheelEventData(br, midiEvent);
            }
            else if (statusMsNibble == 0xB)
            {
                return new ControlChangeEventData(br, midiEvent);
            }
            else
            {
                throw new InvalidOperationException($"unknown status 0x{statusMsNibble:X}");
            }
        }
    }
}
