#if UNITY_EDITOR

using System.Linq;
using EncosyTower.CodeGen;
using EncosyTower.Core;

namespace EncosyTower.Editor.Tasks
{
    [CodeGenerator]
    [ApiForEditor]
    internal sealed class AwaitablesGenerator : ICodeGenerator
    {
        private const int MIN_ARITY = 2;
        private const int MAX_ARITY = 15;

        public GeneratedCode[] Generate()
        {
            var p = Printer.DefaultLarge;
            PrintWhenAll(ref p);
            var whenAllContent = p.Result;

            p = Printer.DefaultLarge;
            PrintWhenAny(ref p);

            return new GeneratedCode[] {
                CodeGenAPI.GetGeneratedCode(whenAllContent, "Awaitables.WhenAll.gen"),
                CodeGenAPI.GetGeneratedCode(p.Result, "Awaitables.WhenAny.gen"),
            };
        }

        private static void PrintWhenAll(ref Printer p)
        {
            PrintHeader(ref p);
            p.PrintLine("namespace UnityEngine.Tasks");
            p.OpenScope();
            {
                p.PrintLine("public static partial class Awaitables");
                p.OpenScope();
                {
                    for (var arity = MIN_ARITY; arity <= MAX_ARITY; arity++)
                    {
                        PrintWhenAll(ref p, arity);
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWhenAll(ref Printer p, int arity)
        {
            var typeParameters = GetTypeParameters(arity);
            var stateType = $"FixedWhenAllState<{typeParameters}>";

            p.PrintLine("public static Awaitable<");
            PrintTupleType(ref p, arity, includeWinner: false);
            p.PrintBeginLine("> WhenAll<").Print(typeParameters).PrintEndLine(">(");
            PrintParameters(ref p, arity, "Awaitable", "awaitable");
            p.PrintLine(")");
            p.OpenScope();
            {
                for (var i = 1; i <= arity; i++)
                {
                    p.PrintBeginLine("global::EncosyTower.Debugging.ThrowHelper.ThrowIfNull(awaitable")
                        .Print($"{i}").PrintEndLine(");");
                }

                p.PrintBeginLine("var state = ").Print(stateType).PrintEndLine(".Rent();");

                for (var i = 1; i <= arity; i++)
                {
                    p.PrintBeginLine("PooledAwaitableObserver<T")
                        .Print($"{i}")
                        .Print(", AwaitablePosition")
                        .Print($"{i}")
                        .Print(", ")
                        .Print(stateType)
                        .Print(">.Observe(awaitable")
                        .Print($"{i}")
                        .PrintEndLine(", state);");
                }

                p.PrintLine("return state.WaitAsync();");
            }
            p.CloseScope();
            p.PrintEndLine();

            PrintWhenAllState(ref p, arity, stateType);
        }

        private static void PrintWhenAllState(ref Printer p, int arity, string stateType)
        {
            PrintStateDeclaration(ref p, arity, stateType);
            p.OpenScope();
            {
                PrintStateFields(ref p, arity, stateType, includeWinner: false);
                p.PrintLine("private FixedWhenAllState() { }");
                p.PrintEndLine();

                PrintRent(ref p, arity, stateType, includeWinner: false);

                p.PrintBeginLine("internal async Awaitable<(")
                    .Print(GetTypeParameters(arity))
                    .PrintEndLine(")> WaitAsync()");
                PrintWaitAsyncBody(ref p);

                for (var i = 1; i <= arity; i++)
                {
                    PrintWhenAllCompletion(ref p, i);
                }

                PrintSignalFailure(ref p);
                PrintWhenAllDetach(ref p, arity);
                PrintTryRecycle(ref p, arity);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWhenAllCompletion(ref Printer p, int index)
        {
            p.PrintBeginLine("void IAwaitableResultSink<T")
                .Print($"{index}")
                .Print(", AwaitablePosition")
                .Print($"{index}")
                .PrintEndLine(">.Complete(");

            p = p.IncreasedIndent();
            {
                p.PrintLine($"  AwaitablePosition{index} position");
                p.PrintLine($", T{index} result");
                p.PrintLine(", Exception exception");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
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

            p.PrintBeginLine("void IAwaitableResultSink<T")
                .Print($"{index}")
                .Print(", AwaitablePosition")
                .Print($"{index}")
                .PrintEndLine(">.Detach()");
            p.WithIncreasedIndent().PrintLine("=> Detach();");
            p.PrintEndLine();
        }

        private static void PrintSignalFailure(ref Printer p)
        {
            p.PrintLine("private void SignalFailure(Exception exception)");
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)");
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

                p.PrintLine("if (Interlocked.CompareExchange(ref _signaled, 1, 0) == 0)");
                p.OpenScope();
                {
                    p.PrintBeginLine("_source.TrySetResult((").Print(GetResultFields(arity)).PrintEndLine("));");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("Volatile.Write(ref _detached, 1);");
                p.PrintLine("TryRecycle();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWhenAny(ref Printer p)
        {
            PrintHeader(ref p);
            p.PrintLine("namespace UnityEngine.Tasks");
            p.OpenScope();
            {
                p.PrintLine("public static partial class Awaitables");
                p.OpenScope();
                {
                    for (var arity = MIN_ARITY; arity <= MAX_ARITY; arity++)
                    {
                        PrintWhenAny(ref p, arity);
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWhenAny(ref Printer p, int arity)
        {
            var typeParameters = GetTypeParameters(arity);
            var stateType = $"FixedWhenAnyState<{typeParameters}>";

            p.PrintLine("public static Awaitable<");
            PrintTupleType(ref p, arity, includeWinner: true);
            p.PrintBeginLine("> WhenAny<").Print(typeParameters).PrintEndLine(">(");
            PrintParameters(ref p, arity, "Awaitable", "task");
            p.PrintLine(")");
            p.OpenScope();
            {
                for (var i = 1; i <= arity; i++)
                {
                    p.PrintBeginLine("global::EncosyTower.Debugging.ThrowHelper.ThrowIfNull(task")
                        .Print($"{i}").PrintEndLine(");");
                }

                p.PrintBeginLine("var state = ").Print(stateType).PrintEndLine(".Rent();");

                for (var i = 1; i <= arity; i++)
                {
                    p.PrintBeginLine("PooledAwaitableObserver<T")
                        .Print($"{i}")
                        .Print(", AwaitablePosition")
                        .Print($"{i}")
                        .Print(", ")
                        .Print(stateType)
                        .Print(">.Observe(task")
                        .Print($"{i}")
                        .PrintEndLine(", state);");
                }

                p.PrintLine("return state.WaitAsync();");
            }
            p.CloseScope();
            p.PrintEndLine();

            PrintWhenAnyState(ref p, arity, stateType);
        }

        private static void PrintWhenAnyState(ref Printer p, int arity, string stateType)
        {
            PrintStateDeclaration(ref p, arity, stateType);
            p.OpenScope();
            {
                PrintStateFields(ref p, arity, stateType, includeWinner: true);
                p.PrintLine("private FixedWhenAnyState() { }");
                p.PrintEndLine();

                PrintRent(ref p, arity, stateType, includeWinner: true);

                p.PrintBeginLine("internal async Awaitable<(int winArgumentIndex, ")
                    .Print(GetNamedResults(arity))
                    .PrintEndLine(")> WaitAsync()");
                PrintWaitAsyncBody(ref p);

                for (var i = 1; i <= arity; i++)
                {
                    PrintWhenAnyCompletion(ref p, arity, i);
                }

                PrintWhenAnyDetach(ref p);
                PrintTryRecycle(ref p, arity);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintWhenAnyCompletion(ref Printer p, int arity, int index)
        {
            p.PrintBeginLine("void IAwaitableResultSink<T")
                .Print($"{index}")
                .Print(", AwaitablePosition")
                .Print($"{index}")
                .PrintEndLine(">.Complete(");

            p = p.IncreasedIndent();
            {
                p.PrintLine($"  AwaitablePosition{index} position");
                p.PrintLine($", T{index} result");
                p.PrintLine(", Exception exception");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.CompareExchange(ref _won, 1, 0) != 0)");
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

            p.PrintBeginLine("void IAwaitableResultSink<T")
                .Print($"{index}")
                .Print(", AwaitablePosition")
                .Print($"{index}")
                .PrintEndLine(">.Detach()");
            p.WithIncreasedIndent().PrintLine("=> Detach();");
            p.PrintEndLine();
        }

        private static void PrintWhenAnyDetach(ref Printer p)
        {
            p.PrintLine("private void Detach()");
            p.OpenScope();
            {
                p.PrintLine("if (Interlocked.Decrement(ref _remaining) == 0)");
                p.OpenScope();
                {
                    p.PrintLine("Volatile.Write(ref _detached, 1);");
                    p.PrintLine("TryRecycle();");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintHeader(ref Printer p)
        {
            p.PrintAutoGeneratedBlock(nameof(AwaitablesGenerator));
            p.PrintEndLine();
            p.PrintLine("#pragma warning disable");
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.Collections.Generic;");
            p.PrintLine("using System.Threading;");
            p.PrintLine("using EncosyTower.Debugging;");
            p.PrintLine("using UnityEngine;");
            p.PrintEndLine();
        }

        private static void PrintTupleType(ref Printer p, int arity, bool includeWinner)
        {
            p.PrintLine("(");
            p = p.IncreasedIndent();

            if (includeWinner)
            {
                p.PrintLine("  int winArgumentIndex");
            }

            for (var i = 1; i <= arity; i++)
            {
                var prefix = includeWinner || i > 1 ? ", " : "  ";
                var name = includeWinner ? $" result{i}" : "";
                p.PrintLine($"{prefix}T{i}{name}");
            }

            p = p.DecreasedIndent();
            p.PrintLine(")");
        }

        private static void PrintParameters(ref Printer p, int arity, string typeName, string parameterName)
        {
            p = p.IncreasedIndent();

            for (var i = 1; i <= arity; i++)
            {
                var prefix = i == 1 ? "  " : ", ";
                p.PrintLine($"{prefix}{typeName}<T{i}> {parameterName}{i}");
            }

            p = p.DecreasedIndent();
        }

        private static void PrintStateDeclaration(ref Printer p, int arity, string stateType)
        {
            p.PrintBeginLine("private sealed class ").PrintEndLine(stateType);
            p = p.IncreasedIndent();

            for (var i = 1; i <= arity; i++)
            {
                var prefix = i == 1 ? ": " : ", ";
                p.PrintLine($"{prefix}IAwaitableResultSink<T{i}, AwaitablePosition{i}>");
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
            p.PrintBeginLine("private static readonly Stack<").Print(stateType).PrintEndLine("> s_pool = new();");
            p.PrintBeginLine("private readonly AwaitableCompletionSource<(")
                .Print(tupleTypes)
                .PrintEndLine(")> _source = new();");

            for (var i = 1; i <= arity; i++)
            {
                p.PrintLine($"private T{i} _result{i};");
            }

            p.PrintLine("private int _remaining;");
            p.PrintLine(includeWinner ? "private int _won;" : "private int _signaled;");
            p.PrintLine("private int _consumed;");
            p.PrintLine("private int _detached;");
            p.PrintLine("private int _returned;");
            p.PrintEndLine();
        }

        private static void PrintRent(ref Printer p, int arity, string stateType, bool includeWinner)
        {
            p.PrintBeginLine("internal static ").Print(stateType).PrintEndLine(" Rent()");
            p.OpenScope();
            {
                p.PrintBeginLine(stateType).PrintEndLine(" state;");
                p.PrintEndLine();

                p.PrintLine("lock (s_pool)");
                p.OpenScope();
                {
                    p.PrintBeginLine("state = s_pool.Count > 0 ? s_pool.Pop() : new ")
                        .Print(stateType)
                        .PrintEndLine("();");
                }
                p.CloseScope();
                p.PrintEndLine();

                for (var i = 1; i <= arity; i++)
                {
                    p.PrintLine($"state._result{i} = default;");
                }

                p.PrintLine($"state._remaining = {arity};");
                p.PrintLine(includeWinner ? "state._won = 0;" : "state._signaled = 0;");
                p.PrintLine("state._consumed = 0;");
                p.PrintLine("state._detached = 0;");
                p.PrintLine("state._returned = 0;");
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
                    p.PrintLine("return await _source.Awaitable;");
                }
                p.CloseScope();
                p.PrintLine("finally");
                p.OpenScope();
                {
                    p.PrintLine("Volatile.Write(ref _consumed, 1);");
                    p.PrintLine("TryRecycle();");
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintTryRecycle(ref Printer p, int arity)
        {
            p.PrintLine("private void TryRecycle()");
            p.OpenScope();
            {
                p.PrintLine("if (Volatile.Read(ref _consumed) == 0");
                p.WithIncreasedIndent().PrintLine("|| Volatile.Read(ref _detached) == 0");
                p.WithIncreasedIndent().PrintLine("|| Interlocked.Exchange(ref _returned, 1) != 0");
                p.PrintLine(")");
                p.OpenScope();
                {
                    p.PrintLine("return;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("_source.Reset();");

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
            p.PrintEndLine();
        }

        private static string GetTypeParameters(int arity)
            => string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"T{i}"));

        private static string GetNamedResults(int arity)
            => string.Join(
                ", "
              , Enumerable.Range(1, arity).Select(static i => $"T{i} result{i}")
            );

        private static string GetResultFields(int arity)
            => string.Join(", ", Enumerable.Range(1, arity).Select(static i => $"_result{i}"));
    }
}

#endif
