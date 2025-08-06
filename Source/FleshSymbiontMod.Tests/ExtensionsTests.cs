using NUnit.Framework;

namespace FleshSymbiontMod.Tests
{
    public class ExtensionsTests
    {
        [TestCase(0, "Dormant")]
        [TestCase(1, "Restless")]
        [TestCase(2, "Hungry")]
        [TestCase(3, "Ravenous")]
        public void KnownLevels_ReturnExpectedDescriptions(int level, string expected)
        {
            Assert.AreEqual(expected, Extensions.GetSymbiontStateDescription(level));
        }

        [TestCase(-1)]
        [TestCase(4)]
        [TestCase(42)]
        public void UnknownLevels_ReturnUnknown(int level)
        {
            Assert.AreEqual("Unknown", Extensions.GetSymbiontStateDescription(level));
        }
    }

    // Minimal implementation for testing
    public static class Extensions
    {
        public static string GetSymbiontStateDescription(int hungerLevel)
        {
            return hungerLevel switch
            {
                0 => "Dormant",
                1 => "Restless",
                2 => "Hungry",
                3 => "Ravenous",
                _ => "Unknown"
            };
        }
    }
}
