using System;
using System.IO;
using libLSD.Audio.Sequence;
using libLSD.Audio.Soundbank;
using libLSD.Formats;
using NUnit.Framework;

namespace libLSD.Tests.Formats;

public class VABTests
{
    [TestCase]
    public void TestSEQWithFont()
    {
        string font = "SE";
        using BinaryReader vhBr = new BinaryReader(TestUtil.OpenTestData($"{font}.VH"));
        using BinaryReader vbBr = new BinaryReader(TestUtil.OpenTestData($"{font}.VB"));
        VAB vab = new VAB(vhBr, vbBr);
        
        Console.WriteLine("breakpoint");
    }
}
