using System.IO;
using libLSD.Formats;
using NUnit.Framework;

namespace libLSD.Tests.Formats
{
    [TestFixture]
    public class TIMTests
    {
        [TestCase]
        public void TestTIMWithSingleCLUT()
        {
            using (BinaryReader br = new BinaryReader(TestUtil.OpenTestData("FONTICON.TIM")))
            {
                TIM tim = new TIM(br);
                Assert.True(tim.Header.HasCLUT);
                Assert.IsNotNull(tim.ColorLookup);
                Assert.AreEqual(TIMHeader.PixelModes.CLUT4Bit, tim.Header.PixelMode);
                Assert.AreEqual(1, tim.ColorLookup.Value.NumberOfCLUTs);
            }
        }

        [TestCase]
        public void TestTIMWithMultipleCLUTs()
        {
            using (BinaryReader br = new BinaryReader(TestUtil.OpenTestData("ETC.TIM")))
            {
                TIM tim = new TIM(br);
                Assert.True(tim.Header.HasCLUT);
                Assert.IsNotNull(tim.ColorLookup);
                Assert.AreEqual(TIMHeader.PixelModes.CLUT4Bit, tim.Header.PixelMode);
                Assert.AreEqual(4, tim.ColorLookup.Value.NumberOfCLUTs);
            }
        }
    }
}
