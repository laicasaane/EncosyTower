using System.Runtime.CompilerServices;

namespace EncosyTower.SourceGen.Tests;

internal readonly record struct ExpectedGeneratedSource(
    Type GeneratorType,
    string HintName,
    string VerifiedPath
)
{
    internal static ExpectedGeneratedSource Create<TGenerator>(
          string hintName
        , string caseName = ""
        , [CallerMemberName] string testMethod = ""
        , [CallerFilePath] string testFile = ""
    )
    {
        var generatorType = typeof(TGenerator);
        var testDirectory = Path.GetDirectoryName(testFile)
            ?? throw new InvalidOperationException($"Cannot resolve the directory for '{testFile}'.");
        var snapshotName = string.IsNullOrWhiteSpace(caseName) ? testMethod : $"{testMethod}.{caseName}";
        var fileName = $"{snapshotName}#{generatorType.Name}#{Sanitize(hintName)}.verified.cs";
        var verifiedPath = Path.Combine(testDirectory, "Snapshots", fileName);

        return new ExpectedGeneratedSource(generatorType, hintName, verifiedPath);
    }

    private static string Sanitize(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();
        var characters = value.Select(character => character is '/' or '\\' || invalidCharacters.Contains(character)
            ? '_'
            : character
        );

        return new string(characters.ToArray());
    }
}
