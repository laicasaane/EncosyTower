// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen
{
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    /// <summary>
    /// A model representing an attribute declaration.
    /// </summary>
    public readonly struct AttributeInfo : IEquatable<AttributeInfo>
    {
        public AttributeInfo(
              string typeName
            , EquatableArray<TypedConstantInfo> constructorArgumentInfo
            , EquatableArray<(string Name, TypedConstantInfo Value)> namedArgumentInfo
        ) : this(typeName, constructorArgumentInfo, namedArgumentInfo, default)
        { }

        public AttributeInfo(
              string typeName
            , EquatableArray<TypedConstantInfo> constructorArgumentInfo
            , EquatableArray<(string Name, TypedConstantInfo Value)> namedArgumentInfo
            , EquatableArray<string> constructorArgumentNames
        )
        {
            this.TypeName = typeName;
            this.ConstructorArgumentInfo = constructorArgumentInfo;
            this.NamedArgumentInfo = namedArgumentInfo;
            this.ConstructorArgumentNames = constructorArgumentNames;
        }

        public bool IsValid => string.IsNullOrEmpty(TypeName) == false;

        public string TypeName { get; }

        public EquatableArray<TypedConstantInfo> ConstructorArgumentInfo { get; }

        public EquatableArray<string> ConstructorArgumentNames { get; }

        public EquatableArray<(string Name, TypedConstantInfo Value)> NamedArgumentInfo { get; }

        /// <summary>
        /// Tries to create a new <see cref="AttributeInfo"/> instance from a given <see cref="AttributeData"/> value.
        /// </summary>
        /// <param name="attributeData">The input <see cref="AttributeData"/> value.</param>
        /// <param name="result">The created value; <see langword="default"/> on failure.</param>
        /// <returns>
        /// <see langword="false"/> when the attribute did not bind to a constructor or one of its arguments is not a
        /// valid constant.
        /// </returns>
        public static bool TryFrom(AttributeData attributeData, out AttributeInfo result)
        {
            if (attributeData.AttributeClass == null || attributeData.AttributeConstructor == null)
            {
                result = default;
                return false;
            }

            using var constructorArguments = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
            using var namedArguments = ImmutableArrayBuilder<(string, TypedConstantInfo)>.Rent();

            var constructorConstants = attributeData.ConstructorArguments;
            var constructorCount = constructorConstants.Length;

            for (var i = 0; i < constructorCount; i++)
            {
                if (TypedConstantInfo.TryFrom(constructorConstants[i], out var argumentInfo) == false)
                {
                    result = default;
                    return false;
                }

                constructorArguments.Add(argumentInfo);
            }

            var namedConstants = attributeData.NamedArguments;
            var namedCount = namedConstants.Length;

            for (var i = 0; i < namedCount; i++)
            {
                var namedConstant = namedConstants[i];

                if (TypedConstantInfo.TryFrom(namedConstant.Value, out var argumentInfo) == false)
                {
                    result = default;
                    return false;
                }

                namedArguments.Add((namedConstant.Key, argumentInfo));
            }

            result = new(
                  attributeData.AttributeClass.ToFullName()
                , constructorArguments.ToImmutable()
                , namedArguments.ToImmutable()
            );

            return true;
        }

        /// <summary>
        /// Tries to create a new <see cref="AttributeInfo"/> instance from a given syntax node.
        /// </summary>
        /// <param name="typeSymbol">The symbol for the attribute type.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="arguments">The sequence of <see cref="AttributeArgumentSyntax"/> instances to process.</param>
        /// <param name="result">The created value; <see langword="default"/> on failure.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <returns><see langword="false"/> when one of the arguments is not a valid constant.</returns>
        public static bool TryFrom(
              INamedTypeSymbol typeSymbol
            , SemanticModel semanticModel
            , IEnumerable<AttributeArgumentSyntax> arguments
            , out AttributeInfo result
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var constructorArguments = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
            using var constructorArgumentNames = ImmutableArrayBuilder<string>.Rent();
            using var namedArguments = ImmutableArrayBuilder<(string, TypedConstantInfo)>.Rent();

            foreach (var argument in arguments)
            {
                token.ThrowIfCancellationRequested();

                if (semanticModel.GetOperation(argument.Expression, token) is not IOperation operation)
                {
                    continue;
                }

                if (TypedConstantInfo.TryFrom(
                      operation
                    , semanticModel
                    , argument.Expression
                    , out var argumentInfo
                    , token
                ) == false
                )
                {
                    result = default;
                    return false;
                }

                if (argument.NameEquals?.Name.Identifier.ValueText is string argumentName)
                {
                    namedArguments.Add((argumentName, argumentInfo));
                }
                else
                {
                    constructorArguments.Add(argumentInfo);
                    constructorArgumentNames.Add(argument.NameColon?.Name.Identifier.ValueText ?? string.Empty);
                }
            }

            result = new(
                  typeSymbol.ToFullName()
                , constructorArguments.ToImmutable()
                , namedArguments.ToImmutable()
                , constructorArgumentNames.ToImmutable()
            );

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is AttributeInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashValue.Combine(TypeName, ConstructorArgumentInfo, NamedArgumentInfo, ConstructorArgumentNames);
        }

        public bool Equals(AttributeInfo other)
        {
            return string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
                && ConstructorArgumentInfo.Equals(other.ConstructorArgumentInfo)
                && NamedArgumentInfo.Equals(other.NamedArgumentInfo)
                && ConstructorArgumentNames.Equals(other.ConstructorArgumentNames);
        }

        /// <summary>
        /// Gets an <see cref="AttributeSyntax"/> instance representing the current value.
        /// </summary>
        /// <returns>The <see cref="ExpressionSyntax"/> instance representing the current value.</returns>
        public AttributeSyntax GetSyntax()
        {
            var constructorNames = ConstructorArgumentNames;
            var arguments = ConstructorArgumentInfo.Select((arg, index) => {
                var syntax = AttributeArgument(arg.GetSyntax());
                var name = constructorNames.Count > index ? constructorNames[index] : string.Empty;

                return string.IsNullOrEmpty(name)
                    ? syntax
                    : syntax.WithNameColon(NameColon(IdentifierName(name.EscapeCSharpIdentifier())));
            });
            var namedArguments = NamedArgumentInfo.Select(static arg => AttributeArgument(arg.Value.GetSyntax())
                .WithNameEquals(NameEquals(IdentifierName(arg.Name))));

            return Attribute(
                  IdentifierName(TypeName)
                , AttributeArgumentList(SeparatedList(arguments.Concat(namedArguments)))
            );
        }
    }
}
