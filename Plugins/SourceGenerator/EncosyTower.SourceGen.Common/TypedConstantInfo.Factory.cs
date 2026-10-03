// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace EncosyTower.SourceGen
{
    /// <inheritdoc/>
    partial class TypedConstantInfo
    {
        /// <summary>
        /// Tries to create a <see cref="TypedConstantInfo"/> from a <see cref="TypedConstant"/> value.
        /// </summary>
        /// <param name="arg">The input <see cref="TypedConstant"/> value.</param>
        /// <param name="result">The created value, or <see langword="null"/> on failure.</param>
        /// <returns>
        /// <see langword="false"/> when <paramref name="arg"/> or one of its array elements is not a valid constant,
        /// for example an argument that names a member of a type the compilation cannot see.
        /// </returns>
        public static bool TryFrom(TypedConstant arg, out TypedConstantInfo result)
        {
            if (arg.Kind == TypedConstantKind.Error)
            {
                result = null;
                return false;
            }

            if (arg.IsNull)
            {
                result = new Null();
                return true;
            }

            if (arg.Kind == TypedConstantKind.Array)
            {
                return TryFromArray(arg, out result);
            }

            var format = SymbolDisplayFormat.FullyQualifiedFormat;

            result = (arg.Kind, arg.Value) switch {
                (TypedConstantKind.Primitive, string text) => new Primitive.String(text),
                (TypedConstantKind.Primitive, bool flag) => new Primitive.Boolean(flag),
                (TypedConstantKind.Primitive, object value) => CreateNumericOrNull(value),
                (TypedConstantKind.Type, ITypeSymbol type) => new Type(type.ToDisplayString(format)),
                (TypedConstantKind.Enum, object value) => new Enum(arg.Type.ToDisplayString(format), value),
                _ => null,
            };

            return result != null;
        }

        /// <summary>
        /// Tries to create a <see cref="TypedConstantInfo"/> from an attribute argument <see cref="IOperation"/>.
        /// </summary>
        /// <param name="operation">The input <see cref="IOperation"/> value.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> that produced the operation.</param>
        /// <param name="expression">The <see cref="ExpressionSyntax"/> that produced the operation.</param>
        /// <param name="result">The created value, or <see langword="null"/> on failure.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <returns>
        /// <see langword="false"/> when <paramref name="operation"/> or one of its array elements is not a constant,
        /// a <see langword="typeof"/> expression, or an array creation.
        /// </returns>
        public static bool TryFrom(
              IOperation operation
            , SemanticModel semanticModel
            , ExpressionSyntax expression
            , out TypedConstantInfo result
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var format = SymbolDisplayFormat.FullyQualifiedFormat;

            if (operation.ConstantValue.HasValue)
            {
                var value = operation.ConstantValue.Value;

                // Enum values are constant but need to be checked explicitly in this case
                if (operation.Type?.TypeKind is TypeKind.Enum)
                {
                    result = new Enum(operation.Type.ToDisplayString(format), value);
                    return true;
                }

                // Handle all other constant literals normally
                result = value switch {
                    null => new Null(),
                    string text => new Primitive.String(text),
                    bool flag => new Primitive.Boolean(flag),
                    _ => CreateNumericOrNull(value),
                };

                return result != null;
            }

            if (operation is ITypeOfOperation typeOfOperation)
            {
                result = new Type(typeOfOperation.TypeOperand.ToDisplayString(format));
                return true;
            }

            if (operation is IArrayCreationOperation)
            {
                return TryFromArrayCreation(operation, semanticModel, expression, out result, token);
            }

            result = null;
            return false;
        }

        private static bool TryFromArray(TypedConstant arg, out TypedConstantInfo result)
        {
            if (arg.Type is not IArrayTypeSymbol arrayType)
            {
                result = null;
                return false;
            }

            var values = arg.Values;
            var count = values.Length;
            using var items = ImmutableArrayBuilder<TypedConstantInfo>.Rent();

            for (var i = 0; i < count; i++)
            {
                if (TryFrom(values[i], out var item) == false)
                {
                    result = null;
                    return false;
                }

                items.Add(item);
            }

            var elementTypeName = arrayType.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            result = new Array(elementTypeName, items.ToImmutable());
            return true;
        }

        private static bool TryFromArrayCreation(
              IOperation operation
            , SemanticModel semanticModel
            , ExpressionSyntax expression
            , out TypedConstantInfo result
            , CancellationToken token
        )
        {
            // If the element type is not available (since the attribute wasn't checked), just default to object
            var elementTypeName = operation.Type is IArrayTypeSymbol arrayType
                ? arrayType.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                : "object";

            var initializer = (expression as ImplicitArrayCreationExpressionSyntax)?.Initializer
                ?? (expression as ArrayCreationExpressionSyntax)?.Initializer;

            // No initializer found, just return an empty array
            if (initializer == null)
            {
                result = new Array(elementTypeName, ImmutableArray<TypedConstantInfo>.Empty);
                return true;
            }

            var elements = initializer.Expressions;
            var count = elements.Count;
            using var items = ImmutableArrayBuilder<TypedConstantInfo>.Rent();

            // Enumerate all array elements and extract serialized info for them
            for (var i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();

                var element = elements[i];

                if (semanticModel.GetOperation(element, token) is not IOperation elementOperation
                    || TryFrom(elementOperation, semanticModel, element, out var item, token) == false
                )
                {
                    result = null;
                    return false;
                }

                items.Add(item);
            }

            result = new Array(elementTypeName, items.ToImmutable());
            return true;
        }

        private static TypedConstantInfo CreateNumericOrNull(object value)
            => value switch {
                byte b => new Primitive.Of<byte>(b),
                char c => new Primitive.Of<char>(c),
                double d => new Primitive.Of<double>(d),
                float f => new Primitive.Of<float>(f),
                int i => new Primitive.Of<int>(i),
                long l => new Primitive.Of<long>(l),
                sbyte sb => new Primitive.Of<sbyte>(sb),
                short sh => new Primitive.Of<short>(sh),
                uint ui => new Primitive.Of<uint>(ui),
                ulong ul => new Primitive.Of<ulong>(ul),
                ushort ush => new Primitive.Of<ushort>(ush),
                _ => null,
            };
    }
}
