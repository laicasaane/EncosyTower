using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

internal static class ThrowHelper
{
    private const MethodImplOptions NO_INLINING = MethodImplOptions.NoInlining;

    internal static void ThrowIfRunnerPhaseMismatch(
          [DoesNotReturnIf(true)] bool condition
        , string phase
        , string buildPhase
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(phase, buildPhase);

        [MethodImpl(NO_INLINING)]
        static InvalidOperationException CreateException(string value, string expected)
            => new($"Runner phase '{value}' does not match build phase '{expected}'.");
    }

    [MethodImpl(NO_INLINING), DoesNotReturn]
    internal static int ThrowUnknownCodeGenPhase(string phase)
        => throw new InvalidOperationException($"Unknown CodeGen phase '{phase}'.");

    internal static void ThrowIfGeneratorNameCollision(
          [DoesNotReturnIf(true)] bool condition
        , string identity
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(identity);

        [MethodImpl(NO_INLINING)]
        static InvalidOperationException CreateException(string value)
            => new($"Generated generator-name collision: {value}");
    }

    internal static void ThrowIfDuplicateIslandId(
          [DoesNotReturnIf(true)] bool condition
        , string islandId
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(islandId);

        [MethodImpl(NO_INLINING)]
        static InvalidDataException CreateException(string value)
            => new($"Duplicate island ID: {value}");
    }

    internal static void ThrowIfIslandOutputMissing(
          [DoesNotReturnIf(true)] bool condition
        , string targetPath
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(targetPath);

        [MethodImpl(NO_INLINING)]
        static FileNotFoundException CreateException(string value)
            => new("Island output is missing.", value);
    }

    internal static void ThrowIfNamespaceRewriteInvalid(
          [DoesNotReturnIf(true)] bool condition
        , string filePath
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(filePath);

        [MethodImpl(NO_INLINING)]
        static InvalidOperationException CreateException(string value)
            => new($"Namespace rewrite created invalid syntax in {value}.");
    }

    [MethodImpl(NO_INLINING), DoesNotReturn]
    internal static string ThrowUnknownReferenceKind(string kind)
        => throw new InvalidDataException($"Unknown reference kind: {kind}");

    internal static void ThrowIfDuplicateTemplateToken(
          [DoesNotReturnIf(true)] bool condition
        , string templatePath
        , string token
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(templatePath, token);

        [MethodImpl(NO_INLINING)]
        static InvalidDataException CreateException(string path, string value)
            => new($"Cannot compose template '{path}': duplicate token '{value}'.");
    }

    internal static void ThrowIfMissingTemplateToken(
          [DoesNotReturnIf(true)] bool condition
        , string templatePath
        , string token
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(templatePath, token);

        [MethodImpl(NO_INLINING)]
        static InvalidDataException CreateException(string path, string value)
            => new($"Cannot compose template '{path}': missing token '{value}'.");
    }

    internal static void ThrowIfUnresolvedTemplateToken(
          [DoesNotReturnIf(true)] bool condition
        , string templatePath
        , string token
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(templatePath, token);

        [MethodImpl(NO_INLINING)]
        static InvalidDataException CreateException(string path, string value)
            => new($"Cannot compose template '{path}': unresolved token '{value}'.");
    }

    internal static void ThrowIfTemplateFileMissing(
          [DoesNotReturnIf(true)] bool condition
        , string path
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(path);

        [MethodImpl(NO_INLINING)]
        static FileNotFoundException CreateException(string value)
            => new("Template file does not exist.", value);
    }

    [MethodImpl(NO_INLINING), DoesNotReturn]
    internal static string ThrowCannotReadTemplate(string path, Exception exception)
        => throw new InvalidDataException($"Cannot read template '{path}': {exception.Message}", exception);

    internal static void ThrowIfGeneratedContentNull([DoesNotReturnIf(true)] bool condition)
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException();

        [MethodImpl(NO_INLINING)]
        static InvalidDataException CreateException()
            => new("Generated content is null.");
    }

    internal static void ThrowIfUnsupportedSchema(
          [DoesNotReturnIf(true)] bool condition
        , string name
        , int version
        , int expectedVersion
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(name, version, expectedVersion);

        [MethodImpl(NO_INLINING)]
        static InvalidDataException CreateException(string value, int actualVersion, int expectedVersion)
            => new($"Unsupported {value} schema {actualVersion}; expected {expectedVersion}.");
    }

    internal static void ThrowIfMalformedRunnerArguments([DoesNotReturnIf(true)] bool condition)
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException();

        [MethodImpl(NO_INLINING)]
        static ArgumentException CreateException()
            => new("Runner arguments must be key/value pairs.");
    }

    internal static void ThrowIfDuplicateRunnerArgument(
          [DoesNotReturnIf(true)] bool condition
        , string argument
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(argument);

        [MethodImpl(NO_INLINING)]
        static ArgumentException CreateException(string value)
            => new($"Duplicate Runner argument: {value}");
    }

    internal static void ThrowIfRunnerArgumentMissing(
          [DoesNotReturnIf(true)] bool condition
        , string key
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(key);

        [MethodImpl(NO_INLINING)]
        static ArgumentException CreateException(string value)
            => new($"Missing Runner argument: {value}");
    }

    internal static void ThrowIfJsonPropertyNull(
          [DoesNotReturnIf(true)] bool condition
        , string name
    )
    {
        if (condition == false)
        {
            return;
        }

        throw CreateException(name);

        [MethodImpl(NO_INLINING)]
        static InvalidDataException CreateException(string value)
            => new($"JSON property '{value}' is null.");
    }
}
