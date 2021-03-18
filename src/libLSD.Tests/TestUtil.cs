using System.IO;
using NUnit.Framework;

namespace libLSD.Tests
{
    public static class TestUtil
    {
        public const string TEST_DATA_FOLDER = "TestData";

        public static FileStream OpenTestData(string pathToFile)
        {
            return File.Open(Path.Combine(TestContext.CurrentContext.TestDirectory, TEST_DATA_FOLDER, pathToFile),
                FileMode.Open);
        }
    }
}
