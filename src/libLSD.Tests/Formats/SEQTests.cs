using System;
using System.Collections.Generic;
using System.IO;
using libLSD.Audio.Sequence;
using libLSD.Audio.Soundbank;
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
    
    [TestCase]
    public void TestSEQWithFont()
    {
        string data = "BGB.SEQ";
        using BinaryReader seqBr = new BinaryReader(TestUtil.OpenTestData(data));
        SEQ seq = new SEQ(seqBr);
        SEQSequence sequence = new SEQSequence(seq);

        string font = "AMBIENT";
        using BinaryReader vhBr = new BinaryReader(TestUtil.OpenTestData($"{font}.VH"));
        using BinaryReader vbBr = new BinaryReader(TestUtil.OpenTestData($"{font}.VB"));
        VAB vab = new VAB(vhBr, vbBr);
        VABSoundbank soundbank = new VABSoundbank(vab);
        
        Console.WriteLine("breakpoint");
    }
}
