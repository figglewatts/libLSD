using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using libLSD.Formats;
using NUnit.Framework;

namespace libLSD.Tests.Analysis;

public class FootstepTests
{
    class AnalysisResult
    {
        public int RawValue;
        public string BinaryValue;
        public int Frequency;
        public HashSet<string> Sources;

        public AnalysisResult(int rawValue, string source)
        {
            RawValue = rawValue;
            BinaryValue = Convert.ToString(rawValue, 2);
            Frequency = 1;
            Sources = new HashSet<string> { source };
        }
    }
    
    [TestCase]
    public void GetUniqueFootstepTypes()
    {
        Dictionary<int, AnalysisResult> uniqueFootstepValues = new Dictionary<int, AnalysisResult>();

        void addToDict(int footstepVal, string source)
        {
            if (uniqueFootstepValues.TryGetValue(footstepVal, out AnalysisResult value))
            {
                value.Frequency++;
                value.Sources.Add(source);
            }
            else
            {
                uniqueFootstepValues[footstepVal] = new AnalysisResult(footstepVal, source);
            }
        }

        var dataDir = @"Z:\archive\LSD Revamped\Experiment\extractedDisk\CDI";
        foreach (var lbdFile in Directory.GetFiles(dataDir, "*.LBD", SearchOption.AllDirectories))
        {
            Console.WriteLine($"Processing {lbdFile}...");
            using var fs = new FileStream(lbdFile, FileMode.Open);
            using var br = new BinaryReader(fs);
            LBD lbd = new LBD(br);

            foreach (var tile in lbd.TileLayout)
            {
                addToDict(tile.Footstep, lbdFile);
            }
        }
        
        Console.WriteLine("breakpoint");
    }
}
