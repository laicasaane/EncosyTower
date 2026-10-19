#if UNITY_BURST && UNITY_COLLECTIONS

using EncosyTower.Logging;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Logging
{
    public class BurstCallerInfoTests
    {
        private const string MEMBER_NAME = "Run";
        private const string SHORT_PATH = "Assets/Scripts/Foo.cs";
        private const int LINE_NUMBER = 42;

        [Test]
        public void GetFull_ShortPath_KeepsWholePath()
        {
            var info = GetInfo(SHORT_PATH);

            Assert.AreEqual(SHORT_PATH, info.FilePath.ToString());
            Assert.AreEqual(MEMBER_NAME, info.MemberName.ToString());
            Assert.AreEqual(LINE_NUMBER, info.LineNumber);
        }

        [Test]
        public void GetFull_PathOf250Bytes_KeepsWholePath()
        {
            var path = CreatePath(250);
            var info = GetInfo(path);

            Assert.AreEqual(path, info.FilePath.ToString());
        }

        [Test]
        public void GetFull_LongPath_KeepsTheLast375Bytes()
        {
            var path = CreatePath(400);
            var info = GetInfo(path);

            Assert.AreEqual(path[25..], info.FilePath.ToString());
        }

        [Test]
        public void ToFixedString_FormatsMemberPathAndLine()
        {
            var info = GetInfo(SHORT_PATH);

            Assert.AreEqual("Run (at Assets/Scripts/Foo.cs:42)", info.ToFixedString().ToString());
        }

        private static BurstCallerInfo GetInfo(string filePath)
            => BurstCallerInfo.GetFull(lineNumber: LINE_NUMBER, memberName: MEMBER_NAME, filePath: filePath);

        private static string CreatePath(int length)
        {
            var chars = new char[length];

            for (var i = 0; i < length; i++)
            {
                chars[i] = (char)('a' + i % 26);
            }

            return new string(chars);
        }
    }
}

#endif
