using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public static class UnmanagedSizeUpperBoundCalculator
    {
        public static UnmanagedSizeUpperBoundResult Calculate(
              ITypeSymbol type
            , CancellationToken token
        )
        {
            if (type is null || type.TypeKind == TypeKind.Error)
            {
                return Failure(UnmanagedSizeUpperBoundFailure.InvalidLayout);
            }

            var cache = new Dictionary<ITypeSymbol, Calculation>(SymbolEqualityComparer.Default);
            var active = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            try
            {
                return Calculate(type, cache, active, token).result;
            }
            catch (OverflowException)
            {
                return Failure(UnmanagedSizeUpperBoundFailure.SizeOverflow);
            }
        }

        private static Calculation Calculate(
              ITypeSymbol type
            , Dictionary<ITypeSymbol, Calculation> cache
            , HashSet<ITypeSymbol> active
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (type is null || type.TypeKind == TypeKind.Error)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.InvalidLayout);
            }

            if (cache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            if (TryGetPrimitiveLayout(type, out var primitiveSize, out var primitiveAlignment))
            {
                return Known(primitiveSize, primitiveAlignment);
            }

            if (type.SpecialType is SpecialType.System_IntPtr or SpecialType.System_UIntPtr)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.PointerSizedStorage);
            }

            if (type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.PointerStorage);
            }

            if (type is INamedTypeSymbol { IsGenericType: true } generic)
            {
                GetLayout(generic.OriginalDefinition, token, out var genericLayout, out _, out _, out _);

                if (genericLayout == 2)
                {
                    return FailureCalculation(UnmanagedSizeUpperBoundFailure.InvalidGenericExplicitLayout);
                }
            }

            if (type.TypeKind == TypeKind.TypeParameter || IsOpen(type, token))
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.OpenGeneric);
            }

            if (type.IsRefLikeType)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.RefLikeType);
            }

            if (type is not INamedTypeSymbol named)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.InvalidLayout);
            }

            if (named.TypeKind == TypeKind.Enum)
            {
                return Calculate(named.EnumUnderlyingType, cache, active, token);
            }

            if (named.TypeKind != TypeKind.Struct)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.ManagedStorage);
            }

            if (HasCompleteStorage(named, token) == false)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.OpaqueExternalStorage);
            }

            if (active.Add(named) == false)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.RecursiveLayout);
            }

            var result = CalculateNamed(named, cache, active, token);
            active.Remove(named);
            cache[named] = result;
            return result;
        }

        private static Calculation CalculateNamed(
              INamedTypeSymbol type
            , Dictionary<ITypeSymbol, Calculation> cache
            , HashSet<ITypeSymbol> active
            , CancellationToken token
        )
        {
            GetLayout(type, token, out var layoutKind, out var pack, out var declaredSize, out var hasLayoutAttribute);

            if ((hasLayoutAttribute && layoutKind is not (0 or 2 or 3)) || declaredSize < 0)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.InvalidLayout);
            }

            if (layoutKind == 3)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.AutoLayout);
            }

            if (IsSupportedPack(pack) == false)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.UnsupportedPacking);
            }

            if (layoutKind == 2 && type.OriginalDefinition.Arity > 0)
            {
                return FailureCalculation(UnmanagedSizeUpperBoundFailure.InvalidGenericExplicitLayout);
            }

            var members = type.GetMembers();
            var memberCount = members.Length;
            var fields = new List<IFieldSymbol>(memberCount);

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                if (members[i] is IFieldSymbol { IsStatic: false } field)
                {
                    fields.Add(field);
                }
            }

            if (fields.Count == 0)
            {
                return Known(Math.Max(1, declaredSize), alignment: 1);
            }

            var fieldCount = fields.Count;
            var fieldResults = new FieldCalculation[fieldCount];
            var typeAlignment = 1;

            for (var i = 0; i < fieldCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var field = fields[i];

                if (field.IsImplicitlyDeclared)
                {
                    return FailureCalculation(UnmanagedSizeUpperBoundFailure.GeneratedStorageUnknown);
                }

                Calculation fieldResult;

                if (field.IsFixedSizeBuffer)
                {
                    if (field.Type is not IPointerTypeSymbol pointer || field.FixedSize <= 0)
                    {
                        return FailureCalculation(UnmanagedSizeUpperBoundFailure.InvalidLayout);
                    }

                    var element = Calculate(pointer.PointedAtType, cache, active, token);
                    fieldResult = element.result.isKnown
                        ? Known(checked(element.result.sizeUpperBound * field.FixedSize), element.alignment)
                        : element;
                }
                else
                {
                    fieldResult = Calculate(field.Type, cache, active, token);
                }

                if (fieldResult.result.isKnown == false)
                {
                    return fieldResult;
                }

                fieldResults[i] = new FieldCalculation(field, fieldResult);
                typeAlignment = Math.Max(typeAlignment, fieldResult.alignment);
            }

            var fieldResultCount = fieldResults.Length;

            if (layoutKind == 2)
            {
                var explicitSize = 0;

                for (var i = 0; i < fieldResultCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var entry = fieldResults[i];

                    if (TryGetFieldOffset(entry.field, token, out var offset) == false || offset < 0)
                    {
                        return FailureCalculation(UnmanagedSizeUpperBoundFailure.InvalidLayout);
                    }

                    explicitSize = Math.Max(explicitSize, checked(offset + entry.calculation.result.sizeUpperBound));
                }

                return Known(AlignUp(Math.Max(Math.Max(1, declaredSize), explicitSize), typeAlignment), typeAlignment);
            }

            var size = 0;
            var alignmentPadding = 0;
            var packedAlignment = pack > 0 ? Math.Min(typeAlignment, pack) : typeAlignment;

            for (var i = 0; i < fieldResultCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var entry = fieldResults[i];
                var fieldAlignment = pack > 0
                    ? Math.Min(entry.calculation.alignment, pack)
                    : entry.calculation.alignment;
                size = checked(size + entry.calculation.result.sizeUpperBound);
                alignmentPadding = checked(alignmentPadding + fieldAlignment - 1);
            }

            size = checked(size + alignmentPadding + packedAlignment - 1);
            var declaredMinimum = Math.Max(1, declaredSize);
            var declaredUpperBound = checked(declaredMinimum + packedAlignment - 1);
            size = Math.Max(size, declaredUpperBound);

            if (pack > 0 && type.IsGenericType)
            {
                var unpackedSize = 0;
                var unpackedAlignment = 1;

                for (var i = 0; i < fieldResultCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var fieldCalculation = fieldResults[i].calculation;
                    unpackedSize = AlignUp(unpackedSize, fieldCalculation.alignment);
                    unpackedSize = checked(unpackedSize + fieldCalculation.result.sizeUpperBound);
                    unpackedAlignment = Math.Max(unpackedAlignment, fieldCalculation.alignment);
                }

                unpackedSize = AlignUp(Math.Max(declaredMinimum, unpackedSize), unpackedAlignment);
                size = Math.Max(size, unpackedSize);
                packedAlignment = Math.Max(packedAlignment, unpackedAlignment);
            }

            return Known(size, packedAlignment);
        }

        private static bool TryGetPrimitiveLayout(ITypeSymbol type, out int size, out int alignment)
        {
            size = type.SpecialType switch {
                SpecialType.System_Boolean => 1,
                SpecialType.System_Byte => 1,
                SpecialType.System_SByte => 1,
                SpecialType.System_Char => 2,
                SpecialType.System_Int16 => 2,
                SpecialType.System_UInt16 => 2,
                SpecialType.System_Int32 => 4,
                SpecialType.System_UInt32 => 4,
                SpecialType.System_Single => 4,
                SpecialType.System_Int64 => 8,
                SpecialType.System_UInt64 => 8,
                SpecialType.System_Double => 8,
                SpecialType.System_Decimal => 16,
                _ => 0,
            };
            alignment = size switch {
                1 => 1,
                2 => 2,
                4 => 4,
                8 => 8,
                16 => 16,
                _ => 1,
            };
            return size > 0;
        }

        private static void GetLayout(
              INamedTypeSymbol type
            , CancellationToken token
            , out int layoutKind
            , out int pack
            , out int declaredSize
            , out bool hasLayoutAttribute
        )
        {
            layoutKind = 0;
            pack = 0;
            declaredSize = 0;
            hasLayoutAttribute = false;
            var attributes = type.GetAttributes();
            var attributeCount = attributes.Length;

            for (var i = 0; i < attributeCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var attribute = attributes[i];

                if (attribute.AttributeClass?.ToDisplayString()
                    != "System.Runtime.InteropServices.StructLayoutAttribute")
                {
                    continue;
                }

                hasLayoutAttribute = true;

                if (attribute.ConstructorArguments.Length > 0
                    && attribute.ConstructorArguments[0].Value is int value)
                {
                    layoutKind = value;
                }

                var namedArguments = attribute.NamedArguments;
                var namedArgumentCount = namedArguments.Length;

                for (var j = 0; j < namedArgumentCount; j++)
                {
                    token.ThrowIfCancellationRequested();
                    var argument = namedArguments[j];

                    if (argument.Key == "Pack" && argument.Value.Value is int packValue)
                    {
                        pack = packValue;
                    }
                    else if (argument.Key == "Size" && argument.Value.Value is int sizeValue)
                    {
                        declaredSize = sizeValue;
                    }
                }
            }
        }

        private static bool TryGetFieldOffset(IFieldSymbol field, CancellationToken token, out int offset)
        {
            var attributes = field.GetAttributes();
            var attributeCount = attributes.Length;

            for (var i = 0; i < attributeCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var attribute = attributes[i];

                if (attribute.AttributeClass?.ToDisplayString()
                        == "System.Runtime.InteropServices.FieldOffsetAttribute"
                    && attribute.ConstructorArguments.Length > 0
                    && attribute.ConstructorArguments[0].Value is int value)
                {
                    offset = value;
                    return true;
                }
            }

            offset = 0;
            return false;
        }

        private static bool IsOpen(ITypeSymbol type, CancellationToken token)
        {
            if (type is not INamedTypeSymbol named)
            {
                return false;
            }

            for (var current = named; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();
                var typeArguments = current.TypeArguments;
                var typeArgumentCount = typeArguments.Length;

                for (var i = 0; i < typeArgumentCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var argument = typeArguments[i];

                    if (argument.TypeKind == TypeKind.TypeParameter || IsOpen(argument, token))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int AlignUp(int value, int alignment)
        {
            alignment = Math.Max(1, alignment);
            return checked(((value + alignment - 1) / alignment) * alignment);
        }

        private static UnmanagedSizeUpperBoundResult Failure(UnmanagedSizeUpperBoundFailure failure)
            => new(false, 0, failure);

        private static Calculation FailureCalculation(UnmanagedSizeUpperBoundFailure failure)
            => new(Failure(failure), alignment: 1);

        private static Calculation Known(int size, int alignment)
            => new(new(true, size, UnmanagedSizeUpperBoundFailure.None), alignment);

        private static bool HasCompleteStorage(INamedTypeSymbol type, CancellationToken token)
        {
            var locations = type.Locations;
            var locationCount = locations.Length;

            for (var i = 0; i < locationCount; i++)
            {
                token.ThrowIfCancellationRequested();

                if (locations[i].IsInSource)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSupportedPack(int pack)
            => pack is 0 or 1 or 2 or 4 or 8 or 16 or 32 or 64 or 128;

        private readonly struct Calculation
        {
            public readonly UnmanagedSizeUpperBoundResult result;
            public readonly int alignment;

            public Calculation(UnmanagedSizeUpperBoundResult result, int alignment)
            {
                this.result = result;
                this.alignment = alignment;
            }
        }

        private readonly struct FieldCalculation
        {
            public readonly IFieldSymbol field;
            public readonly Calculation calculation;

            public FieldCalculation(IFieldSymbol field, Calculation calculation)
            {
                this.field = field;
                this.calculation = calculation;
            }
        }
    }
}
