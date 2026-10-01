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
        /// Creates a new <see cref="AttributeInfo"/> instance from a given <see cref="AttributeData"/> value.
        /// </summary>
        /// <param name="attributeData">The input <see cref="AttributeData"/> value.</param>
        /// <returns>A <see cref="AttributeInfo"/> instance representing <paramref name="attributeData"/>.</returns>
        public static AttributeInfo From(AttributeData attributeData)
        {
            string typeName = attributeData.AttributeClass!.ToFullName();

            using var constructorArguments = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
            using var namedArguments = ImmutableArrayBuilder<(string, TypedConstantInfo)>.Rent();

            foreach (TypedConstant typedConstant in attributeData.ConstructorArguments)
            {
                constructorArguments.Add(TypedConstantInfo.From(typedConstant));
            }

            foreach (KeyValuePair<string, TypedConstant> namedConstant in attributeData.NamedArguments)
            {
                namedArguments.Add((namedConstant.Key, TypedConstantInfo.From(namedConstant.Value)));
            }

            return new(typeName, constructorArguments.ToImmutable(), namedArguments.ToImmutable());
        }

        /// <summary>
        /// Creates a new <see cref="AttributeInfo"/> instance from a given syntax node.
        /// </summary>
        /// <param name="typeSymbol">The symbol for the attribute type.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="arguments">The sequence of <see cref="AttributeArgumentSyntax"/> instances to process.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <returns>A <see cref="AttributeInfo"/> instance representing the input attribute data.</returns>
        public static AttributeInfo From(
              INamedTypeSymbol typeSymbol
            , SemanticModel semanticModel
            , IEnumerable<AttributeArgumentSyntax> arguments
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            string typeName = typeSymbol.ToFullName();

            using var constructorArguments = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
            using var constructorArgumentNames = ImmutableArrayBuilder<string>.Rent();
            using var namedArguments = ImmutableArrayBuilder<(string, TypedConstantInfo)>.Rent();

            foreach (AttributeArgumentSyntax argument in arguments)
            {
                token.ThrowIfCancellationRequested();

                if (semanticModel.GetOperation(argument.Expression, token) is not IOperation operation)
                {
                    continue;
                }

                var argumentInfo = TypedConstantInfo.From(operation, semanticModel, argument.Expression, token);

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

            return new(
                  typeName
                , constructorArguments.ToImmutable()
                , namedArguments.ToImmutable()
                , constructorArgumentNames.ToImmutable()
            );
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
