using System;
using System.Collections.Generic;
using System.IO;
using libLSD.Audio.Sequence;
using libLSD.Formats;
using NUnit.Framework;

namespace libLSD.Tests.Formats;

[TestFixture]
public class SEQTests
{
    [TestCase]
    public void TestSEQLength()
    {
        var data = new List<string>
        {
            "BGA.SEQ",
            "BGB.SEQ",
            "BGC.SEQ",
            "BGD.SEQ",
            "BGE.SEQ",
        };
        foreach (var seqFile in data)
        {
            using (BinaryReader br = new BinaryReader(TestUtil.OpenTestData(seqFile)))
            {
                SEQ seq = new SEQ(br);
                SEQSequence sequence = new SEQSequence(seq);
                Console.WriteLine(sequence.GetLengthSeconds(0));
            }
        }
    }
}
