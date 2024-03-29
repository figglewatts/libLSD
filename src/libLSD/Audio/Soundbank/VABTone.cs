﻿using System;
using libLSD.Formats;

namespace libLSD.Audio.Soundbank
{
    [Serializable]
    public class VABTone
    {
        public ToneAttribute Attributes;
        public VAG Sample;

        public bool NoteInTone(int note)
        {
            return note >= Attributes.MinNote && note <= Attributes.MaxNote;
        }
    }
}
