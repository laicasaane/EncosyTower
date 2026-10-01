using System.Text;

namespace EncosyTower.SourceGen.Tests;

internal static class GeneratedSourceSnapshot
{
    internal static async Task<string?> VerifyAsync(
          string generatedSource
        , string verifiedPath
        , CancellationToken token = default
    )
    {
        var normalizedSource = NormalizeNewLines(generatedSource);
        var receivedPath = verifiedPath.EndsWith(".verified.cs", StringComparison.Ordinal)
            ? verifiedPath[..^".verified.cs".Length] + ".received.cs"
            : throw new ArgumentException("Snapshot path must end with '.verified.cs'.", nameof(verifiedPath));

        if (File.Exists(verifiedPath))
        {
            var verifiedSource = NormalizeNewLines(await File.ReadAllTextAsync(verifiedPath, token));

            if (string.Equals(verifiedSource, normalizedSource, StringComparison.Ordinal))
            {
                if (File.Exists(receivedPath))
                {
                    File.Delete(receivedPath);
                }

                return null;
            }
        }

        var directory = Path.GetDirectoryName(receivedPath)
            ?? throw new InvalidOperationException($"Cannot resolve the directory for '{receivedPath}'.");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(receivedPath, normalizedSource, new UTF8Encoding(false), token);
        return $"Generated source differs from its snapshot.\nVerified: {verifiedPath}\nReceived: {receivedPath}";
    }

    private static string NormalizeNewLines(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
