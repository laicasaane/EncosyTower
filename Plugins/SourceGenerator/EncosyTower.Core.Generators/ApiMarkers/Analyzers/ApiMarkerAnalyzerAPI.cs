using Microsoft.CodeAnalysis.Operations;

namespace EncosyTower.Core.Analyzers.ApiMarkers
{
    internal static class ApiMarkerAnalyzerAPI
    {
        internal static ImmutableArray<SyntaxKind> NameKinds { get; } = ImmutableArray.Create(
              SyntaxKind.IdentifierName
            , SyntaxKind.GenericName
        );

        internal static ImmutableArray<SyntaxKind> UseKinds { get; } = ImmutableArray.Create(
              SyntaxKind.InvocationExpression
            , SyntaxKind.ObjectCreationExpression
            , SyntaxKind.ImplicitObjectCreationExpression
            , SyntaxKind.Attribute
            , SyntaxKind.BaseConstructorInitializer
            , SyntaxKind.ThisConstructorInitializer
            , SyntaxKind.ElementAccessExpression
            , SyntaxKind.ElementBindingExpression
            , SyntaxKind.AddExpression
            , SyntaxKind.SubtractExpression
            , SyntaxKind.MultiplyExpression
            , SyntaxKind.DivideExpression
            , SyntaxKind.ModuloExpression
            , SyntaxKind.LeftShiftExpression
            , SyntaxKind.RightShiftExpression
            , SyntaxKind.BitwiseAndExpression
            , SyntaxKind.BitwiseOrExpression
            , SyntaxKind.ExclusiveOrExpression
            , SyntaxKind.LogicalAndExpression
            , SyntaxKind.LogicalOrExpression
            , SyntaxKind.EqualsExpression
            , SyntaxKind.NotEqualsExpression
            , SyntaxKind.LessThanExpression
            , SyntaxKind.LessThanOrEqualExpression
            , SyntaxKind.GreaterThanExpression
            , SyntaxKind.GreaterThanOrEqualExpression
            , SyntaxKind.UnaryPlusExpression
            , SyntaxKind.UnaryMinusExpression
            , SyntaxKind.BitwiseNotExpression
            , SyntaxKind.LogicalNotExpression
            , SyntaxKind.PreIncrementExpression
            , SyntaxKind.PreDecrementExpression
            , SyntaxKind.PostIncrementExpression
            , SyntaxKind.PostDecrementExpression
            , SyntaxKind.AddAssignmentExpression
            , SyntaxKind.SubtractAssignmentExpression
            , SyntaxKind.MultiplyAssignmentExpression
            , SyntaxKind.DivideAssignmentExpression
            , SyntaxKind.ModuloAssignmentExpression
            , SyntaxKind.AndAssignmentExpression
            , SyntaxKind.OrAssignmentExpression
            , SyntaxKind.ExclusiveOrAssignmentExpression
            , SyntaxKind.LeftShiftAssignmentExpression
            , SyntaxKind.RightShiftAssignmentExpression
            , SyntaxKind.CastExpression
        );

        internal static ImmutableArray<SyntaxKind> ConversionKinds { get; } = ImmutableArray.Create(
              SyntaxKind.Argument
            , SyntaxKind.EqualsValueClause
            , SyntaxKind.ReturnStatement
            , SyntaxKind.ArrowExpressionClause
            , SyntaxKind.SimpleAssignmentExpression
            , SyntaxKind.SwitchExpressionArm
        );

        internal static void AnalyzeName(
              SyntaxNodeAnalysisContext context
            , string marker
            , ReadOnlySpan<string> allowedSymbols
            , ReadOnlySpan<string> exemptionMarkers
            , DiagnosticDescriptor descriptor
        )
        {
            var name = (SimpleNameSyntax)context.Node;

            if (IsAllowedBuild(context, allowedSymbols) || IsInsideNameOf(context) || IsOwnedByUse(name))
            {
                return;
            }

            var symbol = context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol;

            if (symbol is IAliasSymbol alias)
            {
                symbol = alias.Target;
            }

            if (symbol is not (INamedTypeSymbol or IMethodSymbol or IPropertySymbol or IFieldSymbol or IEventSymbol))
            {
                return;
            }

            var implicitType = name is IdentifierNameSyntax identifier
                && identifier.Identifier.Text == "var"
                && symbol.Name != "var"
                && context.SemanticModel.GetAliasInfo(identifier, context.CancellationToken) == null
                && (name.Parent is VariableDeclarationSyntax declaration && declaration.Type == name
                    || name.Parent is DeclarationExpressionSyntax expression && expression.Type == name
                );

            if (implicitType || (symbol is INamedTypeSymbol && IsRestrictedQualifier(context, name, marker)))
            {
                return;
            }

            ReportUse(context, symbol, name.Identifier.GetLocation(), marker, exemptionMarkers, descriptor);
        }

        internal static void AnalyzeUse(
              SyntaxNodeAnalysisContext context
            , string marker
            , ReadOnlySpan<string> allowedSymbols
            , ReadOnlySpan<string> exemptionMarkers
            , DiagnosticDescriptor descriptor
        )
        {
            if (IsAllowedBuild(context, allowedSymbols) || IsInsideNameOf(context))
            {
                return;
            }

            var symbol = context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol;

            if (symbol is IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove })
            {
                return;
            }

            if (symbol != null)
            {
                ReportUse(context, symbol, GetUseLocation(context.Node), marker, exemptionMarkers, descriptor);
            }
        }

        internal static void AnalyzeConversion(
              SyntaxNodeAnalysisContext context
            , string marker
            , ReadOnlySpan<string> allowedSymbols
            , ReadOnlySpan<string> exemptionMarkers
            , DiagnosticDescriptor descriptor
        )
        {
            if (IsAllowedBuild(context, allowedSymbols) || IsInsideNameOf(context))
            {
                return;
            }

            var expression = context.Node switch {
                ArgumentSyntax argument => argument.Expression,
                EqualsValueClauseSyntax initializer => initializer.Value,
                ReturnStatementSyntax statement => statement.Expression,
                ArrowExpressionClauseSyntax arrow => arrow.Expression,
                AssignmentExpressionSyntax assignment => assignment.Right,
                SwitchExpressionArmSyntax arm => arm.Expression,
                _ => null,
            };

            if (expression == null)
            {
                return;
            }

            var conversion = context.SemanticModel.GetConversion(expression, context.CancellationToken);

            if (conversion.MethodSymbol != null)
            {
                ReportUse(
                      context
                    , conversion.MethodSymbol
                    , expression.GetLocation()
                    , marker
                    , exemptionMarkers
                    , descriptor
                );
            }
        }

        private static bool HasSymbol(SyntaxTree tree, string symbol)
            => tree.Options is CSharpParseOptions options && options.PreprocessorSymbolNames.Contains(symbol);

        private static bool IsInsideNameOf(SyntaxNodeAnalysisContext context)
        {
            for (var node = context.Node; node != null; node = node.Parent)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (node is InvocationExpressionSyntax invocation
                    && invocation.Expression is IdentifierNameSyntax name
                    && name.Identifier.ValueText == "nameof"
                    && context.SemanticModel.GetOperation(invocation, context.CancellationToken) is INameOfOperation
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsOwnedByUse(SimpleNameSyntax name)
        {
            SyntaxNode node = name;

            while (node.Parent is QualifiedNameSyntax qualified && qualified.Right == node
                || node.Parent is AliasQualifiedNameSyntax
            )
            {
                node = node.Parent;
            }

            if (node.Parent is ObjectCreationExpressionSyntax creation && creation.Type == node
                || node.Parent is AttributeSyntax attribute && attribute.Name == node
            )
            {
                return true;
            }

            if (node.Parent is MemberAccessExpressionSyntax access && access.Name == node
                || node.Parent is MemberBindingExpressionSyntax
            )
            {
                node = node.Parent;
            }

            return node.Parent is InvocationExpressionSyntax invocation && invocation.Expression == node;
        }

        private static bool HasUseMarker(SyntaxNodeAnalysisContext context, ISymbol symbol, string marker)
        {
            var token = context.CancellationToken;

            if (HasMarker(symbol, marker, token))
            {
                return true;
            }

            SyntaxNode expression = context.Node;

            if (expression.Parent is MemberAccessExpressionSyntax or MemberBindingExpressionSyntax)
            {
                expression = expression.Parent;
            }

            if (symbol is IEventSymbol item)
            {
                var add = expression.Parent.IsKind(SyntaxKind.AddAssignmentExpression);
                var remove = expression.Parent.IsKind(SyntaxKind.SubtractAssignmentExpression);
                return (add && HasMarker(item.AddMethod, marker, token))
                    || (remove && HasMarker(item.RemoveMethod, marker, token));
            }

            if (symbol is not IPropertySymbol property)
            {
                return false;
            }

            var isAssignment = expression.Parent is AssignmentExpressionSyntax assignment
                && assignment.Left == expression;

            var isIncrement = (expression.Parent is PrefixUnaryExpressionSyntax prefix
                    && prefix.Kind() is SyntaxKind.PreIncrementExpression or SyntaxKind.PreDecrementExpression
                )
                || expression.Parent is PostfixUnaryExpressionSyntax;

            var writes = isAssignment || isIncrement;
            var reads = isAssignment == false
                || expression.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression) == false;

            return (reads && HasMarker(property.GetMethod, marker, token))
                || (writes && HasMarker(property.SetMethod, marker, token));
        }

        private static ISymbol GetCaller(SyntaxNodeAnalysisContext context)
        {
            for (var node = context.Node.Parent; node != null; node = node.Parent)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (node is BaseMethodDeclarationSyntax
                    or BasePropertyDeclarationSyntax
                    or LocalFunctionStatementSyntax
                    or AccessorDeclarationSyntax
                    or BaseTypeDeclarationSyntax
                    or DelegateDeclarationSyntax
                    or VariableDeclaratorSyntax
                )
                {
                    var symbol = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);

                    if (symbol != null)
                    {
                        return symbol;
                    }
                }

                if (node is BaseFieldDeclarationSyntax field && field.Declaration.Variables.Count > 0)
                {
                    return context.SemanticModel.GetDeclaredSymbol(
                          field.Declaration.Variables[0]
                        , context.CancellationToken
                    );
                }
            }

            return context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart, context.CancellationToken);
        }

        private static bool IsExempt(ISymbol caller, string marker, CancellationToken token)
        {
            for (var symbol = caller; symbol != null; symbol = symbol.ContainingSymbol)
            {
                token.ThrowIfCancellationRequested();

                if (symbol is INamedTypeSymbol or IMethodSymbol or IPropertySymbol or IFieldSymbol or IEventSymbol
                    && HasMarker(symbol, marker, token)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasMarker(ISymbol symbol, string marker, CancellationToken token)
        {
            if (symbol is IMethodSymbol method)
            {
                symbol = method.ReducedFrom ?? method;

                if (HasDirectMarker(method.AssociatedSymbol, marker, token))
                {
                    return true;
                }
            }

            if (HasDirectMarker(symbol, marker, token))
            {
                return true;
            }

            for (var type = symbol?.ContainingType; type != null; type = type.ContainingType)
            {
                token.ThrowIfCancellationRequested();

                if (HasDirectMarker(type, marker, token))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDirectMarker(ISymbol symbol, string marker, CancellationToken token)
        {
            if (symbol == null)
            {
                return false;
            }

            var attributes = symbol.GetAttributes();
            var count = attributes.Length;

            for (var i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();

                if (attributes[i].AttributeClass?.ToDisplayString() == marker)
                {
                    return true;
                }
            }

            return false;
        }

        private static Location GetUseLocation(SyntaxNode node)
            => node switch {
                InvocationExpressionSyntax invocation => GetUseLocation(invocation.Expression),
                MemberAccessExpressionSyntax access => access.Name.Identifier.GetLocation(),
                MemberBindingExpressionSyntax binding => binding.Name.Identifier.GetLocation(),
                SimpleNameSyntax name => name.Identifier.GetLocation(),
                QualifiedNameSyntax qualified => qualified.Right.Identifier.GetLocation(),
                AliasQualifiedNameSyntax alias => alias.Name.Identifier.GetLocation(),
                ObjectCreationExpressionSyntax creation => GetUseLocation(creation.Type),
                ImplicitObjectCreationExpressionSyntax creation => creation.NewKeyword.GetLocation(),
                AttributeSyntax attribute => GetUseLocation(attribute.Name),
                ConstructorInitializerSyntax initializer => initializer.ThisOrBaseKeyword.GetLocation(),
                ElementAccessExpressionSyntax element => element.ArgumentList.OpenBracketToken.GetLocation(),
                ElementBindingExpressionSyntax element => element.ArgumentList.OpenBracketToken.GetLocation(),
                BinaryExpressionSyntax binary => binary.OperatorToken.GetLocation(),
                PrefixUnaryExpressionSyntax prefix => prefix.OperatorToken.GetLocation(),
                PostfixUnaryExpressionSyntax postfix => postfix.OperatorToken.GetLocation(),
                AssignmentExpressionSyntax assignment => assignment.OperatorToken.GetLocation(),
                CastExpressionSyntax cast => cast.OpenParenToken.GetLocation(),
                _ => node.GetLocation(),
            };

        private static bool IsAllowedBuild(SyntaxNodeAnalysisContext context, ReadOnlySpan<string> allowedSymbols)
        {
            var count = allowedSymbols.Length;

            for (var i = 0; i < count; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (HasSymbol(context.Node.SyntaxTree, allowedSymbols[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRestrictedQualifier(
              SyntaxNodeAnalysisContext context
            , SimpleNameSyntax name
            , string marker
        )
        {
            SyntaxNode node = name;

            while (node.Parent is QualifiedNameSyntax or AliasQualifiedNameSyntax or MemberAccessExpressionSyntax)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var parent = node.Parent;
                var isQualifier = parent is QualifiedNameSyntax qualified && qualified.Left == node
                    || parent is MemberAccessExpressionSyntax access && access.Expression == node;

                if (isQualifier)
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(parent, context.CancellationToken).Symbol;

                    if (HasMarker(symbol, marker, context.CancellationToken))
                    {
                        return true;
                    }
                }

                node = parent;
            }

            return false;
        }

        private static void ReportUse(
              SyntaxNodeAnalysisContext context
            , ISymbol symbol
            , Location location
            , string marker
            , ReadOnlySpan<string> exemptionMarkers
            , DiagnosticDescriptor descriptor
        )
        {
            if (HasUseMarker(context, symbol, marker) == false)
            {
                return;
            }

            var token = context.CancellationToken;
            var caller = GetCaller(context);
            var count = exemptionMarkers.Length;

            for (var i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();

                if (IsExempt(caller, exemptionMarkers[i], token))
                {
                    return;
                }
            }

            symbol = symbol is IMethodSymbol method ? method.ReducedFrom ?? method : symbol;

            context.ReportDiagnostic(Diagnostic.Create(
                  descriptor
                , location
                , symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
            ));
        }
    }
}
