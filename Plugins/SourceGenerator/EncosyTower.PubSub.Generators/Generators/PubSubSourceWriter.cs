namespace EncosyTower.PubSub.Generators
{
    internal static class PubSubSourceWriter
    {
        private const string AGGRESSIVE_INLINING =
            "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";

        public static SourceText WriteMessage(in PubSubMessageSpec spec, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var printer = new Printer(0, 1024 * 16, token);
            WriteDeclarationHeader(ref printer, spec.Declaration, spec.WithSync);
            printer.OpenScope();
            {
                if (spec.WithAsync)
                {
                    WriteAsyncStorage(ref printer, spec);
                }
            }
            printer.CloseScope();
            return CreateSource(spec.Declaration, printer.Result, token);
        }

        public static SourceText WriteScope(in PubSubScopeSpec spec, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var printer = new Printer(0, 1024 * 16, token);
            WriteScopeDeclaration(ref printer, spec);
            return CreateSource(spec.Declaration, printer.Result, token);
        }

        private static SourceText CreateSource(
              PubSubTypeDeclarationSpec declaration
            , string body
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var printer = new Printer(0, 1024 * 16, token);
            var source = TypeCreationHelpers.GenerateSourceText(
                  declaration.OpeningSource
                , body
                , declaration.ClosingSource
                , token
                , overridePrinter: printer
            ).ToString();
            token.ThrowIfCancellationRequested();
            printer.Clear().Print(source.TrimEnd('\r', '\n')).PrintEndLine();
            return SourceText.From(printer.Result, Encoding.UTF8);
        }

        private static void WriteDeclarationHeader(
              ref Printer printer
            , PubSubTypeDeclarationSpec declaration
            , bool withMessageInterface
        )
        {
            printer.PrintBeginLine("partial ").Print(declaration.TypeKeyword).Print(" ")
                .Print(declaration.TypeName)
                .PrintEndLineIf(withMessageInterface, " : g__ETPS.IMessage", string.Empty);
        }

        private static void WriteAsyncStorage(ref Printer printer, PubSubMessageSpec spec)
        {
            var storageType = spec.Declaration.FullTypeName;
            printer.PrintLine("public readonly partial struct Async : g__ETPS.IMessage");
            printer.OpenScope();
            {
                printer.PrintBeginLine("private readonly ").Print(storageType).PrintEndLine(" _value;");
                printer.PrintEndLine();
                printer.PrintLine(AGGRESSIVE_INLINING);
                printer.PrintBeginLine("private Async(").Print(storageType).PrintEndLine(" value)");
                printer.OpenScope();
                {
                    printer.PrintLine("_value = value;");
                }
                printer.CloseScope();
                printer.PrintEndLine();
                printer.PrintLine(AGGRESSIVE_INLINING);
                printer.PrintBeginLine("public static implicit operator Async(")
                    .Print(storageType).PrintEndLine(" value)");
                printer.OpenScope();
                {
                    printer.PrintLine("return new Async(value);");
                }
                printer.CloseScope();
                printer.PrintEndLine();
                printer.PrintLine(AGGRESSIVE_INLINING);
                printer.PrintBeginLine("public static implicit operator ").Print(storageType)
                    .PrintEndLine("(Async value)");
                printer.OpenScope();
                {
                    printer.PrintLine("return value._value;");
                }
                printer.CloseScope();
            }
            printer.CloseScope();
        }

        private static void WriteScopeDeclaration(ref Printer printer, PubSubScopeSpec spec)
        {
            WriteDeclarationHeader(ref printer, spec.Declaration, false);
            printer.OpenScope();
            {
                if (spec.WithSync)
                {
                    WriteScopeMembers(ref printer, spec, false, false);

                    if (spec.IsGlobalScope)
                    {
                        printer.PrintEndLine();
                        WriteScopeMembers(ref printer, spec, false, true);
                    }
                }

                if (spec.WithAsync)
                {
                    printer.PrintEndLineIf(spec.WithSync, string.Empty);
                    printer.PrintLine("public readonly partial struct Async");
                    printer.OpenScope();
                    {
                        WriteScopeMembers(ref printer, spec, true, false);

                        if (spec.IsGlobalScope)
                        {
                            printer.PrintEndLine();
                            WriteScopeMembers(ref printer, spec, true, true);
                        }
                    }
                    printer.CloseScope();
                }
            }
            printer.CloseScope();
        }

        private static void WriteScopeMembers(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool globalShortcut
        )
        {
            if (asynchronous == false)
            {
                WriteCache(ref printer, scope, globalShortcut);
            }

            if (scope.CanPublishParameterless)
            {
                WritePublish(ref printer, scope, asynchronous, globalShortcut, parameterless: true);
            }

            WritePublish(ref printer, scope, asynchronous, globalShortcut, parameterless: false);

            if (scope.WithStateless)
            {
                WriteSubscriptions(
                      ref printer
                    , scope
                    , asynchronous
                    , globalShortcut
                    , stateful: false
                    , separateState: false
                );
            }

            if (scope.WithStateful)
            {
                if (globalShortcut == false)
                {
                    WriteSubscriptions(
                          ref printer
                        , scope
                        , asynchronous
                        , globalShortcut
                        , stateful: true
                        , separateState: false
                    );
                }

                WriteSubscriptions(
                      ref printer
                    , scope
                    , asynchronous
                    , globalShortcut
                    , stateful: true
                    , separateState: true
                );
            }
        }

        private static void WriteCache(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool globalShortcut
        )
        {
            WriteAggressiveInlining(ref printer);
            WriteCacheSignature(ref printer, scope, globalShortcut);

            printer.OpenScope();
            {
                WriteGlobalPublisherIfNeeded(ref printer, globalShortcut);
                printer.PrintLine("return publisher.Cache(factory, logger);");
            }
            printer.CloseScope();
            printer.PrintEndLine();
        }

        private static void WriteCacheSignature(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool globalShortcut
        )
        {
            printer.PrintBeginLine("public static ");
            WriteCachedPublisherType(ref printer, scope);
            printer.Print(" Cache(");
            var hasParameter = false;

            if (globalShortcut == false)
            {
                WriteParameterSeparator(ref printer, ref hasParameter);
                WritePublisherParameter(ref printer, scope);
            }

            WriteParameterSeparator(ref printer, ref hasParameter);
            WriteFactoryParameter(ref printer, scope);
            WriteParameterSeparator(ref printer, ref hasParameter);
            WriteLoggerParameter(ref printer);
            printer.PrintEndLine(")");
        }

        private static void WritePublish(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool globalShortcut
            , bool parameterless
        )
        {
            WriteAggressiveInlining(ref printer);
            WritePublishSignature(
                  ref printer
                , scope
                , asynchronous
                , globalShortcut
                , parameterless
            );
            printer.OpenScope();
            {
                WriteGlobalPublisherIfNeeded(ref printer, globalShortcut);

                if (parameterless)
                {
                    if (asynchronous)
                    {
                        printer.PrintBeginLine("return publisher.PublishAsync(new Async(new ")
                            .Print(scope.Declaration.FullTypeName).PrintEndLine("()), context);");
                    }
                    else
                    {
                        printer.PrintBeginLine("publisher.Publish<").Print(scope.Declaration.FullTypeName)
                            .PrintEndLine(">(context);");
                    }
                }
                else
                {
                    printer.PrintBeginLineIf(asynchronous, "return ", string.Empty).Print("publisher.")
                        .PrintIf(asynchronous, "PublishAsync", "Publish").PrintEndLine("(message, context);");
                }
            }
            printer.CloseScope();
            printer.PrintEndLine();
        }

        private static void WritePublishSignature(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool globalShortcut
            , bool parameterless
        )
        {
            printer.PrintBeginLine("public static ");
            WritePublishReturnType(ref printer, asynchronous);
            printer.Print(" Publish(");
            var hasParameter = false;

            WritePublishParameters(
                  ref printer
                , scope
                , asynchronous
                , globalShortcut
                , parameterless
                , ref hasParameter
            );
            printer.PrintEndLine(")");
        }

        private static void WritePublishParameters(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool globalShortcut
            , bool parameterless
            , ref bool hasParameter
        )
        {
            if (globalShortcut == false)
            {
                WriteParameterSeparator(ref printer, ref hasParameter);
                WritePublisherParameter(ref printer, scope);
            }

            if (parameterless == false)
            {
                WriteParameterSeparator(ref printer, ref hasParameter);
                printer.PrintIf(asynchronous, "Async", scope.Declaration.FullTypeName).Print(" message");
            }

            WriteParameterSeparator(ref printer, ref hasParameter);
            printer.Print(PubSubAliasSet.PUBSUB).Print(".PublishingContext context = default");
        }

        private static void WriteSubscriptions(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool globalShortcut
            , bool stateful
            , bool separateState
        )
        {
            for (var handler = 0; handler < 4; handler++)
            {
                WriteSubscribe(
                      ref printer
                    , scope
                    , asynchronous
                    , globalShortcut
                    , stateful
                    , separateState
                    , handler
                    , tokenOwned: false
                );
                WriteSubscribe(
                      ref printer
                    , scope
                    , asynchronous
                    , globalShortcut
                    , stateful
                    , separateState
                    , handler
                    , tokenOwned: true
                );
            }
        }

        private static void WriteSubscribe(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool globalShortcut
            , bool stateful
            , bool separateState
            , int handler
            , bool tokenOwned
        )
        {
            WriteAggressiveInlining(ref printer);
            WriteSubscribeSignature(
                  ref printer
                , scope
                , asynchronous
                , globalShortcut
                , stateful
                , separateState
                , handler
                , tokenOwned
            );

            if (stateful)
            {
                printer.WithIncreasedIndent().PrintLine("where TState : class");
            }

            printer.OpenScope();
            {
                WriteGlobalSubscriberIfNeeded(ref printer, globalShortcut);

                if (separateState)
                {
                    printer.PrintBeginLine("var statefulSubscriber = ").Print(PubSubAliasSet.PUBSUB)
                        .PrintEndLine(".MessageSubscriberExtensions.WithState(in subscriber, state);");
                }

                printer.PrintBeginLineIf(tokenOwned == false, "return ", string.Empty)
                    .PrintIf(separateState, "statefulSubscriber", "subscriber").Print(".Subscribe<")
                    .PrintIf(asynchronous, "Async", scope.Declaration.FullTypeName).Print(">(handler");

                if (tokenOwned)
                {
                    printer.Print(", unsubscribeToken");
                }

                printer.PrintEndLine(", order, logger);");
            }
            printer.CloseScope();
            printer.PrintEndLine();
        }

        private static void WriteSubscribeSignature(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool globalShortcut
            , bool stateful
            , bool separateState
            , int handler
            , bool tokenOwned
        )
        {
            printer.PrintBeginLine("public static ");

            if (tokenOwned)
            {
                printer.Print("void");
            }
            else
            {
                printer.Print(PubSubAliasSet.PUBSUB).Print(".ISubscription");
            }

            printer.Print(" Subscribe").PrintIf(stateful, "<TState>").Print("(");
            var hasParameter = false;

            if (globalShortcut == false)
            {
                WriteParameterSeparator(ref printer, ref hasParameter);
                WriteSubscriberParameter(ref printer, scope, stateful && separateState == false);
            }

            if (separateState)
            {
                WriteParameterSeparator(ref printer, ref hasParameter);
                printer.Print("[").Print(PubSubAliasSet.CODE_ANALYSIS).Print(".NotNull] TState state");
            }

            WriteParameterSeparator(ref printer, ref hasParameter);
            printer.Print("[").Print(PubSubAliasSet.CODE_ANALYSIS).Print(".NotNull] ");
            WriteHandlerType(ref printer, scope, asynchronous, stateful, handler);
            printer.Print(" handler");

            if (tokenOwned)
            {
                WriteParameterSeparator(ref printer, ref hasParameter);
                printer.Print(PubSubAliasSet.THREADING).Print(".CancellationToken unsubscribeToken");
            }

            WriteParameterSeparator(ref printer, ref hasParameter);
            printer.Print("int order = 0");
            WriteParameterSeparator(ref printer, ref hasParameter);
            WriteLoggerParameter(ref printer);
            printer.PrintEndLine(")");
        }

        private static void WriteAggressiveInlining(ref Printer printer)
        {
            printer.PrintBeginLine("[").Print(PubSubAliasSet.RUNTIME_COMPILER_SERVICES).Print(".MethodImpl(")
                .Print(PubSubAliasSet.RUNTIME_COMPILER_SERVICES)
                .PrintEndLine(".MethodImplOptions.AggressiveInlining)]");
        }

        private static void WritePublisherParameter(
              ref Printer printer
            , PubSubScopeSpec scope
        )
        {
            printer.Print("in ");
            WritePublisherType(ref printer, scope);
            printer.Print(" publisher");
        }

        private static void WriteSubscriberParameter(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool stateful
        )
        {
            printer.Print("in ");
            WriteSubscriberType(ref printer, scope, stateful);
            printer.Print(" subscriber");
        }

        private static void WriteFactoryParameter(ref Printer printer, PubSubScopeSpec scope)
        {
            printer.Print("[").Print(PubSubAliasSet.CODE_ANALYSIS).Print(".NotNull] ").Print(PubSubAliasSet.SYSTEM)
                .Print(".Func<").Print(scope.Declaration.FullTypeName).Print("> factory");
        }

        private static void WriteLoggerParameter(ref Printer printer)
        {
            printer.Print(PubSubAliasSet.LOGGING).Print(".ILogger logger = null");
        }

        private static void WriteParameterSeparator(ref Printer printer, ref bool hasParameter)
        {
            printer.PrintIf(hasParameter, ", ");
            hasParameter = true;
        }

        private static void WritePublisherType(
              ref Printer printer
            , PubSubScopeSpec scope
        )
        {
            printer.Print(PubSubAliasSet.PUBSUB).Print(".MessagePublisher.")
                .PrintIf(scope.IsUnityScope, "UnityPublisher<", "Publisher<");
            WriteScopeType(ref printer, scope);
            printer.Print(">");
        }

        private static void WriteSubscriberType(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool stateful
        )
        {
            printer.Print(PubSubAliasSet.PUBSUB).Print(".MessageSubscriber.")
                .PrintIf(scope.IsUnityScope, "UnitySubscriber<", "Subscriber<");
            WriteScopeType(ref printer, scope);
            printer.PrintIf(stateful, ", TState").Print(">");
        }

        private static void WriteCachedPublisherType(
              ref Printer printer
            , PubSubScopeSpec scope
        )
        {
            printer.Print(PubSubAliasSet.PUBSUB).Print(".CachedPublisher<");

            if (scope.IsUnityScope)
            {
                printer.Print(PubSubAliasSet.UNITY_EXTENSIONS).Print(".UnityEntityId<");
                WriteScopeType(ref printer, scope);
                printer.Print(">");
            }
            else
            {
                WriteScopeType(ref printer, scope);
            }

            printer.Print(", ").Print(scope.Declaration.FullTypeName).Print(">");
        }

        private static void WriteScopeType(
              ref Printer printer
            , PubSubScopeSpec scope
        )
            => printer.Print(scope.ScopeTypeName);

        private static void WriteHandlerType(
              ref Printer printer
            , PubSubScopeSpec scope
            , bool asynchronous
            , bool stateful
            , int handler
        )
        {
            printer.Print(PubSubAliasSet.SYSTEM);
            var argumentCount = GetHandlerArgumentCount(asynchronous, stateful, handler);

            if (asynchronous == false && argumentCount == 0)
            {
                printer.Print(".Action");
                return;
            }

            printer.PrintIf(asynchronous, ".Func<", ".Action<");
            var hasArgument = false;

            if (stateful)
            {
                WriteHandlerArgumentSeparator(ref printer, ref hasArgument);
                printer.Print("TState");
            }

            if (HandlerHasMessage(handler))
            {
                WriteHandlerArgumentSeparator(ref printer, ref hasArgument);
                printer.PrintIf(asynchronous, "Async", scope.Declaration.FullTypeName);
            }

            if (HandlerHasContext(handler))
            {
                WriteHandlerArgumentSeparator(ref printer, ref hasArgument);
                printer.Print(PubSubAliasSet.PUBSUB).Print(".PublishingContext");
            }

            if (asynchronous)
            {
                WriteHandlerArgumentSeparator(ref printer, ref hasArgument);
                printer.Print(PubSubAliasSet.TASKS).Print(".UnityTask");
            }

            printer.Print(">");
        }

        private static void WriteHandlerArgumentSeparator(ref Printer printer, ref bool hasArgument)
        {
            printer.PrintIf(hasArgument, ", ");
            hasArgument = true;
        }

        private static void WritePublishReturnType(
              ref Printer printer
            , bool asynchronous
        )
        {
            if (asynchronous)
            {
                printer.Print(PubSubAliasSet.TASKS).Print(".UnityTask");
            }
            else
            {
                printer.Print("void");
            }
        }

        private static void WriteGlobalPublisherIfNeeded(
              ref Printer printer
            , bool globalShortcut
        )
        {
            if (globalShortcut)
            {
                printer.PrintBeginLine("var publisher = ").Print(PubSubAliasSet.PUBSUB)
                    .PrintEndLine(".GlobalMessenger.Publisher.Global();");
            }
        }

        private static void WriteGlobalSubscriberIfNeeded(
              ref Printer printer
            , bool globalShortcut
        )
        {
            if (globalShortcut)
            {
                printer.PrintBeginLine("var subscriber = ").Print(PubSubAliasSet.PUBSUB)
                    .PrintEndLine(".GlobalMessenger.Subscriber.Global();");
            }
        }

        private static int GetHandlerArgumentCount(bool asynchronous, bool stateful, int handler)
            => (asynchronous ? 1 : 0) + (stateful ? 1 : 0) + (HandlerHasMessage(handler) ? 1 : 0)
                + (HandlerHasContext(handler) ? 1 : 0);

        private static bool HandlerHasMessage(int handler)
            => handler is 1 or 3;

        private static bool HandlerHasContext(int handler)
            => handler >= 2;
    }
}
