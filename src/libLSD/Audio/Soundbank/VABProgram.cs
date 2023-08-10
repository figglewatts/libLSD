using System.Collections.Generic;
using System.Linq;

namespace libLSD.Audio.Soundbank
{
    public class VABProgram
    {
        public ProgramAttribute Attributes;
        public List<VABTone> Tones;

        public VABTone GetToneForNote(int note)
        {
            return Tones.FirstOrDefault(t => t.NoteInTone(note));
        }
    }
}
