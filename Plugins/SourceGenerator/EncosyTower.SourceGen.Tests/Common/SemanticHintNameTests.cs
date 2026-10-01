using System.Globalization;

namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
[DoNotParallelize]
public sealed class SemanticHintNameTests
{
    private const string GENERATOR = "Tests.ContractGenerator";

    [TestMethod]
    public void SameSemanticFacts_ProduceExactHint()
    {
        var hintName = SourceGenHelpers.BuildSemanticHintName(
              GENERATOR
            , "Consumer"
            , "Tests.Outer`1+Target"
            , "Contract"
            , string.Empty
        );

        Assert.AreEqual(
              "Target.Contract.bf2dc1c079465514.g.cs"
            , hintName
        );
    }

    [TestMethod]
    public void EverySemanticFact_ChangesHash()
    {
        var hints = new HashSet<string>(StringComparer.Ordinal) {
            BuildHint(GENERATOR, "Consumer", "Test.Target", "Role", string.Empty),
            BuildHint(GENERATOR + "2", "Consumer", "Test.Target", "Role", string.Empty),
            BuildHint(GENERATOR, "Consumer2", "Test.Target", "Role", string.Empty),
            BuildHint(GENERATOR, "Consumer", "Test.Target2", "Role", string.Empty),
            BuildHint(GENERATOR, "Consumer", "Test.Target", "Role2", string.Empty),
            BuildHint(GENERATOR, "Consumer", "Test.Target", "Role", "Second"),
        };

        Assert.AreEqual(6, hints.Count);
    }

    [TestMethod]
    public void SourceCoordinates_DoNotAffectHint()
    {
        var beforeMove = BuildHint(GENERATOR, "Consumer", "Test.Namespace.Outer`1+Target`2", "Role", string.Empty);
        var afterMoveAndPrecedingLineEdit = BuildHint(
              GENERATOR
            , "Consumer"
            , "Test.Namespace.Outer`1+Target`2"
            , "Role"
            , string.Empty
        );

        Assert.AreEqual(beforeMove, afterMoveAndPrecedingLineEdit);
        StringAssert.StartsWith(afterMoveAndPrecedingLineEdit, "Target_2.Role.");
    }

    [TestMethod]
    public void SanitizationCollision_StillHasDistinctHash()
    {
        var slash = BuildHint(GENERATOR, "Consumer", "Test.Outer+B/C", "Role", string.Empty);
        var colon = BuildHint(GENERATOR, "Consumer", "Test.Outer+B:C", "Role", string.Empty);

        StringAssert.StartsWith(slash, "B_C.Role.");
        StringAssert.StartsWith(colon, "B_C.Role.");
        Assert.AreNotEqual(slash, colon);
    }

    [TestMethod]
    public void Hint_IsCultureInvariant()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var invariant = BuildHint(GENERATOR, "Consumer", "Test.Target", "Role", "i");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");

            Assert.AreEqual(invariant, BuildHint(GENERATOR, "Consumer", "Test.Target", "Role", "i"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [TestMethod]
    public void HintSegments_AreBounded()
    {
        var hintName = BuildHint(
              GENERATOR
            , "Consumer"
            , "Test." + new string('T', 80)
            , new string('R', 40)
            , string.Empty
        );
        var segments = hintName.Split('.');

        Assert.AreEqual(64, segments[0].Length);
        Assert.AreEqual(32, segments[1].Length);
        Assert.AreEqual(16, segments[2].Length);
        Assert.AreEqual("g", segments[3]);
        Assert.AreEqual("cs", segments[4]);
        Assert.AreEqual(119, hintName.Length);
    }

    private static string BuildHint(string generator, string assembly, string target, string role, string discriminator)
        => SourceGenHelpers.BuildSemanticHintName(generator, assembly, target, role, discriminator);
}
