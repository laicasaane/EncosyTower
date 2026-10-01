// com.unity.entities © 2024 Unity Technologies
//
// Licensed under the Unity Companion License for Unity-dependent projects
// (see https://unity3d.com/legal/licenses/unity_companion_license).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS”
// BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// Please review the license for details on these and other terms and conditions.

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen
{
    public static class SourceGenHelpers
    {
        public const string TRACKED_NODE_ANNOTATION_USED_BY_ROSLYN = "Id";

        /// <summary>
        /// Line to replace with on generated source.
        /// </summary>
        public const string GENERATED_LINE_TRIVIA_TO_GENERATED_SOURCE = "// __generatedline__";

        public const string NEWLINE = "\n";

        private const int SEMANTIC_HINT_HASH_BYTE_COUNT = 8;

        public static string BuildSemanticHintName(
              string generatorMetadataName
            , string consumerAssemblyName
            , string targetMetadataName
            , string outputRole
            , string discriminator
        )
        {
            var segments = new[] {
                generatorMetadataName ?? string.Empty,
                consumerAssemblyName ?? string.Empty,
                targetMetadataName ?? string.Empty,
                outputRole ?? string.Empty,
                discriminator ?? string.Empty,
            };
            var encodedSegments = new byte[segments.Length][];
            var keyLength = 2;

            for (var i = 0; i < segments.Length; i++)
            {
                var encodedSegment = Encoding.UTF8.GetBytes(segments[i]);
                encodedSegments[i] = encodedSegment;
                keyLength += sizeof(int) + encodedSegment.Length;
            }

            var key = new byte[keyLength];
            key[0] = (byte)'v';
            key[1] = (byte)'1';
            var offset = 2;

            foreach (var encodedSegment in encodedSegments)
            {
                WriteLengthPrefix(key, offset, encodedSegment.Length);
                offset += sizeof(int);
                Buffer.BlockCopy(encodedSegment, 0, key, offset, encodedSegment.Length);
                offset += encodedSegment.Length;
            }

            byte[] hash;

            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(key);
            }

            var simpleTargetName = GetSimpleMetadataName(targetMetadataName);
            var readableTarget = SanitizeHintSegment(simpleTargetName, 64);
            var readableRole = SanitizeHintSegment(outputRole, 32);
            var hashText = ToLowerHex(hash);

            return $"{readableTarget}.{readableRole}.{hashText}.g.cs";
        }

        public static SourceText WithIgnoreUnassignedVariableWarning(this SourceText sourceText)
        {
            var firstLine = sourceText.Lines.FirstOrDefault();
            return sourceText.WithChanges(new TextChange(
                  firstLine.Span
                , $"#pragma warning disable 0219{NEWLINE}{firstLine}"
            ));
        }

        private static string GetSimpleMetadataName(string metadataName)
        {
            if (string.IsNullOrEmpty(metadataName))
            {
                return string.Empty;
            }

            var namespaceSeparator = metadataName.LastIndexOf('.');
            var nestingSeparator = metadataName.LastIndexOf('+');
            var separator = Math.Max(namespaceSeparator, nestingSeparator);

            return separator < 0 ? metadataName : metadataName.Substring(separator + 1);
        }

        private static string SanitizeHintSegment(string value, int maximumLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "_";
            }

            var length = Math.Min(value.Length, maximumLength);
            var chars = new char[length];

            for (var i = 0; i < length; i++)
            {
                var c = value[i];
                chars[i] = c is >= 'a' and <= 'z'
                    or >= 'A' and <= 'Z'
                    or >= '0' and <= '9'
                    or '.'
                    or '_'
                    or '-'
                        ? c
                        : '_';
            }

            return new string(chars);
        }

        private static string ToLowerHex(byte[] bytes)
        {
            const string HEX = "0123456789abcdef";
            var chars = new char[SEMANTIC_HINT_HASH_BYTE_COUNT * 2];

            for (var i = 0; i < SEMANTIC_HINT_HASH_BYTE_COUNT; i++)
            {
                var value = bytes[i];
                chars[i * 2] = HEX[value >> 4];
                chars[(i * 2) + 1] = HEX[value & 0x0f];
            }

            return new string(chars);
        }

        private static void WriteLengthPrefix(byte[] destination, int offset, int value)
        {
            destination[offset] = (byte)(value >> 24);
            destination[offset + 1] = (byte)(value >> 16);
            destination[offset + 2] = (byte)(value >> 8);
            destination[offset + 3] = (byte)value;
        }
    }
}
