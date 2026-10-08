#if UNITY_EDITOR

using System.Linq;
using EncosyTower.CodeGen;
using EncosyTower.Core;

namespace EncosyTower.Editor.Tasks
{
    [CodeGenerator]
    [ApiForEditor]
    internal sealed class UnityTaskGenerator : ICodeGenerator
    {
        private const int MIN_ARITY = 2;
        private const int MAX_ARITY = 15;

        private static readonly string[] s_whenAllBehaviour = {
            "/// <para>",
            "/// <b>Behaviour:</b> completes when every input succeeds, with each result at its argument",
            "/// position. The first observed fault or cancellation completes the task immediately and is",
            "/// rethrown as the same instance. Later inputs are still observed and their faults are discarded.",
            "/// </para>",
            "/// <para>",
            "/// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAll</c>; Unity: none.",
            "/// </para>",
        };

        private static readonly string[] s_whenAnyBehaviour = {
            "/// <para>",
            "/// <b>Behaviour:</b> the first observed completion wins; a winner's fault or cancellation is",
            "/// rethrown as the same instance. Losers are observed and not cancelled, and their faults are",
            "/// discarded. When several inputs are already complete when observed on the calling thread",
            "/// kind, the lowest argument index wins.",
            "/// </para>",
            "/// <para>",
            "/// <b>Counterparts:</b> UniTask: <c>UniTask.WhenAny</c>; Unity: none.",
            "/// </para>",
        };

        private static readonly string[] s_commonRemarks = {
            "/// <para>",
            "/// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main",
            "/// thread when called on the main thread, a thread-pool thread otherwise.",
            "/// </para>",
            "/// <para><b>Undefined behaviour:</b></para>",
            "/// <list type=\"bullet\">",
            "/// <item><description>",
            "/// Awaiting the task or a copy more than once, calling <c>GetResult</c> twice, or passing the",
            "/// same task (or a copy) more than once: may throw <see cref=\"InvalidOperationException\"/>,",
            "/// return a stale result, never complete, or observe another operation's result because sources",
            "/// are pooled.",
            "/// </description></item>",
            "/// <item><description>",
            "/// Calling <c>GetAwaiter().GetResult()</c> before completion: may throw or block indefinitely.",
            "/// </description></item>",
            "/// <item><description>",
            "/// Calling Unity APIs after resuming on a thread-pool thread (task created off the main thread):",
            "/// Unity throws or corrupts state, per Unity rules.",
            "/// </description></item>",
            "/// <item><description>",
            "/// An input obtained from <c>UniTask</c> or <c>Awaitable</c> through an interop conversion: that",
            "/// input follows native-library behaviour.",
            "/// </description></item>",
            "/// </list>",
            "/// </remarks>",
        };

        private static readonly string[] s_whenAllStateRemarks = {
            "/// <remarks>",
            "/// <para>",
            "/// <b>Protocol:</b> one <c>PooledUnityTaskObserver</c> per input calls <c>Complete</c> with that",
            "/// input's outcome and then <c>Detach</c>. <c>_remaining</c> counts the inputs not yet detached.",
            "/// </para>",
            "/// <para>",
            "/// <b>Results:</b> a successful input stores its value in its own <c>_result</c> field. The last",
            "/// detach completes the source with a tuple of those fields.",
            "/// </para>",
            "/// <para>",
            "/// <b>Outcome:</b> <c>_signaled</c> is set once with <c>Interlocked.CompareExchange</c>. The first",
            "/// fault wins and faults the source. If no input faults, the last detach completes the source",
            "/// successfully.",
            "/// </para>",
            "/// <para>",
            "/// <b>Pooling:</b> <c>_releases</c> counts two owners: the last detaching observer and",
            "/// <c>WaitAsync</c>. On the second release the instance clears its result fields and returns to",
            "/// <c>s_pool</c>. The pool holds at most <c>MAX_POOL_SIZE</c> instances and is guarded by a lock",
            "/// on <c>s_pool</c>.",
            "/// </para>",
            "/// </remarks>",
        };

        private static readonly string[] s_whenAnyStateRemarks = {
            "/// <remarks>",
            "/// <para>",
            "/// <b>Protocol:</b> one <c>PooledUnityTaskObserver</c> per input calls <c>Complete</c> with that",
            "/// input's outcome and then <c>Detach</c>. <c>_remaining</c> counts the inputs not yet detached.",
            "/// </para>",
            "/// <para>",
            "/// <b>Outcome:</b> <c>_won</c> is set once with <c>Interlocked.CompareExchange</c>. The first input",
            "/// to complete wins. The winner stores its value in its own <c>_result</c> field and completes the",
            "/// source with its zero-based index and a tuple of those fields, or faults the source if it",
            "/// faulted or was canceled. Later completions return without effect.",
            "/// </para>",
            "/// <para>",
            "/// <b>Pooling:</b> <c>_releases</c> counts two owners: the last detaching observer and",
            "/// <c>WaitAsync</c>. On the second release the instance clears its result fields and returns to",
            "/// <c>s_pool</c>. The pool holds at most <c>MAX_POOL_SIZE</c> instances and is guarded by a lock",
            "/// on <c>s_pool</c>.",
            "/// </para>",
            "/// </remarks>",
        };

        public GeneratedCode[] Generate()
        {
            var p = Printer.DefaultLarge;
            PrintWhenAll(ref p);
            var whenAllContent = p.Result;

            p = Printer.DefaultLarge;
            PrintWhenAny(ref p);
            var whenAnyContent = p.Result;

            p = Printer.DefaultLarge;
            PrintShorthand(ref p);

            return new GeneratedCode[] {
                CodeGenAPI.GetGeneratedCode(whenAllContent, "UnityTask+WhenAll.gen"),
                CodeGenAPI.GetGeneratedCode(whenAnyContent, "UnityTask+WhenAny.gen"),
                CodeGenAPI.GetGeneratedCode(p.Result, "UnityTaskExtensions+Shorthand.gen"),
            };
        }

        private static void PrintWhenAll(ref Printer p)
        {
            PrintHeader(ref p);
            p.PrintLine("namespace EncosyTower.Tasks");
            p.OpenScope();
            {
                p.PrintLine("public readonly partial struct UnityTask");
                p.OpenScope();
                {
                    for (var arity = MIN_ARITY; arity <= MAX_ARITY; arity++)
                    {
                        PrintWhenAll(ref p, arity);
                    }

                    for (var arity = MIN_ARITY; arity <= MAX_ARITY; arity++)
                    {
                        PrintWhenAllState(ref p, arity);
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
        }

        private static void PrintWhenAll(ref Printer p, int arity)
        {
            var typeParameters = GetTypeParameters(arity);
            var stateType = $"FixedWhenAllState<{typeParameters}>";

            PrintWhenAllDoc(ref p, arity);
            p.PrintBeginLine("public static UnityTask<(")
                .Print(typeParameters)
                .Print(")> WhenAll<")
                .Print(typeParameters)
                .PrintEndLine(">(");

            PrintParameters(ref p, arity);
            p.PrintLine(")");
            PrintMethodBody(ref p, arity, stateType);
        }

        private static void PrintWhenAllState(ref Printer p, int arity)
        {
            var stateType = $"FixedWhenAllState<{GetTypeParameters(arity)}>";

            PrintWhenAllStateDoc(ref p, arity);
            PrintStateDeclaration(ref p, arity, stateType);
            p.OpenScope();
            {
                PrintStateFields(p: ref p, arity: arity, stateType: stateType, includeWinner: false);
                PrintConstructor(ref p, "FixedWhenAllState");
                PrintRent(p: ref p, arity: arity, stateType: stateType, includeWinner: false);

                PrintWaitAsyncDoc(ref p);
                p.PrintBeginLine("internal async UnityTask<(")
                    .Print(GetTypeParameters(arity))
                    .PrintEndLine(")> WaitAsync()");

                PrintWaitAsyncBody(ref p);

                for (var i = 1; i <= arity; i++)
                {
                    PrintWhenAllCompletion(ref p, i);
                }

                PrintSignalFailure(ref p);
                PrintWhenAllDetach(ref p, arity);
                PrintRelease(ref p, arity);
            }
            p.CloseScope();
            PrintMemberSeparator(ref p, arity);
        }

        private static void PrintWhenAllCompletion(ref Printer p, int index)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine($"/// Stores the result of the task at argument position {index}, or signals its fault.");
            p.PrintLine("/// </summary>");
            PrintCompletionSignature(ref p, index);
            p.OpenScope();
            {
                p.PrintLine("if (exception == null)");
                p.OpenScope();
                {
                    p.PrintLine($"_result{index} = result;");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintLine("SignalFailure(exception);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            PrintDetachForwarder(ref p, index);
        }

        private static void PrintSignalFailure(ref Printer p)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine("/// Faults the source with <paramref name=\"exception\"/> if no other input has");
            p.PrintLine("/// signaled yet.");
            p.PrintLine("/// </summary>");
            p.PrintLine("private void SignalFailure(Exception exception)");
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.CompareExchange(location1: ref _signaled, value: 1, comparand: 0) == 0)");
                p.OpenScope();
                {
                    p.PrintLine("_source.TrySetException(exception);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWhenAllDetach(ref Printer p, int arity)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine("/// Counts one input as detached. The last detach completes the source successfully unless a");
            p.PrintLine("/// fault signaled first, then releases the observers' ownership.");
            p.PrintLine("/// </summary>");
            p.PrintLine("private void Detach()");
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.Decrement(ref _remaining) != 0)");
                p.OpenScope();
                {
                    p.PrintLine("return;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("if (Interlocked.CompareExchange(location1: ref _signaled, value: 1, comparand: 0) == 0)");
                p.OpenScope();
                {
                    p.PrintBeginLine("_source.TrySetResult((").Print(GetResultFields(arity)).PrintEndLine("));");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("Release();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWhenAny(ref Printer p)
        {
            PrintHeader(ref p);
            p.PrintLine("namespace EncosyTower.Tasks");
            p.OpenScope();
            {
                p.PrintLine("public readonly partial struct UnityTask");
                p.OpenScope();
                {
                    for (var arity = MIN_ARITY; arity <= MAX_ARITY; arity++)
                    {
                        PrintWhenAny(ref p, arity);
                    }

                    for (var arity = MIN_ARITY; arity <= MAX_ARITY; arity++)
                    {
                        PrintWhenAnyState(ref p, arity);
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
        }

        private static void PrintWhenAny(ref Printer p, int arity)
        {
            var typeParameters = GetTypeParameters(arity);
            var stateType = $"FixedWhenAnyState<{typeParameters}>";

            PrintWhenAnyDoc(ref p, arity);
            p.PrintBeginLine("public static UnityTask<(int winArgumentIndex, ")
                .Print(GetNamedResults(arity))
                .Print(")> WhenAny<")
                .Print(typeParameters)
                .PrintEndLine(">(");

            PrintParameters(ref p, arity);
            p.PrintLine(")");
            PrintMethodBody(ref p, arity, stateType);
        }

        private static void PrintWhenAnyState(ref Printer p, int arity)
        {
            var stateType = $"FixedWhenAnyState<{GetTypeParameters(arity)}>";

            PrintWhenAnyStateDoc(ref p, arity);
            PrintStateDeclaration(ref p, arity, stateType);
            p.OpenScope();
            {
                PrintStateFields(p: ref p, arity: arity, stateType: stateType, includeWinner: true);
                PrintConstructor(ref p, "FixedWhenAnyState");
                PrintRent(p: ref p, arity: arity, stateType: stateType, includeWinner: true);

                PrintWaitAsyncDoc(ref p);
                p.PrintBeginLine("internal async UnityTask<(int winArgumentIndex, ")
                    .Print(GetNamedResults(arity))
                    .PrintEndLine(")> WaitAsync()");

                PrintWaitAsyncBody(ref p);

                for (var i = 1; i <= arity; i++)
                {
                    PrintWhenAnyCompletion(ref p, arity, i);
                }

                PrintWhenAnyDetach(ref p);
                PrintRelease(ref p, arity);
            }
            p.CloseScope();
            PrintMemberSeparator(ref p, arity);
        }

        private static void PrintWhenAnyCompletion(ref Printer p, int arity, int index)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine($"/// Completes the source with the outcome of the task at argument position {index}");
            p.PrintLine("/// if it is the first to complete.");
            p.PrintLine("/// </summary>");
            PrintCompletionSignature(ref p, index);
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.CompareExchange(location1: ref _won, value: 1, comparand: 0) != 0)");
                p.OpenScope();
                {
                    p.PrintLine("return;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("if (exception == null)");
                p.OpenScope();
                {
                    p.PrintLine($"_result{index} = result;");
                    p.PrintBeginLine("_source.TrySetResult((")
                        .Print($"{index - 1}")
                        .Print(", ")
                        .Print(GetResultFields(arity))
                        .PrintEndLine("));");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintLine("_source.TrySetException(exception);");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();

            PrintDetachForwarder(ref p, index);
        }

        private static void PrintWhenAnyDetach(ref Printer p)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine("/// Counts one input as detached and releases the observers' ownership when none remain.");
            p.PrintLine("/// </summary>");
            p.PrintLine("private void Detach()");
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.Decrement(ref _remaining) == 0)");
                p.OpenScope();
                {
                    p.PrintLine("Release();");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintShorthand(ref Printer p)
        {
            p.PrintAutoGeneratedBlock(nameof(UnityTaskGenerator));
            p.PrintEndLine();
            p.PrintLine("using System.Collections.Generic;");
            p.PrintEndLine();
            p.PrintLine("namespace EncosyTower.Tasks");
            p.OpenScope();
            {
                p.PrintLine("public static partial class UnityTaskExtensions");
                p.OpenScope();
                {
                    PrintCollectionShorthands(ref p);

                    for (var arity = MIN_ARITY; arity <= MAX_ARITY; arity++)
                    {
                        PrintTupleShorthand(ref p, arity);
                        p.PrintEndLine();
                        PrintGenericTupleShorthand(ref p, arity);
                        PrintMemberSeparator(ref p, arity);
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
        }

        private static void PrintCollectionShorthands(ref Printer p)
        {
            PrintCollectionShorthand(
                  p: ref p
                , returnType: "UnityTask.Awaiter"
                , typeParameters: ""
                , receiver: "UnityTask[]"
            );

            PrintCollectionShorthand(
                  p: ref p
                , returnType: "UnityTask.Awaiter"
                , typeParameters: ""
                , receiver: "IEnumerable<UnityTask>"
            );

            PrintCollectionShorthand(
                  p: ref p
                , returnType: "UnityTask<T[]>.Awaiter"
                , typeParameters: "<T>"
                , receiver: "UnityTask<T>[]"
            );

            PrintCollectionShorthand(
                  p: ref p
                , returnType: "UnityTask<T[]>.Awaiter"
                , typeParameters: "<T>"
                , receiver: "IEnumerable<UnityTask<T>>"
            );
        }

        private static void PrintCollectionShorthand(
              ref Printer p
            , string returnType
            , string typeParameters
            , string receiver
        )
        {
            p.PrintLine("/// <summary>");
            p.PrintBeginLine("/// Awaits every task in <paramref name=\"tasks\"/>, ")
                .PrintEndLine("as <c>UnityTask.WhenAll(tasks)</c> does.");
            p.PrintLine("/// </summary>");

            if (typeParameters.Length > 0)
            {
                p.PrintLine("/// <typeparam name=\"T\">The result type of the tasks.</typeparam>");
            }

            p.PrintLine("/// <param name=\"tasks\">The tasks to await.</param>");
            p.PrintLine("/// <returns>An awaiter for the combined task.</returns>");
            PrintShorthandRemarks(ref p);
            p.PrintBeginLine("public static ")
                .Print(returnType)
                .Print(" GetAwaiter")
                .Print(typeParameters)
                .Print("(this ")
                .Print(receiver)
                .PrintEndLine(" tasks)");

            p = p.IncreasedIndent();
            p.PrintLine("=> UnityTask.WhenAll(tasks).GetAwaiter();");
            p = p.DecreasedIndent();
            p.PrintEndLine();
        }

        private static void PrintTupleShorthand(ref Printer p, int arity)
        {
            p.PrintLine("/// <summary>");
            p.PrintBeginLine($"/// Awaits the {arity} tasks of <paramref name=\"tasks\"/>, ")
                .PrintEndLine("as <c>UnityTask.WhenAll</c> does.");
            p.PrintLine("/// </summary>");
            p.PrintLine("/// <param name=\"tasks\">The tasks to await.</param>");
            p.PrintLine("/// <returns>An awaiter for the combined task.</returns>");
            PrintShorthandRemarks(ref p);
            p.PrintBeginLine("public static UnityTask.Awaiter GetAwaiter(this (")
                .Print(GetTupleElements(arity, generic: false))
                .PrintEndLine(") tasks)");

            p = p.IncreasedIndent();
            p.PrintBeginLine("=> UnityTask.WhenAll(").Print(GetTupleItems(arity)).PrintEndLine(").GetAwaiter();");
            p = p.DecreasedIndent();
        }

        private static void PrintGenericTupleShorthand(ref Printer p, int arity)
        {
            var typeParameters = GetTypeParameters(arity);

            p.PrintLine("/// <summary>");
            p.PrintBeginLine($"/// Awaits the {arity} tasks of <paramref name=\"tasks\"/>, ")
                .PrintEndLine("as <c>UnityTask.WhenAll</c> does.");
            p.PrintLine("/// </summary>");

            for (var i = 1; i <= arity; i++)
            {
                p.PrintLine($"/// <typeparam name=\"T{i}\">The result type of task {i}.</typeparam>");
            }

            p.PrintLine("/// <param name=\"tasks\">The tasks to await.</param>");
            p.PrintLine("/// <returns>An awaiter whose result holds each result at its tuple position.</returns>");
            PrintShorthandRemarks(ref p);
            p.PrintBeginLine("public static UnityTask<(")
                .Print(typeParameters)
                .Print(")>.Awaiter GetAwaiter<")
                .Print(typeParameters)
                .Print(">(this (")
                .Print(GetTupleElements(arity, generic: true))
                .PrintEndLine(") tasks)");

            p = p.IncreasedIndent();
            p.PrintBeginLine("=> UnityTask.WhenAll(").Print(GetTupleItems(arity)).PrintEndLine(").GetAwaiter();");
            p = p.DecreasedIndent();
        }

        private static void PrintShorthandRemarks(ref Printer p)
        {
            p.PrintLine("/// <remarks>");
            p.PrintLine("/// <para>");
            p.PrintBeginLine("/// <b>Counterparts:</b> UniTask: <c>UniTaskExtensions.GetAwaiter</c>; ")
                .PrintEndLine("Unity: none.");
            p.PrintLine("/// </para>");
            p.PrintLine("/// </remarks>");
        }

        private static string GetTupleElements(int arity, bool generic)
            => generic
                ? string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"UnityTask<T{i}> task{i}"))
                : string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"UnityTask task{i}"));

        private static string GetTupleItems(int arity)
            => string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"tasks.task{i}"));

        private static void PrintHeader(ref Printer p)
        {
            p.PrintAutoGeneratedBlock(nameof(UnityTaskGenerator));
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.Collections.Generic;");
            p.PrintLine("using System.Threading;");
            p.PrintEndLine();
        }

        private static void PrintWhenAllDoc(ref Printer p, int arity)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine($"/// Creates a task that completes with the results of all {arity} tasks.");
            p.PrintLine("/// </summary>");
            PrintTypeParameterDocs(ref p, arity);
            PrintParameterDocs(ref p, arity);
            p.PrintLine("/// <returns>A task whose result holds each input result at its argument position.</returns>");
            p.PrintLine("/// <remarks>");
            PrintLines(ref p, s_whenAllBehaviour);
            PrintLines(ref p, s_commonRemarks);
        }

        private static void PrintWhenAnyDoc(ref Printer p, int arity)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine($"/// Creates a task that completes when any of the {arity} tasks completes.");
            p.PrintLine("/// </summary>");
            PrintTypeParameterDocs(ref p, arity);
            PrintParameterDocs(ref p, arity);
            p.PrintLine("/// <returns>");
            p.PrintLine("/// A task whose result holds the zero-based argument index of the winner and, at the");
            p.PrintLine("/// winner's position, its result; the other result positions hold <c>default</c>.");
            p.PrintLine("/// </returns>");
            p.PrintLine("/// <remarks>");
            PrintLines(ref p, s_whenAnyBehaviour);
            PrintLines(ref p, s_commonRemarks);
        }

        private static void PrintWhenAllStateDoc(ref Printer p, int arity)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine($"/// The pooled state behind the {arity}-argument <c>WhenAll</c> overload.");
            p.PrintLine("/// </summary>");
            PrintStateTypeParameterDocs(ref p, arity);
            PrintLines(ref p, s_whenAllStateRemarks);
        }

        private static void PrintWhenAnyStateDoc(ref Printer p, int arity)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine($"/// The pooled state behind the {arity}-argument <c>WhenAny</c> overload.");
            p.PrintLine("/// </summary>");
            PrintStateTypeParameterDocs(ref p, arity);
            PrintLines(ref p, s_whenAnyStateRemarks);
        }

        private static void PrintStateTypeParameterDocs(ref Printer p, int arity)
        {
            for (var i = 1; i <= arity; i++)
            {
                p.PrintBeginLine($"/// <typeparam name=\"T{i}\">")
                    .PrintEndLine($"The result type of the task at argument position {i}.</typeparam>");
            }
        }

        private static void PrintWaitAsyncDoc(ref Printer p)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine("/// Awaits the source and releases the <c>WaitAsync</c> ownership when the await ends,");
            p.PrintLine("/// including on a fault.");
            p.PrintLine("/// </summary>");
        }

        private static void PrintLines(ref Printer p, string[] lines)
        {
            var count = lines.Length;

            for (var i = 0; i < count; i++)
            {
                p.PrintLine(lines[i]);
            }
        }

        private static void PrintTypeParameterDocs(ref Printer p, int arity)
        {
            for (var i = 1; i <= arity; i++)
            {
                p.PrintBeginLine($"/// <typeparam name=\"T{i}\">")
                    .PrintEndLine($"The result type of <paramref name=\"task{i}\"/>.</typeparam>");
            }
        }

        private static void PrintParameterDocs(ref Printer p, int arity)
        {
            for (var i = 1; i <= arity; i++)
            {
                p.PrintLine($"/// <param name=\"task{i}\">The task at argument position {i}.</param>");
            }
        }

        private static void PrintParameters(ref Printer p, int arity)
        {
            p = p.IncreasedIndent();

            for (var i = 1; i <= arity; i++)
            {
                var prefix = i == 1 ? "  " : ", ";
                p.PrintLine($"{prefix}UnityTask<T{i}> task{i}");
            }

            p = p.DecreasedIndent();
        }

        private static void PrintMethodBody(ref Printer p, int arity, string stateType)
        {
            p.OpenScope();
            {
                p.PrintBeginLine("var state = ").Print(stateType).PrintEndLine(".Rent();");
                p.PrintEndLine();

                for (var i = 1; i <= arity; i++)
                {
                    p.PrintBeginLine("PooledUnityTaskObserver<T")
                        .Print($"{i}")
                        .Print(", UnityTaskPosition")
                        .Print($"{i}")
                        .Print(", ")
                        .Print(stateType)
                        .Print(">.Observe(task")
                        .Print($"{i}")
                        .PrintEndLine(", state);");
                }

                p.PrintEndLine();
                p.PrintLine("return state.WaitAsync();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintStateDeclaration(ref Printer p, int arity, string stateType)
        {
            p.PrintBeginLine("private sealed class ").PrintEndLine(stateType);
            p = p.IncreasedIndent();

            for (var i = 1; i <= arity; i++)
            {
                var prefix = i == 1 ? ": " : ", ";
                p.PrintLine($"{prefix}IUnityTaskResultSink<T{i}, UnityTaskPosition{i}>");
            }

            p = p.DecreasedIndent();
        }

        private static void PrintStateFields(ref Printer p, int arity, string stateType, bool includeWinner)
        {
            var tupleTypes = GetTypeParameters(arity);

            if (includeWinner)
            {
                tupleTypes = $"int, {tupleTypes}";
            }

            p.PrintLine("private const int MAX_POOL_SIZE = 256;");
            p.PrintEndLine();
            p.PrintBeginLine("private static readonly Stack<").Print(stateType).PrintEndLine("> s_pool = new();");
            p.PrintEndLine();
            p.PrintBeginLine("private UnityTaskCompletionSource<(").Print(tupleTypes).PrintEndLine(")> _source;");

            for (var i = 1; i <= arity; i++)
            {
                p.PrintLine($"private T{i} _result{i};");
            }

            p.PrintLine("private int _remaining;");
            p.PrintLine(includeWinner ? "private int _won;" : "private int _signaled;");
            p.PrintLine("private int _releases;");
            p.PrintEndLine();
        }

        private static void PrintConstructor(ref Printer p, string typeName)
        {
            p.PrintBeginLine("private ").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintRent(ref Printer p, int arity, string stateType, bool includeWinner)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine("/// Takes an instance from the pool or creates one, resets its source and result fields, and");
            p.PrintLine($"/// sets <c>_remaining</c> to {arity}.");
            p.PrintLine("/// </summary>");
            p.PrintBeginLine("internal static ").Print(stateType).PrintEndLine(" Rent()");
            p.OpenScope();
            {
                p.PrintBeginLine(stateType).PrintEndLine(" state;");
                p.PrintEndLine();

                p.PrintLine("lock (s_pool)");
                p.OpenScope();
                {
                    p.PrintLine("state = s_pool.Count > 0 ? s_pool.Pop() : new();");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("if (state._source == null)");
                p.OpenScope();
                {
                    p.PrintLine("state._source = new();");
                }
                p.CloseScope();
                p.PrintLine("else");
                p.OpenScope();
                {
                    p.PrintLine("state._source.Reset();");
                }
                p.CloseScope();
                p.PrintEndLine();

                for (var i = 1; i <= arity; i++)
                {
                    p.PrintLine($"state._result{i} = default;");
                }

                p.PrintLine($"state._remaining = {arity};");
                p.PrintLine(includeWinner ? "state._won = 0;" : "state._signaled = 0;");
                p.PrintLine("state._releases = 0;");
                p.PrintLine("return state;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWaitAsyncBody(ref Printer p)
        {
            p.OpenScope();
            {
                p.PrintLine("try");
                p.OpenScope();
                {
                    p.PrintLine("return await _source.Task;");
                }
                p.CloseScope();
                p.PrintLine("finally");
                p.OpenScope();
                {
                    p.PrintLine("Release();");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintCompletionSignature(ref Printer p, int index)
        {
            p.PrintBeginLine("void IUnityTaskResultSink<T")
                .Print($"{index}")
                .Print(", UnityTaskPosition")
                .Print($"{index}")
                .PrintEndLine(">.Complete(");

            p = p.IncreasedIndent();
            {
                p.PrintLine($"  UnityTaskPosition{index} position");
                p.PrintLine($", T{index} result");
                p.PrintLine(", Exception exception");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
        }

        private static void PrintDetachForwarder(ref Printer p, int index)
        {
            p.PrintBeginLine("void IUnityTaskResultSink<T")
                .Print($"{index}")
                .Print(", UnityTaskPosition")
                .Print($"{index}")
                .PrintEndLine(">.Detach()");
            p.WithIncreasedIndent().PrintLine("=> Detach();");
            p.PrintEndLine();
        }

        private static void PrintRelease(ref Printer p, int arity)
        {
            p.PrintLine("/// <summary>");
            p.PrintLine("/// Gives up one of the two ownerships. On the second call it clears the result fields and");
            p.PrintLine("/// returns this instance to the pool.");
            p.PrintLine("/// </summary>");
            p.PrintLine("private void Release()");
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.Increment(ref _releases) != 2)");
                p.OpenScope();
                {
                    p.PrintLine("return;");
                }
                p.CloseScope();
                p.PrintEndLine();

                for (var i = 1; i <= arity; i++)
                {
                    p.PrintLine($"_result{i} = default;");
                }

                p.PrintEndLine();
                p.PrintLine("lock (s_pool)");
                p.OpenScope();
                {
                    p.PrintLine("if (s_pool.Count < MAX_POOL_SIZE)");
                    p.OpenScope();
                    {
                        p.PrintLine("s_pool.Push(this);");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
        }

        private static void PrintMemberSeparator(ref Printer p, int arity)
        {
            if (arity < MAX_ARITY)
            {
                p.PrintEndLine();
            }
        }

        private static string GetTypeParameters(int arity)
            => string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"T{i}"));

        private static string GetNamedResults(int arity)
            => string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"T{i} result{i}"));

        private static string GetResultFields(int arity)
            => string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"_result{i}"));
    }
}

#endif
