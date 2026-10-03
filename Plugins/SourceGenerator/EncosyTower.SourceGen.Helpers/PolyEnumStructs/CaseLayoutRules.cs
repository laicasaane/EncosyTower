using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Helpers.PolyEnumStructs
{
    /// <summary>
    /// Diagnostic-neutral layout rules shared by the PolyEnumStruct generator and analyzer.
    /// </summary>
    public static class CaseLayoutRules
    {
        private const string STRUCT_LAYOUT_ATTRIBUTE = "global::System.Runtime.InteropServices.StructLayoutAttribute";

        /// <summary>
        /// Returns whether <paramref name="target"/> declares <c>[StructLayout(LayoutKind.Explicit)]</c>.
        /// </summary>
        public static bool IsExplicitLayout(INamedTypeSymbol target, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (target.TryGetAttribute(STRUCT_LAYOUT_ATTRIBUTE, out var layoutAttribute, token) == false)
            {
                return false;
            }

            var arguments = layoutAttribute.ConstructorArguments;
            var count = arguments.Length;

            for (var i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();

                if (arguments[i].Value is int layoutKind
                    && Enum.IsDefined(typeof(LayoutKind), layoutKind)
                    && (LayoutKind)layoutKind == LayoutKind.Explicit
                )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether <paramref name="field"/> is instance storage whose unmanaged size cannot be determined:
        /// an error type, or a value type whose computed size is 0 although it is not an empty struct.
        /// </summary>
        public static bool HasUnknownSize(IFieldSymbol field, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (field.IsStatic || field.IsConst)
            {
                return false;
            }

            var type = field.Type;

            if (type.TypeKind == TypeKind.Error)
            {
                return true;
            }

            if (type.IsReferenceType || type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer)
            {
                return false;
            }

            var size = 0;
            var alignment = 1;
            type.GetUnmanagedSizeAndAlignment(ref size, ref alignment, token);

            return size == 0 && IsEmptyStruct(type) == false;
        }

        /// <summary>
        /// Returns whether a value of <paramref name="type"/> can contain a managed reference: a reference type,
        /// a struct that stores one, or a type parameter not constrained to <c>unmanaged</c>.
        /// An error type returns <see langword="false"/>; <see cref="HasUnknownSize"/> covers it.
        /// </summary>
        public static bool CanHoldManagedReference(ITypeSymbol type)
            => type.TypeKind != TypeKind.Error && type.IsUnmanagedType == false;

        /// <summary>
        /// Returns whether <paramref name="eventSymbol"/> is a field-like event, whose delegate lives in a
        /// compiler-generated field that <c>GetMembers()</c> does not return.
        /// </summary>
        public static bool IsFieldLikeEvent(IEventSymbol eventSymbol)
            => eventSymbol.IsExtern == false && eventSymbol.AddMethod is { IsImplicitlyDeclared: true };

        private static bool IsEmptyStruct(ITypeSymbol type)
        {
            if (type.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            var members = type.GetMembers();
            var count = members.Length;

            for (var i = 0; i < count; i++)
            {
                if (members[i] is IFieldSymbol { IsStatic: false, IsConst: false })
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Finds the first named type that <paramref name="type"/> stores by value and <paramref name="match"/>
        /// accepts: the type, its containing types, and for a struct the types of its instance fields, recursively.
        /// Types behind a reference, array or pointer are not visited because they do not change the size.
        /// </summary>
        public static bool TryFindStoredType(
              ITypeSymbol type
            , Func<INamedTypeSymbol, CancellationToken, bool> match
            , CancellationToken token
            , out INamedTypeSymbol storedType
        )
        {
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            return TryFindStoredType(type, match, visited, token, out storedType);
        }

        private static bool TryFindStoredType(
              ITypeSymbol type
            , Func<INamedTypeSymbol, CancellationToken, bool> match
            , HashSet<ITypeSymbol> visited
            , CancellationToken token
            , out INamedTypeSymbol storedType
        )
        {
            token.ThrowIfCancellationRequested();
            storedType = null;

            if (type is not INamedTypeSymbol named || visited.Add(named) == false)
            {
                return false;
            }

            for (var current = named; current != null; current = current.ContainingType)
            {
                if (match(current, token))
                {
                    storedType = current;
                    return true;
                }
            }

            if (named.TypeKind != TypeKind.Struct)
            {
                return false;
            }

            var members = named.GetMembers();
            var count = members.Length;

            for (var i = 0; i < count; i++)
            {
                if (members[i] is IFieldSymbol { IsStatic: false, IsConst: false } field
                    && TryFindStoredType(field.Type, match, visited, token, out storedType)
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
