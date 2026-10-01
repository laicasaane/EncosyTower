#if UNITY_EDITOR

using EncosyTower.Editor.CodeGen;
using NUnit.Framework;

namespace EncosyTower.Tests.Editor.CodeGen
{
    [Category("Editor.CodeGen")]
    public sealed class DotnetGeneratorSyntaxTests
    {
        [Test]
        public void ProtocolSchema_DefaultsAreNonNull()
        {
            var input = new DotnetWorkspaceProtocol.InputManifest();
            var islands = new DotnetWorkspaceProtocol.IslandsManifest();
            var skipped = new DotnetWorkspaceProtocol.SkippedGeneratorsManifest();
            var result = new DotnetWorkspaceProtocol.AggregateResultManifest();

            Assert.That(input.assemblies, Is.Not.Null);
            Assert.That(islands.islands, Is.Not.Null);
            Assert.That(skipped.skippedGenerators, Is.Not.Null);
            Assert.That(result.generatedCodes, Is.Not.Null);
            Assert.That(result.diagnostics, Is.Not.Null);
            Assert.That(new DotnetWorkspaceProtocol.CompilerOptionsInput().responseFiles, Is.Not.Null);
        }
    }
}

#endif
