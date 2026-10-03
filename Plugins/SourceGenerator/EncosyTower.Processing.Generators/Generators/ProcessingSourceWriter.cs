namespace EncosyTower.Processing.Generators
{
    internal static class ProcessingSourceWriter
    {
        private const string AGGRESSIVE_INLINING = "[g__SR.MethodImpl(g__SR.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[" + ProcessingAliasSet.CODE_DOM_COMPILER + ".GeneratedCode(\""
            + ProcessingSourceGenContract.GENERATOR_METADATA_NAME + "\", \"" + SourceGenVersion.VALUE + "\")]";

        public static SourceText WriteRequest(in ProcessingRequestSpec spec, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var printer = new Printer(0, 1024 * 16, token);
            WriteRequestDeclaration(ref printer, spec);
            return CreateSource(spec.Declaration, printer.Result, token);
        }

        public static SourceText WriteScope(in ProcessingScopeSpec spec, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var printer = new Printer(0, 1024 * 16, token);
            WriteScopeDeclaration(ref printer, spec);
            return CreateSource(spec.Declaration, printer.Result, token);
        }

        private static SourceText CreateSource(
              ProcessingTypeDeclarationSpec declaration
            , string body
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            return TypeCreationHelpers.GenerateSourceText(
                  declaration.OpeningSource
                , body
                , declaration.ClosingSource
                , token
            );
        }

        private static void WriteRequestDeclaration(ref Printer printer, ProcessingRequestSpec spec)
        {
            WriteDeclarationHeader(ref printer, spec.Declaration, spec.GenerateRequestInterface);
            printer.OpenScope();
            {
                if (spec.WithAsync)
                {
                    WriteAsyncStorage(ref printer, spec);
                }
            }
            printer.CloseScope();
        }

        private static void WriteScopeDeclaration(ref Printer printer, ProcessingScopeSpec spec)
        {
            WriteDeclarationHeader(ref printer, spec.Declaration, false);
            printer.OpenScope();
            {
                if (spec.WithSync)
                {
                    WriteHubFamily(ref printer, spec, false);

                    if (spec.IsGlobalScope)
                    {
                        printer.PrintEndLine();
                        WriteGlobalFamily(ref printer, spec, false);
                    }
                }

                if (spec.WithAsync)
                {
                    printer.PrintEndLineIf(spec.WithSync, string.Empty);
                    printer.PrintLine("public readonly partial struct Async");
                    printer.OpenScope();
                    {
                        WriteHubFamily(ref printer, spec, true);

                        if (spec.IsGlobalScope)
                        {
                            printer.PrintEndLine();
                            WriteGlobalFamily(ref printer, spec, true);
                        }
                    }
                    printer.CloseScope();
                }
            }
            printer.CloseScope();
        }

        private static void WriteDeclarationHeader(
              ref Printer printer
            , ProcessingTypeDeclarationSpec declaration
            , bool generateRequestInterface
        )
        {
            printer.PrintBeginLine("partial ").Print(declaration.TypeKeyword).Print(" ")
                .Print(declaration.TypeName)
                .PrintEndLineIf(generateRequestInterface, " : g__ETP.IRequest", string.Empty);
        }

        private static void WriteAsyncStorage(ref Printer printer, ProcessingRequestSpec spec)
        {
            var fullTypeName = spec.Declaration.FullTypeName;
            var isReferenceType = spec.Declaration.IsReferenceType;
            printer.PrintLine(GENERATED_CODE);
            printer.PrintBeginLine("public readonly partial struct Async : g__ETP.IAsyncRequest")
                .PrintIf(spec.HasResult, "<", string.Empty)
                .PrintIf(spec.HasResult, spec.ResultTypeName, string.Empty)
                .PrintEndLineIf(spec.HasResult, ">", string.Empty);
            printer.OpenScope();
            {
                printer.PrintBeginLine("private readonly ").Print(fullTypeName)
                    .PrintIf(isReferenceType, "?").PrintEndLine(" _request;");
                printer.PrintEndLine();
                printer.PrintLine(AGGRESSIVE_INLINING);
                printer.PrintBeginLine("private Async(").Print(fullTypeName)
                    .PrintIf(isReferenceType, "?").PrintEndLine(" request)");
                printer.OpenScope();
                {
                    printer.PrintLine("_request = request;");
                }
                printer.CloseScope();
                printer.PrintEndLine();
                printer.PrintLine(AGGRESSIVE_INLINING);
                printer.PrintBeginLine("public static implicit operator Async(")
                    .Print(fullTypeName).PrintIf(isReferenceType, "?").PrintEndLine(" request)");
                printer.WithIncreasedIndent().PrintLine("=> new(request);");
                printer.PrintEndLine();
                printer.PrintLine(AGGRESSIVE_INLINING);
                printer.PrintBeginLine("public static implicit operator ").Print(fullTypeName)
                    .PrintIf(isReferenceType, "?").PrintEndLine("(Async request)");
                printer.WithIncreasedIndent().PrintLine("=> request._request;");
            }
            printer.CloseScope();
        }

        private static void WriteHubFamily(ref Printer printer, ProcessingScopeSpec spec, bool asynchronous)
        {
            if (spec.WithStateless)
            {
                WriteRegister(ref printer, spec, asynchronous, false, false);
                printer.PrintEndLine();
                WriteRegister(ref printer, spec, asynchronous, false, true);
                printer.PrintEndLine();
            }

            if (spec.WithStateful)
            {
                WriteRegister(ref printer, spec, asynchronous, true, false);
                printer.PrintEndLine();
                WriteRegister(ref printer, spec, asynchronous, true, true);
                printer.PrintEndLine();
                WriteRegisterWithState(ref printer, spec, asynchronous, false, true);
                printer.PrintEndLine();
                WriteRegisterWithState(ref printer, spec, asynchronous, true, true);
                printer.PrintEndLine();
            }

            WriteProcess(ref printer, spec, asynchronous, true);
            printer.PrintEndLine();
            WriteTryProcess(ref printer, spec, asynchronous, true);
        }

        private static void WriteGlobalFamily(ref Printer printer, ProcessingScopeSpec spec, bool asynchronous)
        {
            if (spec.WithStateless)
            {
                WriteRegister(ref printer, spec, asynchronous, false, false, true);
                printer.PrintEndLine();
                WriteRegister(ref printer, spec, asynchronous, false, true, true);
                printer.PrintEndLine();
            }

            if (spec.WithStateful)
            {
                WriteRegisterWithState(ref printer, spec, asynchronous, false, false);
                printer.PrintEndLine();
                WriteRegisterWithState(ref printer, spec, asynchronous, true, false);
                printer.PrintEndLine();
            }

            WriteProcess(ref printer, spec, asynchronous, false);
            printer.PrintEndLine();
            WriteTryProcess(ref printer, spec, asynchronous, false);
        }

        private static void WriteRegister(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , bool statefulHub
            , bool contextual
            , bool globalShortcut = false
        )
        {
            printer.PrintLine(AGGRESSIVE_INLINING);
            printer.PrintBeginLine("public static g__ETP.ProcessRegistry Register")
                .PrintIf(statefulHub, "<TState>", string.Empty).PrintEndLine("(");

            if (globalShortcut == false)
            {
                WriteHubParameter(ref printer, spec, statefulHub);
            }

            WriteHandlerParameter(
                  ref printer
                , spec
                , asynchronous
                , statefulHub
                , contextual
                , globalShortcut == false
            );
            printer.WithIncreasedIndent().PrintLine(", g__ETL.ILogger logger = null");
            printer.PrintLine(")");

            if (statefulHub)
            {
                printer.WithIncreasedIndent().PrintLine("where TState : class");
            }

            printer.OpenScope();
            {
                if (globalShortcut)
                {
                    printer.PrintLine("var hub = g__ETP.GlobalProcessor.Instance.Global();");
                }

                WriteRegisterDelegation(ref printer, spec, asynchronous, "hub");
            }
            printer.CloseScope();
        }

        private static void WriteRegisterWithState(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , bool contextual
            , bool hubBound
        )
        {
            printer.PrintLine(AGGRESSIVE_INLINING);
            printer.PrintLine("public static g__ETP.ProcessRegistry Register<TState>(");

            if (hubBound)
            {
                WriteHubParameter(ref printer, spec, false);
            }

            printer.WithIncreasedIndent()
                .PrintBeginLineIf(hubBound, ", ", string.Empty)
                .PrintEndLine("[g__SCA.NotNull] TState state");
            WriteHandlerParameter(ref printer, spec, asynchronous, true, contextual, true);
            printer.WithIncreasedIndent().PrintLine(", g__ETL.ILogger logger = null");
            printer.PrintLine(")");
            printer.WithIncreasedIndent().PrintLine("where TState : class");
            printer.OpenScope();
            {
                if (hubBound == false)
                {
                    printer.PrintLine("var hub = g__ETP.GlobalProcessor.Instance.Global();");
                }

                printer.PrintLine("var statefulHub = g__ETP.ProcessorExtensions.WithState(in hub, state);");
                WriteRegisterDelegation(ref printer, spec, asynchronous, "statefulHub");
            }
            printer.CloseScope();
        }

        private static void WriteHandlerParameter(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , bool withState
            , bool contextual
            , bool leadingComma
        )
        {
            var requestType = asynchronous ? "Async" : spec.Declaration.TypeName;
            var delegateType = asynchronous || spec.HasResult ? "g__S.Func" : "g__S.Action";
            var parameterPrinter = printer.WithIncreasedIndent();

            parameterPrinter.PrintBeginLineIf(leadingComma, ", ", string.Empty)
                .Print("[g__SCA.NotNull] ").Print(delegateType).Print("<")
                .PrintIf(withState, "TState, ")
                .Print(requestType)
                .PrintIf(contextual, ", g__ETP.ProcessingContext")
                .PrintIf(asynchronous, ", g__ETT.UnityTask")
                .PrintIf(asynchronous && spec.HasResult, "<")
                .PrintIf(asynchronous && spec.HasResult, spec.ResultTypeName)
                .PrintIf(asynchronous && spec.HasResult, ">")
                .PrintIf(asynchronous == false && spec.HasResult, ", ")
                .PrintIf(asynchronous == false && spec.HasResult, spec.ResultTypeName)
                .PrintEndLine("> process");
        }

        private static void WriteRegisterDelegation(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , string hubName
        )
        {
            printer.PrintBeginLine("return ").Print(hubName).Print(".Register<")
                .PrintIf(asynchronous, "Async", spec.Declaration.TypeName)
                .PrintIf(spec.HasResult, ", ", string.Empty)
                .PrintIf(spec.HasResult, spec.ResultTypeName, string.Empty)
                .PrintEndLine(">(process, logger);");
        }

        private static void WriteProcess(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , bool hubBound
        )
        {
            var requestType = asynchronous ? "Async" : spec.Declaration.TypeName;
            printer.PrintLine(AGGRESSIVE_INLINING);
            printer.PrintBeginLine("public static ");
            WriteProcessReturnType(ref printer, spec, asynchronous, false);
            printer.PrintEndLine(" Process(");

            if (hubBound)
            {
                WriteHubParameter(ref printer, spec, false);
            }

            printer.WithIncreasedIndent().PrintBeginLineIf(hubBound, ", ", string.Empty)
                .Print(requestType).PrintEndLine(" request");
            printer.WithIncreasedIndent().PrintLine(", g__ETP.ProcessingContext context = default");
            printer.PrintLine(")");
            printer.OpenScope();
            {
                if (hubBound == false)
                {
                    printer.PrintLine("var hub = g__ETP.GlobalProcessor.Instance.Global();");
                }

                WriteProcessDelegation(ref printer, spec, asynchronous, false, requestType);
            }
            printer.CloseScope();
        }

        private static void WriteTryProcess(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , bool hubBound
        )
        {
            var requestType = asynchronous ? "Async" : spec.Declaration.TypeName;
            printer.PrintLine(AGGRESSIVE_INLINING);
            printer.PrintBeginLine("public static ");
            WriteProcessReturnType(ref printer, spec, asynchronous, true);
            printer.PrintEndLine(" TryProcess(");

            if (hubBound)
            {
                WriteHubParameter(ref printer, spec, false);
            }

            printer.WithIncreasedIndent().PrintBeginLineIf(hubBound, ", ", string.Empty)
                .Print(requestType).PrintEndLine(" request");
            printer.WithIncreasedIndent().PrintLine(", g__ETP.ProcessingContext context = default");
            printer.PrintLine(")");
            printer.OpenScope();
            {
                if (hubBound == false)
                {
                    printer.PrintLine("var hub = g__ETP.GlobalProcessor.Instance.Global();");
                }

                WriteProcessDelegation(ref printer, spec, asynchronous, true, requestType);
            }
            printer.CloseScope();
        }

        private static void WriteProcessReturnType(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , bool tryProcess
        )
        {
            if (asynchronous)
            {
                printer.Print("g__ETT.UnityTask");

                if (tryProcess == false && spec.HasResult == false)
                {
                    return;
                }

                printer.Print("<");

                if (tryProcess && spec.HasResult)
                {
                    printer.Print("g__ETC.Option<").Print(spec.ResultTypeName).Print(">");
                }
                else if (tryProcess)
                {
                    printer.Print("bool");
                }
                else if (spec.HasResult)
                {
                    printer.Print(spec.ResultTypeName);
                }

                printer.Print(">");
            }
            else if (tryProcess && spec.HasResult)
            {
                printer.Print("g__ETC.Option<").Print(spec.ResultTypeName).Print(">");
            }
            else if (tryProcess)
            {
                printer.Print("bool");
            }
            else if (spec.HasResult)
            {
                printer.Print(spec.ResultTypeName);
            }
            else
            {
                printer.Print("void");
            }
        }

        private static void WriteProcessDelegation(
              ref Printer printer
            , ProcessingScopeSpec spec
            , bool asynchronous
            , bool tryProcess
            , string requestType
        )
        {
            var runtimeMethod = asynchronous
                ? tryProcess ? "TryProcessAsync" : "ProcessAsync"
                : tryProcess ? "TryProcess" : "Process";
            var withReturn = asynchronous || tryProcess || spec.HasResult;
            printer.PrintBeginLineIf(withReturn, "return ", string.Empty).Print("hub.")
                .Print(runtimeMethod).Print("<").Print(requestType)
                .PrintIf(spec.HasResult, ", ", string.Empty)
                .PrintIf(spec.HasResult, spec.ResultTypeName, string.Empty)
                .PrintEndLine(">(request, context);");
        }

        private static void WriteHubParameter(ref Printer printer, ProcessingScopeSpec spec, bool withState)
        {
            printer.WithIncreasedIndent().PrintBeginLine("  in g__ETP.Processor.")
                .PrintIf(spec.IsUnityScope, "UnityHub<", "Hub<")
                .Print(spec.ScopeTypeName)
                .PrintIf(withState, ", TState", string.Empty)
                .PrintEndLine("> hub");
        }
    }
}
