using System.Linq;
using EncosyTower.Naming;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Naming
{
    public class NamingPolicyTests
    {
        [Test]
        public void ConvertName_EmptyString_ReturnsEmptyForEverySeparatorPolicy()
        {
            Assert.AreEqual(string.Empty, NamingPolicy.PascalCase.ConvertName(string.Empty));
            Assert.AreEqual(string.Empty, NamingPolicy.SnakeCaseLower.ConvertName(string.Empty));
            Assert.AreEqual(string.Empty, NamingPolicy.SnakeCaseUpper.ConvertName(string.Empty));
            Assert.AreEqual(string.Empty, NamingPolicy.KebabCaseLower.ConvertName(string.Empty));
            Assert.AreEqual(string.Empty, NamingPolicy.KebabCaseUpper.ConvertName(string.Empty));
        }

        [Test]
        public void ConvertName_OnlySpaces_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, NamingPolicy.SnakeCaseLower.ConvertName("   "));
            Assert.AreEqual(string.Empty, NamingPolicy.KebabCaseUpper.ConvertName(" "));
            Assert.AreEqual(string.Empty, NamingPolicy.PascalCase.ConvertName("  "));
        }

        [TestCase("A", "a")]
        [TestCase("a", "a")]
        [TestCase("1", "1")]
        [TestCase("HelloWorld", "hello_world")]
        [TestCase("helloWorld", "hello_world")]
        [TestCase("XMLReader", "xml_reader")]
        [TestCase("SHA512Hash", "sha512_hash")]
        [TestCase("a1B", "a1_b")]
        [TestCase("ABC???def", "abc???def")]
        [TestCase("already_snake", "already_snake")]
        [TestCase("Hello World", "hello_world")]
        [TestCase("a  b", "a_b")]
        [TestCase("  leading and trailing  ", "leading_and_trailing")]
        public void ConvertName_SnakeCaseLower_ReturnsExpectedName(string name, string expected)
        {
            Assert.AreEqual(expected, NamingPolicy.SnakeCaseLower.ConvertName(name));
        }

        [TestCase("HelloWorld", "HELLO_WORLD")]
        [TestCase("XMLReader", "XML_READER")]
        [TestCase("SHA512Hash", "SHA512_HASH")]
        [TestCase("hello world", "HELLO_WORLD")]
        public void ConvertName_SnakeCaseUpper_ReturnsExpectedName(string name, string expected)
        {
            Assert.AreEqual(expected, NamingPolicy.SnakeCaseUpper.ConvertName(name));
        }

        [TestCase("HelloWorld", "hello-world")]
        [TestCase("XMLReader", "xml-reader")]
        [TestCase("SHA512Hash", "sha512-hash")]
        [TestCase("hello world", "hello-world")]
        public void ConvertName_KebabCaseLower_ReturnsExpectedName(string name, string expected)
        {
            Assert.AreEqual(expected, NamingPolicy.KebabCaseLower.ConvertName(name));
        }

        [TestCase("HelloWorld", "HELLO-WORLD")]
        [TestCase("XMLReader", "XML-READER")]
        [TestCase("SHA512Hash", "SHA512-HASH")]
        public void ConvertName_KebabCaseUpper_ReturnsExpectedName(string name, string expected)
        {
            Assert.AreEqual(expected, NamingPolicy.KebabCaseUpper.ConvertName(name));
        }

        [TestCase("hello world", "HelloWorld")]
        [TestCase("helloWorld", "HelloWorld")]
        [TestCase("XMLReader", "XmlReader")]
        [TestCase("snake_case_name", "Snake_Case_Name")]
        public void ConvertName_PascalCase_ReturnsExpectedName(string name, string expected)
        {
            Assert.AreEqual(expected, NamingPolicy.PascalCase.ConvertName(name));
        }

        [TestCase(1)]
        [TestCase(105)]
        [TestCase(106)]
        [TestCase(107)]
        [TestCase(108)]
        [TestCase(127)]
        [TestCase(128)]
        [TestCase(129)]
        [TestCase(1000)]
        public void ConvertName_LowerCaseInputAroundStackThreshold_ReturnsSameText(int length)
        {
            var name = new string('a', length);

            Assert.AreEqual(name, NamingPolicy.SnakeCaseLower.ConvertName(name));
        }

        [TestCase(10)]
        [TestCase(11)]
        public void ConvertName_RepeatedWordsAroundStackThreshold_JoinsWithSeparators(int repeat)
        {
            var name = string.Concat(Enumerable.Repeat("HelloWorld", repeat));
            var expected = string.Join("_", Enumerable.Repeat("hello_world", repeat));

            Assert.AreEqual(expected, NamingPolicy.SnakeCaseLower.ConvertName(name));
        }

        [TestCase(50)]
        [TestCase(300)]
        public void ConvertName_OutputLongerThanInitialBuffer_ExpandsAndKeepsEveryChar(int repeat)
        {
            var name = string.Concat(Enumerable.Repeat("aB", repeat));
            var expected = string.Concat(Enumerable.Repeat("a_b", repeat));

            Assert.AreEqual(expected, NamingPolicy.SnakeCaseLower.ConvertName(name));
        }

        [Test]
        public void ConvertName_OutputLongerThanInitialBuffer_ExpandsForEveryCasing()
        {
            var name = string.Concat(Enumerable.Repeat("aB", 300));
            var expectedUpper = string.Concat(Enumerable.Repeat("A-B", 300));
            var expectedLower = string.Concat(Enumerable.Repeat("a-b", 300));

            Assert.AreEqual(expectedUpper, NamingPolicy.KebabCaseUpper.ConvertName(name));
            Assert.AreEqual(expectedLower, NamingPolicy.KebabCaseLower.ConvertName(name));
        }

        [Test]
        public void ConvertName_NonAsciiLetters_ConvertsCaseAndSeparators()
        {
            const string NAME = "\u00c9cole\u00c0bc";

            Assert.AreEqual("\u00e9cole_\u00e0bc", NamingPolicy.SnakeCaseLower.ConvertName(NAME));
            Assert.AreEqual("\u00c9COLE-\u00c0BC", NamingPolicy.KebabCaseUpper.ConvertName(NAME));
        }

        [Test]
        public void ConvertName_SurrogatePair_IsKeptIntact()
        {
            const string EMOJI = "\U0001F600";

            var result = NamingPolicy.SnakeCaseLower.ConvertName("A" + EMOJI + "B");

            Assert.AreEqual("a" + EMOJI + "b", result);
        }

        [Test]
        public void ConvertName_SurrogatePairInLongInput_IsKeptIntactAcrossBufferGrowth()
        {
            const string EMOJI = "\U0001F600";

            var name = string.Concat(Enumerable.Repeat("aB" + EMOJI, 200));
            var expected = string.Concat(Enumerable.Repeat("a_b" + EMOJI, 200));

            Assert.AreEqual(expected, NamingPolicy.SnakeCaseLower.ConvertName(name));
        }
    }
}
