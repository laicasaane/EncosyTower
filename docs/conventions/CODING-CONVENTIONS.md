# EncosyTower C# Coding Conventions

## How to use this document

- These rules apply to all C# code in this repository.
- Apply readability rules to authored implementation, tests and samples, and to generated C# through its owner.
  Source-generated output is exempt only from the 120-character limit and wrapping triggered solely by that limit.
  Every other applicable Encosy convention still applies to the generated C# itself, not just its writer.
- These rules govern new and changed code; they do not authorize unrelated repository-wide cleanup.
- Apply the version of this document that is current at implementation time. Plans, task snippets, prescribed code
  shapes and receipts written against an earlier version describe behavior. They do not freeze an older convention,
  unless they record an explicit Project Owner exception.
- Formatter suggestions are editing aids and do not override these explicit control-flow and layout rules.
- Follow them when writing new code or reformatting old code.
- Sections are ordered by value for effort: the earlier the section, the more often it applies and the cheaper it is to follow.
- Keep subsystem ownership, concrete policy assignments, feature inventories and implementation plans in their owning
  documentation. This document defines cross-cutting C# and Unity conventions.
- Use nearby valid code of the same type and platform as a candidate pattern, then check it against these rules.
  Existing code is not a blanket conformance certificate.

## 1. Naming

| What | Style | Example |
|---|---|---|
| Classes, structs, enums, delegates | PascalCase | `TypeModel`, `FieldSymbol` |
| Interfaces | `I` + PascalCase | `IPrintable`, `IEquatable` |
| Methods, properties | PascalCase | `Extract()`, `IsValid`, `HasAttribute` |
| Public events, including interface events | PascalCase | `Changed`, `SelectionChanged` |
| Nonpublic events, including internal field-like events | camelCase, no field prefix | `changed`, `selectionChanged` |
| Public/protected fields | camelCase | `enumName`, `hasFlags` |
| Public/protected readonly fields | PascalCase | `Name`, `FullName`, `Attributes` |
| Public/protected static fields | PascalCase | `Default`, `Empty`, `None` |
| Private/internal fields | `_` + camelCase | `_builder`, `_symbol`, `_count` |
| Private/internal static fields | `s_` + camelCase | `s_pool`, `s_safetyId` |
| Constants (`const`) | ALL_UPPER | `PRIME`, `NEWLINE`, `MAX_SIZE` |
| Locals and parameters | camelCase | `hintName`, `builder`, `result` |
| `goto` labels | ALL_UPPER | `FAILED`, `DONE`, `RETRY` |

- Event naming follows the event member's visibility, even when its containing type is internal. An Action/delegate
  field still follows field naming. Preserve names required by native/implemented API contracts; do not rename Unity
  members such as `onValueChanged` or `clicked` to impose the convention on an external API.

- Positional record parameters synthesize properties with the same names. Name those parameters in
  PascalCase so the resulting properties follow the property convention. Ordinary constructor and
  method parameters remain camelCase:

  ```csharp
  internal readonly record struct CompilationSpec(string AssemblyName, bool IsValid);
  ```

- Fields in native container structs follow Unity's `m_PascalCase` style. Wrap them in an IDE1006 pragma pair:

  ```csharp
  #pragma warning disable IDE1006 // Naming Styles
      [NativeDisableUnsafePtrRestriction]
      internal unsafe ListUnsafe<T>* m_Data;

  #if ENABLE_UNITY_COLLECTIONS_CHECKS
      internal AtomicSafetyHandle m_Safety;
  #endif
  #pragma warning restore IDE1006 // Naming Styles
  ```

- Extension classes for types this project does not own (BCL, Unity) are named `Encosy<Type>Extensions`, for example `EncosyStringExtensions`. Extension classes for project types are named `<Type>Extensions`.
- Async methods end with `Async`.
- Custom attribute class names end with `Attribute`.

## 2. Formatting

### 2.1. Basics

- 4-space indentation. No tabs.
- LF line endings.
- Every file ends with a newline.
- One statement per line. Never squeeze a whole method body onto one line.
- Line length: 120 physical characters is the hard limit for authored physical C# files, including generator
  implementations and tests, subject to the documented structural exceptions below. Source-generated code has
  no line-length limit, even when materialized as a generated file or stored in a generated-output snapshot.
- Generators must not calculate output width, branch on a character budget or add wrapping solely to satisfy
  120 characters. Tests must not enforce a generated-output line-length ceiling. Authored writer source must
  still satisfy its own physical-file limit; split its source expressions without changing emitted line breaks.
- Keep a newly authored simple construct on one line when its complete physical line, including indentation and
  surrounding syntax, fits within 120 characters and no structural rule requires multiline layout. During a
  convention repair, normalize an already wrapped construct instead of collapsing it merely because it could fit.
  Logical chains and parameter/argument lists are the exception: they follow the complete-line test in
  Section 2.6 and collapse whenever they fit.
  Explicitly requested/accepted multiline layouts remain multiline. Do not wrap solely to target a shorter line.
- Break below 120 characters only when the multiline layout materially clarifies a structurally
  complex expression. A chain of `&&`/`||` clauses or a parameter/argument list is never complex for this purpose.

### 2.2. Braces

- Opening brace goes on its own line for types, methods, properties, accessors, and control blocks.

  ```csharp
  public void Add(T item)
  {
      CheckResizeWrite();
      ...
  }
  ```

- Every control block body gets braces — `if`, `else`, `for`, `foreach`, `while`, `do`, `switch` cases. No exceptions, even for a single `return` or `count++`.

  ```csharp
  // ❌ SHOULDN'T
  if (count == 0) return;
  if (index < 0)
      return false;

  // ✅ SHOULD
  if (count == 0)
  {
      return;
  }
  ```

- Short inline forms are fine when they fit on one line: `get => _count;`, short lambdas, switch arms, and `new Foo { Value = 42 }`.

### 2.3. Blank lines

- Put a blank line above and below any statement that opens a `{ }` scope — `if`, `for`, `foreach`, `while`, `switch`, `try`, `using`, `lock`, and so on.
- Skip the blank line when two blocks touch by design (`if`/`else`, `try`/`catch`) or when the block is the first or last statement in its parent scope.
- Separate groups with different purposes with exactly one blank line: acquire/validate input, compute state,
  configure objects, register/unregister callbacks, mutate data, publish results and cleanup. Keep related
  one-line statements together; do not insert a blank after every statement. Field spacing follows the
  attribute-block exception below.
- A statement spanning multiple physical lines forms its own block. Put one blank line before and after it,
  even if a neighboring statement is one line. Omit that blank at the first/last boundary of its parent scope.
  This includes multiline assignments, calls and local declarations.
- Separate adjacent property, method and type declarations with one blank line.
- Collapse consecutive empty source lines to one within the owned formatting scope. Do not split connected
  `if`/`else`, `try`/`catch`/`finally`, or `do`/`while` parts, or add gaps between structural closing braces.
- Preserve comments, directives and literal contents. Blank lines inside raw/verbatim strings, golden text or
  intentionally malformed fixture input are not source spacing.

  ```csharp
  var count = list.Count;

  for (var i = 0; i < count; i++)
  {
      var item = list[i];
      DoWork(item);
  }

  return result;
  ```

  ```csharp
  // ❌ SHOULDN'T
  var candidates = context.SyntaxProvider
      .ForAttributeWithMetadataName(...)
      .Where(static candidate => candidate.IsCanonical);
  var outputs = candidates
      .Combine(compilation)
      .Select(static (pair, token) => CreateOutput(pair, token));

  // ✅ SHOULD
  var candidates = context.SyntaxProvider
      .ForAttributeWithMetadataName(...)
      .Where(static candidate => candidate.IsCanonical);

  var outputs = candidates
      .Combine(compilation)
      .Select(static (pair, token) => CreateOutput(pair, token));

  var sync = outputs.Select(static (value, _) => value.Sync);
  var async = outputs.Select(static (value, _) => value.Async);
  ```

  ```csharp
  // Names are shortened. Assume each wrapped statement below exceeds 120 characters on one line.

  // ❌ SHOULDN'T
  builder.Append('|');
  builder.Append(member.Parameters[i].RefKind == ParameterRefKind.ByRef
      ? "ref:"
      : "value:"
  );
  builder.Append(GeneratedSourceText.WriteTypeName(
        member.Parameters[i].Type
      , declaration
  ));

  // ✅ SHOULD
  builder.Append('|');

  builder.Append(member.Parameters[i].RefKind == ParameterRefKind.ByRef
      ? "ref:"
      : "value:"
  );

  builder.Append(GeneratedSourceText.WriteTypeName(
        member.Parameters[i].Type
      , declaration
  ));
  ```

### 2.4. Fields

- Keep single-line fields of the same group adjacent, including fields with inline attributes.
- Treat attributes on separate lines plus their field declaration as one declaration block. Separate that block
  from neighboring field declarations with exactly one blank line, even within the same field group. Keep each
  attribute attached to its own declaration; no blank line between the attribute and field. Apply this to all
  field attributes, including serialization and nonserialization attributes. Do not add an extra blank when a
  group boundary already supplies one, or after the type's opening brace/before its closing brace.

  ```csharp
  [SerializeField]
  private int _minimum;

  [SerializeField]
  private int _maximum;

  [NonSerialized]
  private bool _isUpdating;
  ```

- Inline attributed fields may remain compact; do not move their attributes onto separate lines solely to
  introduce spacing. Serialized data and nonserialized transient state remain distinct groups.
- Groups in this order, separated by one blank line: `const`, then `static readonly`, then `static`, then instance fields.
- One blank line between the last field and the next member.

  ```csharp
  private const int MAX_SIZE = 100;
  private const string PREFIX = "item_";

  private static readonly string[] s_keywords = { "if", "else" };

  private static int s_counter;

  private string _name;
  private int _value;

  public string Name => _name;
  ```

- No column alignment. One space before `=`. Only `switch` expression arms may align their `=>`.

  ```csharp
  // ❌ SHOULDN'T — column-aligned
  var baseTypeSymbol            = compilation.GetTypeByMetadataName(...);
  var isOuterClassSealed        = userClassSymbol.IsSealed;

  // ✅ SHOULD
  var baseTypeSymbol = compilation.GetTypeByMetadataName(...);
  var isOuterClassSealed = userClassSymbol.IsSealed;
  ```

### 2.5. Spaces

- Space after every comma. Spaces around operators. Spaces in generic constraints.
- When a long line wraps, the operator starts the new line.
- Merge compatible adjacent string-literal or interpolated-string segments when their concatenation
  stays on one physical line. Preserve raw strings, incompatible prefixes, or delimiter combinations
  whose merge could change escaping or delimiter semantics.
- In mechanical line-break reformatting, this compatible string merge is the sole permitted token
  change. Every other edit in that pass changes whitespace only and preserves comments, directives,
  and other non-whitespace trivia.

  ```csharp
  // ❌ SHOULDN'T
  public static int IndexOf<T,TComparer>(this in NativeBuffer<T> self,T item,TComparer comparer)
      where T:unmanaged where TComparer:unmanaged,IEqualityComparer<T>

  // ✅ SHOULD
  public static int IndexOf<T, TComparer>(this in NativeBuffer<T> self, T item, TComparer comparer)
      where T : unmanaged
      where TComparer : unmanaged, IEqualityComparer<T>
  ```

### 2.6. Wrapping methods and calls

- For an expression-bodied method, local function, or operator, put `=>` on the next line, indented
  one level. When correcting only arrow placement, replace only the trivia immediately before `=>`
  and preserve the expression's existing multiline layout.
- Keep an expression-bodied property on one line when the complete line fits within 120 characters.
  When it exceeds 120 characters, put `=>` on the next line, indented one level, and wrap the
  expression as needed. Accessors, lambdas, and switch arms may keep `=>` inline.

  ```csharp
  // ✅ SHOULD
  public void AddRange(ReadOnlySpan<T> items)
      => AddRange(items, items.Length);
  ```

- Parameter and argument lists follow the same complete-line test as logical chains below. This covers method,
  local-function, constructor, delegate and operator declarations; calls, including delegate invocations; `new(...)`
  and object creation; and `base(...)`/`this(...)` initializers. Named arguments (Section 4.8) count at their full length.
  - **Fits within 120 characters:** keep the whole list on the line of its opening `(`. This applies to new code
    and to an already wrapped list during a repair.
  - **Exceeds 120 characters:** put every top-level parameter/argument on its own line with leading commas,
    including the first item on the line after its opening `(`, and end with a delimiter-only line. Never pack
    several items onto a wrapped row, even when that row is short.
- Test each list at its own line. After an outer list wraps, an inner call that fits on its argument line stays
  inline; an inner call that does not fit expands under the same rule, with its own B (below).

  ```csharp
  // ❌ SHOULDN'T — exceeds 120 characters on one line
  var memberMetadata = new MemberMetadata(memberName, declaringType, valueType, canRead, canWrite, canNotify, unavailableReason);

  // ❌ SHOULDN'T — fits within 120 characters, wrapped anyway
  error = BindingError.TargetPathStale(
        bindingIndex: _bindingIndex
      , segmentIndex: 0
  );

  // ❌ SHOULDN'T — every argument moved together onto one continuation row
  AVeryLongMethodNameThatIsBeingCalled(
      veryLongParameterNamedA, veryLongParameterNamedB, veryLongParameterNamedC, veryLongParameterNamedD
  );

  // ✅ SHOULD
  AVeryLongMethodNameThatIsBeingCalled(
        veryLongParameterNamedA
      , veryLongParameterNamedB
      , veryLongParameterNamedC
      , veryLongParameterNamedD
  );

  // ❌ SHOULDN'T — the closing delimiters are attached to the last argument
  // (at its real 20-space indentation, the one-line form exceeds 120 characters)
  Publish(new ObservableDictionaryChange<TKey, TValue>(
      new ObservableDictionaryChange.Add<TKey, TValue>(key, value)));

  // ✅ SHOULD — a single argument inside an inline wrapper still closes on a delimiter-only line
  Publish(new ObservableDictionaryChange<TKey, TValue>(
        new ObservableDictionaryChange.Add<TKey, TValue>(key, value)
  ));

  // ❌ SHOULDN'T — wrapped, but several arguments packed on each row
  var memberMetadata = new MemberMetadata(
        memberName, declaringType, valueType
      , canRead, canWrite, canNotify, unavailableReason
  );

  // ✅ SHOULD
  var memberMetadata = new MemberMetadata(
        memberName
      , declaringType
      , valueType
      , canRead
      , canWrite
      , canNotify
      , unavailableReason
  );

  error = BindingError.TargetPathStale(bindingIndex: _bindingIndex, segmentIndex: 0);
  ```

- Count items at the list's own syntax nesting level. Commas inside a short nested call, generic type, tuple or
  lambda parameter list are not separators of the enclosing list; those nested constructs need not also expand.
  A single complex argument may begin inline with its outer wrapper, as in `items.Add(new(` or
  `items.Sort(static (left, right) => string.Compare(`; expand the inner data-argument list, not the wrapper.
- When a method declaration wraps, keep its modifiers, return type, method name, and opening `(` on
  the first line. Wrap only the parameters; do not split this declaration prefix merely to satisfy
  the 120-character limit.

  ```csharp
  // ✅ SHOULD — the complete line fits within 120 characters
  public static TypeModel Extract(INamedTypeSymbol symbol, CancellationToken token, ModelOptions options)

  // ✅ SHOULD — the combined declaration would exceed 120 characters
  public static TypeModel Extract(
        INamedTypeSymbol symbol
      , IAssemblySymbol containingAssembly
      , Compilation compilation
      , SemanticModel semanticModel
      , CancellationToken token
      , ModelOptions options
  )
  ```

- Multi-line call arguments and `new` expressions use the same leading-comma style:

  ```csharp
  var sourceText = TypeCreationHelpers.GenerateSourceText(
        candidate.openingSource
      , declaration.WriteCode()
      , candidate.closingSource
  );

  context.AddSource(candidate.hintName, sourceText);

  return new FieldModel(
        name: field.Name
      , typeName: field.Type.Name
      , typeFullName: field.Type.FullName
      , accessibility: field.Accessibility
  );
  ```

- When compacting an entire control expression to one line, bring its closing `)` onto that same
  line. When the expression remains multiline, keep `)` on its own line. Do not move the delimiter
  independently of the expression compaction.
- For a wrapped list, let B be its layout header indentation: first item text is at B+6 spaces; subsequent
  comma tokens are at B+4, followed by one space and item text at B+6. For `if (TryResolve(`, `Add(new(` or
  `Sort(static (...) => Compare(` on one header, B is the statement's indentation, not each opening token's column.
  A call starting on an `&&`/`||` continuation line uses that line's indentation as B for its arguments.
- If an expanded nested call is itself a separate outer argument after a leading comma, use that argument's
  expression column (after the comma and space) as the nested B. Do not add an indentation level for every
  inline opening parenthesis; indentation represents visible layout blocks.
- A delimiter-only line can contain adjacent closing tokens and required punctuation: `))`, `));` or `)));`.
  Do not require one line per closing parenthesis. If several inline-opened constructs finish together, align
  the closing group with the outermost statement/control header it finishes. If an inner argument closes while
  the outer list continues, align that close with the inner expression's B. Never place argument text on this line.
- For expression lambdas whose body is a call, keep the lambda arrow and called method/opening `(` together with
  the wrapper header when that header fits the authored limit. Wrap the call's arguments, not just the whole body
  onto a new line while leaving all arguments packed together. Method/local-function arrow rules are different.
- A logical chain is an expression that joins two or more clauses with `&&` or `||`, wherever it appears:
  a control condition, `return`, assignment, argument or expression body. Choose its layout only by the
  complete-line test, which counts indentation, keywords, delimiters and any trailing `;`:
  - **Fits within 120 characters:** keep the whole chain on one line. This applies to new code and to an
    already wrapped chain during a repair. Do not wrap a fitting chain at a clause boundary for readability.
  - **Exceeds 120 characters:** put every top-level clause on its own line. The first clause stays after the
    opening `(`, `=`, `=>` or `return`. Each later clause starts a continuation line with its logical operator,
    one indentation level deeper than the statement. Never pack two clauses onto one line, including a short
    initial guard pair.
- Split a chain at its lowest-precedence operator. In `a && b || c`, the top-level clauses are `a && b` and `c`.
  A parenthesized group is one clause: keep it on its line when that line fits; otherwise apply the same test
  inside it, one level deeper, with its closing `)` on a delimiter-only line. When an unparenthesized
  mixed-precedence clause such as `a && b` does not fit on its line, add the precedence-preserving parentheses
  `(a && b)` and expand it as a group; never split it across lines without them.
- Keep each clause intact: cast, member access, comparison operator, right operand, call and pattern stay together.
  If a single clause still exceeds 120 characters on its own line, wrap inside that clause under the list rules
  below; do not re-pack the other clauses.
- Pattern combinators (`or`, `and`) follow the same test (Section 4.1). The fieldwise `Equals` chain in
  Section 9.1 keeps its documented one-comparison-per-line layout.
- Close a multiline control condition on its delimiter-only line aligned with the control statement.

  ```csharp
  // ❌ SHOULDN'T — exceeds 120 characters, several clauses packed on each line
  if (_disposed || _epoch != BinderRegistry.Epoch || _callback == null
      || _listenerGenerations[nodeIndex] != listenerGeneration || _nodes[nodeIndex].IsListening == false
  )
  {
  }

  // ❌ SHOULDN'T — fits within 120 characters, wrapped anyway
  if (added && handle != null && IsCurrentOperation(generation)
      && _directListenerGeneration == listenerGeneration
  )
  {
  }

  // ✅ SHOULD — exceeds 120 characters: one clause per line
  if (_disposed
      || _epoch != BinderRegistry.Epoch
      || _callback == null
      || _listenerGenerations[nodeIndex] != listenerGeneration
      || _nodes[nodeIndex].IsListening == false
  )
  {
  }

  // ✅ SHOULD — fits within 120 characters: one line
  if (added && handle != null && IsCurrentOperation(generation) && _directListenerGeneration == listenerGeneration)
  {
  }

  // ✅ SHOULD — the same test applies outside control statements
  return type.IsClass && type.IsArray == false && isStaticClass == false;

  var eligible = (obsolete == null || obsolete.IsError == false)
      && (browsable == null || browsable.Browsable)
      && (assemblyBrowsable == null || assemblyBrowsable.Browsable);
  ```

- Determine multiline layout from the complete condition, from its opening `(` through its matching `)`.
  If that condition spans multiple physical lines, its outer closing `)` must start a separate delimiter-only
  line at the control keyword's indentation. Never append it to the final predicate, even when that predicate
  is short or the condition contains only two predicates.
  This applies to `if`, `else if`, `while`, and the trailing `while` of `do` (`);` on the closing line).
  Keep a wholly single-line condition's `)` inline. A wrapped logical chain that fits within 120 characters
  collapses to one line under the logical-chain rule above; do not otherwise collapse wrapped conditions merely
  to avoid this rule. Inner calls may still close inline with their own arguments; distinguish their
  closing parentheses from the condition's outer closing parenthesis. Combined closing groups remain permitted
  under the delimiter rule above, and the following opening brace remains on its own line.

  ```csharp
  // Names are shortened. Assume each complete condition below exceeds 120 characters.

  // Incorrect: the outer closing parenthesis is attached to the final predicate.
  if (TryRead(out var value) == false
      || value.Kind != expectedKind)
  {
      return false;
  }

  // Correct: the outer closing parenthesis is aligned with the control keyword.
  if (TryRead(out var value) == false
      || value.Kind != expectedKind
  )
  {
      return false;
  }
  ```

  ```csharp
  // Names are shortened. Assume each wrapped construct below exceeds 120 characters on one line.
  if (isValid
      && targetType != null
      && Registry.TryResolve(
            targetType
          , memberName
          , out result
  ))
  {
      Apply(result);
  }

  choices.Add(new(
        identity
      , label
      , value
  ));

  choices.Sort(static (left, right) => string.Compare(
        left.Identity
      , right.Identity
      , StringComparison.Ordinal
  ));

  if (Registry.TryResolve(
        targetType
      , memberName
      , out result
  ))
  {
      Apply(result);
  }

  if (other == null
      || other.Kind != ExpectedKind
      || other.Count != count
  )
  {
      return false;
  }

  choices.Sort(
        1
      , choices.Count - 1
      , Comparer<Choice>.Create(static (left, right) => string.Compare(
              left.Identity
            , right.Identity
            , StringComparison.Ordinal
        ))
  );
  ```


### 2.7. Object initializers

- Combine construction and immediate member assignments into an object initializer when they are one setup
  operation. Preserve assignment order, setter effects and publication timing; retain separate statements when
  later setup needs the assigned local or observable partial initialization.

- Opening brace on the same line as `new`. Trailing comma after the last entry.

  ```csharp
  var settings = new AesManaged {
      Key = rfc2898.GetBytes(16),
      IV = rfc2898.GetBytes(16),
  };
  ```

- A short initializer may stay on one line: `new Foo { Value = 42 }`.

## 3. Usings & Namespaces

- `using` directives go outside the namespace block in production code.
- Sort alphabetically in three groups, no blank line between them: `System*` first, then `EncosyTower*`, then `Unity*`.

  ```csharp
  using System;
  using System.Collections.Generic;
  using System.Runtime.CompilerServices;
  using EncosyTower.Common;
  using EncosyTower.Debugging;
  using EncosyTower.Logging;
  using Unity.Collections;
  using Unity.Collections.LowLevel.Unsafe;
  ```

- `using static` is fine for importing constant containers.
- Remove unused imports after edits, including imports redundant with the enclosing namespace. Check inactive
  compilation branches and embedded test-source ownership before removing an import.
- Production code uses block-scoped namespaces (`namespace X { ... }`). File-scoped namespaces (`namespace X;`) only in tests and samples.
- One file may hold several namespace blocks when platform-specific partials of the same type must live together under `#if`.
- Conditional type aliases (`using T = ...` under `#if`) go inside the namespace block:

  ```csharp
  namespace EncosyTower.Example
  {
  #if UNITASK
      using UnityTask = Cysharp.Threading.Tasks.UniTask;
  #else
      using UnityTask = UnityEngine.Awaitable;
  #endif
  }
  ```

## 4. Everyday Code Style

- `var` when the type is obvious from the right-hand side. Explicit type otherwise.
- Target-typed `new(...)` when the type is clear from context.
- Prefer pattern matching for type/value dispatch and `switch` expressions. For ordinary C# object null checks,
  use `== null` / `!= null`; do not use `is null`, `is not null` or an empty property pattern merely as a null check.
  Unity object validity and fallback operations follow section 4.7.
- Write `== false` instead of `!` in conditions: `if (string.IsNullOrEmpty(x) == false)`. Plain `!expr` is fine inside expression-bodied operators where it reads naturally.
- No empty control blocks. Invert the condition instead of `if (cond) { }`. Remove empty `else { }`. Merge `if (a) { } else { work; }` into `if (a == false) { work; }`.
- Flatten nested ifs: `if (a && b)` instead of `if (a) { if (b) ... }`. Use `else if` instead of `else { if ... }`.
- `switch` expressions for type dispatch — opening brace on the same line, one arm per line, trailing comma, `_` arm last:

  ```csharp
  public override bool Equals(object obj)
      => obj switch {
          Option<T> other => DefaultEquals(this, other),
          Bool<T> other => Equals(other),
          _ => false,
      };
  ```

### 4.1. Branches and named predicates

- An `is` pattern with multiple `or`/`and` alternatives wraps only when its complete line exceeds 120 characters
  (the logical-chain test in Section 2.6). When it wraps, put each alternative on its own line with
  the pattern operator leading the continuation. Preserve parentheses and precedence; do not pack several
  alternatives onto each wrapped line. Close the containing multiline condition on its delimiter-only line.
- Prefer `out var value` at the first assigning call over a separate declaration used only for that call.
  A conditional expression may declare it in the first arm and use `out value` in the other when definite
  assignment and overload resolution are unchanged. Keep an explicit declaration when required by scope/type.

- Prefer range/index syntax for equivalent string/span slices and last-element access: `value[start..end]`,
  `value[start..]`, `value[..end]`, `value[^1]`. Verify target-profile support. Range ends are exclusive, not lengths;
  retain `Slice(start, length)` when clearer. Preserve bounds, evaluation count, result type and allocations;
  array/string slices are not span views.
- Remove redundant casts only after checking overload selection, numeric conversion and generic inference.
  Prefer a declaration pattern over `as` followed by a null check when the branch consumes the typed value.
- Use `??=` and `?.` for simple ordinary managed-reference initialization/access. For UnityEngine.Object and its
  derived types, choose the applicable EncosyUnityObjectExtensions method under section 4.7. Do not use managed
  null operators as a substitute for native object validity.

- Replace nested conditional operators with explicit branches or a clear switch. A single simple `?:` remains
  valid. Inspect conditionals embedded in interpolation too; preserve branch priority.
- Name distinct domain predicates in compound decisions. Renaming an opaque expression to `condition`, `flag`
  or `isValid` does not make it clearer. Separate independently meaningful checks rather than hiding them together.
- Preserve left-to-right short-circuiting, exception timing, out/pattern variable assignment and call counts.
  Evaluate a later predicate only where the original reaches it. Use guarded declarations, early exits or a lazy
  local helper; never eagerly compute all booleans or move reads across callbacks/awaits for readability alone.
  Simple guards and canonical fieldwise equality need no artificial helper proliferation. In particular, a short
  validity/null guard followed by a lazy Try-call may remain one condition; do not extract booleans merely to meet
  an operator-count threshold or break out-variable scope. Distinct filtering/business decisions still need names.
- When several fixed values select one action, use stacked switch case labels with one braced body and a break
  or return. This means stacked empty labels, not falling through from one nonempty case body. Preserve unmatched
  and null behavior. Switch expressions remain appropriate when selecting a value rather than sharing actions.

### 4.2. Reusable semantic constants

- Use named ALL_UPPER constants for stable semantic strings shared by producers and consumers, such as operation
  names, repeated style classes, serialized property paths and labels. Keep them private to the owner unless
  multiple actual owners need the same contract; then use the nearest shared owner, not a global string registry.
- Preserve exact contents, case, comparison kind, resource/serialized identities and visible text. Do not turn
  runtime-dependent text into a constant, extract every one-off message, or abstract generator syntax fragments
  merely because they contain literals. Follow the authorized identity contract when renaming fields.

### 4.3. Method responsibilities and extraction

- Split a method when it mixes independently nameable responsibilities or hides a decision sequence. Length is
  a review trigger, not a maximum. A cohesive dispatch table or test with a large source literal can remain
  intact with a reason; trivial one-line forwarding helpers are not a readability improvement.
- Prefer a static local function for one caller's pure calculation with explicit inputs. Use a nonstatic local
  function only when surrounding state is intentionally needed and its lifetime/allocation effects are understood.
  Use private instance methods for stateful operations and private static methods for reuse or pure transformations.
  Avoid delegate/capture allocations, new manager classes and scratch-state objects created only to split a method.
- Preserve evaluation order, locks, using/disposal extent, try/catch/finally scope, subscription identity,
  reentrancy flags, early exits and async cancellation. Keep mutable structs and stateful writers such as Printer
  by ref when required; never lose writes in a copied local. In struct methods, do not capture `this` in a local
  function; use explicit ref inputs or a suitable instance helper.
- Preserve local aliases only over proven invariant lifetimes; do not snapshot live callback-visible state.
  Use relevant existing allocation checks when extraction can affect hot paths. Do not scatter AggressiveInlining
  annotations or reorder side-effecting field initializers to satisfy layout conventions.
- Use names and layout for clarity; preserve the comment policy in Section 5.

### 4.4. Loops

- Prefer `for` when the collection has an indexer and a `Count`/`Length`. Cache the count first:

  ```csharp
  var count = list.Count;

  for (var i = 0; i < count; i++)
  {
      var item = list[i];
      // ...
  }
  ```

- Use `foreach` only for types that expose nothing but `GetEnumerator()`.
- Never call LINQ `.Count()` on something that already has `.Count` or `.Length`.
- Bind a repeatedly used indexed element to a meaningful local within the iteration. Prefer an early
  `continue` for rejected elements over nesting the whole loop body under a positive match. Preserve break
  placement, short-circuit order and live mutation semantics. A value-type local copies the element; use a
  supported ref access when writes must affect storage, or retain direct indexing when a snapshot is invalid.
- Cache fields/counts only when invariant over that loop. Preserve intentionally live counts and callback-visible
  state; aliasing a collection reference does not permit copying a mutable struct owner or freezing its contents.

### 4.5. Lambdas & LINQ

- Always favor a local function over creating a delegate from a lambda or anonymous method inside a method body.
  Never assign a lambda or anonymous method to a delegate-typed local (`Action`, `Func<>`, `Predicate<>`, event
  handler or any custom delegate type) and never wrap one in `new SomeDelegate(...)`. Declare a local function
  with the delegate's signature, including `in`/`ref`/`out` modifiers, and pass its method group where the delegate
  is required.
- The same rule covers capturing callback lambdas passed directly to an API that retains or invokes them later:
  event subscriptions, listener or callback registration, scheduling and continuations.
- A lambda remains appropriate only in these forms:
  - a `static` non-capturing lambda written inline as an argument or object-initializer member, including a
    retained callback
    (LINQ, comparers, incremental-generator pipelines, `OnEventAction = static (owner, args) => ...`). The compiler
    caches its delegate. Unity's C# version does not cache method-group conversions, so replacing it with a static
    local function would allocate a new delegate on every conversion;
  - a single-expression predicate or selector written inline as an argument that the call does not retain.
- Register and unregister the same function with the same target/capture lifetime; pair teardown with registration.
  A capturing local function converted to a delegate is not automatically allocation-free. Mark the local function
  `static` when it captures nothing, and follow Section 4.3 for struct methods, which must not capture `this`.

  ```csharp
  // ❌ SHOULDN'T
  ValueChanged changed = (in Variant value) => OnDirectChanged(listenerGeneration, in value);

  // ✅ SHOULD
  void OnChanged(in Variant value)
  {
      OnDirectChanged(listenerGeneration, in value);
  }
  ```

- Mark lambdas `static` whenever they capture nothing: `.Where(static t => t.IsValid)`.
- Multi-statement lambdas go on separate lines, opening brace on the same line as `=>`.
- Keep a LINQ chain on one line while the complete line fits within 120 characters. When it exceeds
  120 characters, wrap one method per line.

### 4.6. Early exit with `goto`

- Parsing and validation methods use labeled `goto` for early exit instead of nested `if` chains. Labels are ALL_UPPER:

  ```csharp
  public bool TryParse(ReadOnlySpan<char> str, out MyType result)
  {
      if (str.IsEmpty)
      {
          goto FAILED;
      }

      if (int.TryParse(str, out var value))
      {
          result = new(value);
          return true;
      }

  FAILED:
      result = default;
      return false;
  }
  ```

### 4.7. Null checks and Unity object operations

- Ordinary C# references use `value == null` / `value != null`. This includes object, string, Type, delegates,
  arrays, VisualElement and managed collection/model objects. A type pattern such as `value is SomeType typed`
  remains appropriate when the branch needs that type. Preserve two-object ReferenceEquals for actual identity
  comparisons; do not use ReferenceEquals(value, null) merely to avoid the null-check rule.
- For UnityEngine.Object and derived types, import `EncosyTower.UnityExtensions` and use
  [EncosyUnityObjectExtensions](../../Packages/com.laicasaane.encosy-tower/EncosyTower.Core/UnityEngine.Extensions/EncosyUnityObjectExtensions.cs).
  Choose by the operation's contract, not by textual similarity. These methods account for destroyed native objects.
  The extension implementation itself retains the primitive Unity operators that implement those queries; never
  rewrite IsValid/IsInvalid to recursively call themselves.

| Required operation | Method | Preconditions and exact behavior |
|---|---|---|
| Branch when a Unity object exists and is alive | `value.IsValid()` | Boolean query; false for a null or destroyed object. Use instead of `value != null` or implicit Unity Boolean conversion. |
| Branch when a Unity object is absent or destroyed | `value.IsInvalid()` | Boolean query; true for either state. Use instead of `value == null`, `!value` or a managed null pattern. |
| Assert an already-required valid object and continue with the same typed reference | `value.AssumeValid()` | Preserves the reference. Its ArgumentException guard is conditional; use only when invalid input is a violated precondition, never as a recoverable Try/false branch or an unconditional release validation guarantee. |
| Select an existing valid object, otherwise an already-available fallback | `value.ValidOrDefault(fallback)` | Returns valid value unchanged; otherwise returns fallback through AssumeValid. The fallback expression is evaluated before the call even when value is valid. Use only when that evaluation is safe and intended. |
| Lazily create and store a replacement for an invalid cached object | `cached.ValidOrInitialize(ref cached, initializer)` | Calls initializer only when the receiver is invalid, passes the result through AssumeValid, assigns the backing field, then returns it. Use a non-null reusable/static initializer where practical. The valid branch does not synchronize a different backing field. |

- `AssumeValid` calls [ThrowIfObjectInvalid](../../Packages/com.laicasaane.encosy-tower/EncosyTower.Core/UnityEngine.Extensions/ThrowHelper.cs:32),
  whose call is conditional on UNITY_EDITOR, DEBUG or RUNTIME_CHECKS. Consequently ValidOrDefault and
  ValidOrInitialize also rely on their fallback/initializer validity preconditions when those checks are absent.
  Nullability annotations describe that contract; they do not create a runtime guard.
- ValidOrInitialize is not a lock, disposal policy or reentry guard. Do not replace existing ownership, cleanup,
  retry or notification logic with it. Preserve lazy evaluation and callback/exception order. If creation returns
  invalid objects as an expected result, use IsInvalid with the existing failure path instead of AssumeValid.
- Do not use `?.`, `??` or `??=` on Unity object receivers to express native liveness. Use IsValid/IsInvalid for
  optional access, ValidOrDefault for an eager fallback, or ValidOrInitialize for the described lazy cache operation.
  Keep ordinary managed null operations on managed results such as a VisualElement's panel.
- If an object/interface slot can hold a Unity object, first use a declaration pattern to obtain the Unity-typed
  value, then call its extension when native liveness matters. For example, reject a managed-null or destroyed
  target with `target == null || target is UnityEngine.Object unity && unity.IsInvalid()`.
  Do not call Unity extensions on arbitrary managed objects or erase a type test that preserves mixed-type behavior.
- A prior validity check does not survive arbitrary callbacks, replacement or await. Re-read the current owner and
  recheck liveness at those existing boundaries. Do not cache a positive result as lifetime ownership.

### 4.8. Named arguments

- Pass an argument by name when the call site alone does not tell the reader which parameter it fills. This
  applies to method calls, local-function and delegate calls, constructors, `new(...)`, and `base(...)`/`this(...)`
  initializers. An argument needs a name when its meaning comes only from its position:
  - a Boolean literal: `true`, `false`;
  - `null`, `default` or `default(T)`;
  - a numeric literal, including sentinels such as `-1` and `0`;
  - a discard `out _` in a call with two or more `out`/`ref` arguments.
- When a call needs one named argument, name every argument of that call. Keep them in declaration order, and
  put `in`/`ref`/`out` after the name: `targetPath: in targetPath`, `error: out error`.
- Do not use names to reorder arguments or to skip optional parameters the call previously supplied. Named
  arguments evaluate in the order they are written, so the declaration order preserves evaluation order.
  Verify the call still binds to the same overload.
- No name is needed when the context identifies the parameter:
  - the only argument, when the method name states what it is: `GetValueOrDefault(-1)`, `SetActive(false)`,
    `Add(null)`, and `Dispose(true)` in the dispose pattern;
  - a canonical index/count range operation with BCL or `List<T>` parameter order: `Array.Copy`, `Array.Clear`,
    `AsSpan`, `Slice`, `Math.Max`/`Math.Min`, `IndexOf`, `FindIndex`, `CopyTo`, `Sort` overloads with that shape,
    and project overloads that mirror them (`TryCopyTo`, `CopyFrom`, `TryCopyFrom`);
  - a single `out _` in a call that has no other `out`/`ref` argument: `TryGetTarget(out _)`;
  - named locals, fields, properties, constants, enum members, `nameof` or `typeof` arguments.
- Named arguments follow the list layout in Section 2.6: inline while the complete line fits within
  120 characters, otherwise one argument per line with leading commas. Keep `name: value` together on one line.

  ```csharp
  // ❌ SHOULDN'T
  if (TryResolveTargetPathCore(
        null
      , rootType
      , in targetPath
      , default
      , default
      , -1
      , true
      , false
      , true
      , true
      , default
      , excludedSelectionIndex
      , out var metadata
      , out _
      , out _
      , out error
  ) == false
  )

  // ✅ SHOULD
  if (TryResolveTargetPathCore(
        target: null
      , rootType: rootType
      , targetPath: in targetPath
      , rootBinderId: default
      , childBinderIds: default
      , bindingIndex: -1
      , metadataOnly: true
      , prefix: false
      , validateSelection: true
      , allowTargetPathOverrides: true
      , finalBinderId: default
      , excludedSelectionIndex: excludedSelectionIndex
      , metadata: out var metadata
      , indices: out _
      , directIndex: out _
      , error: out error
  ) == false
  )

  // ❌ SHOULDN'T
  var obsolete = type.GetCustomAttribute<ObsoleteAttribute>(false);
  error = BindingError.BinderNotFound(-1);

  // ✅ SHOULD
  var obsolete = type.GetCustomAttribute<ObsoleteAttribute>(inherit: false);
  error = BindingError.BinderNotFound(bindingIndex: -1);
  ```

## 5. Comments

- **Do not write comments or XML docs unless the Project Owner asks for them.** This covers `//`, `/* */`, section dividers, and every XML doc tag.
- **Do not touch existing comments** in files you edit. Leave them exactly as they are.
- Record an exact protected-comment exception for a pre-existing overlength comment instead of changing it to
  satisfy the line-length checker. This permits neither new overlength comments nor overlength executable code.
- Always allowed:
  - `// SAFETY:` comments above `unsafe` blocks — these are **required** (see Section 13)
  - License headers and attribution comments on vendored or ported files
  - Auto-generated comments from build tools (`<auto-generated>`)
  - `#region` / `#endregion`
- When XML docs are requested, keep them short: `<summary>` says what the type does, `<remarks>` covers thread safety and constraints. Never repeat what the code already says.

  ```csharp
  /// <summary>
  /// A dictionary that stores its values in a contiguous array, so the values
  /// can be iterated directly as an array, without an enumerator.
  /// </summary>
  /// <remarks>
  /// Not thread-safe.
  /// </remarks>
  public partial struct NativeMap<TKey, TValue> : ...
  ```

## 6. Files & Layout

### 6.1. File naming

| File content | Convention | Example |
|---|---|---|
| Single primary type | Type name matches file name | `Example.cs` |
| Partial split of a type | `TypeName+Aspect.cs` | `Example+ReadOnly.cs` |
| Nested type split | `TypeName+NestedTypeName.cs` | `Example+Enumerator.cs` |
| Generic type | `` TypeName`N.cs `` | `` Container`1.cs ``, `` Pair`2.cs `` |
| Async extension file | `TypeName_Async.cs` | `` Worker`1_Async.cs `` |
| Backend-specific partial | `TypeName_Backend.cs` | `Worker_BackendA.cs`, `Worker_BackendB.cs` |
| Generated file | `*.gen.cs` — never hand-edit | `Example.gen.cs` |

- One primary type per file. Small helper types may stay next to the type they serve.
- One extension class per file:

  ```text
  BufferExtensions.cs
  ReadOnlyBufferExtensions.cs   // separate file, not appended to the file above
  ```

- Mark a type `partial` when it is split across files.
- A module folder keeps its own `AssemblyInfo.cs` when it needs assembly-level attributes.
- Files copied or ported from other projects keep the original license header at the very top, plus a comment linking to the source URL.

### 6.2. Generated code and test inputs

- Apply these conventions to generator implementation and emitted C#, except that emitted output has no
  120-character ceiling or wrapping triggered solely by that ceiling. All other rules remain mandatory: naming,
  member order, indentation, braces, meaningful blank lines, isolated multiline statements, leading commas and
  separate closing delimiters for multiline constructs, method/local-function arrow placement, readable control
  flow, constants, safe method structure and applicable API/performance rules. This applies to every generator.
- Choose deterministic structural templates independent of identifier length. Keep fixed multiline layouts when
  structurally appropriate; removing a width check does not authorize flattening blocks or omitting separators.
  Use Printer line/scope operations deliberately and pass mutable writer state by ref to section helpers.
- Distinguish authored test/fixture code from embedded or saved generated-output text; never shorten generated
  identifiers, alter types, or wrap generated output to pass a physical-file line-length audit.
- Correct writers/templates and regenerate;
  never independently patch generated caches or golden outputs as the lasting fix.
- Writer-only restructuring preserves exact emitted bytes. An intentional emitted convention change requires
  before/after output review, unchanged public/serialized/metadata/hint/diagnostic contracts, strict output and
  compilation checks, and focused execution when control flow changes. Whitespace-only changes preserve tokens
  and non-whitespace trivia; update only reviewed expectations through their owner.
- Preserve intentionally malformed or specially named test input proving diagnostics, discovery, serialization
  or naming. Review the authored test around it; do not normalize away the behavior under test.

### 6.3. Generator rendering patterns

- Compose output through coherent section helpers and pass mutable writer state by ref.
- Emit indentation, parameter/member separators and balanced scopes explicitly; do not infer layout from
  identifier width or a physical-line budget.
- Review the actual emitted source against these conventions. A readable writer or an existing generated file
  does not prove that the resulting source follows them.
- Keep producer-specific examples and implementation references with the owning generator's documentation.

### 6.4. Define blocks

- Do not add a file-local define block merely because a file calls or declares validation guards.
  Reference the established validation-policy owner's constants or properties.
- Keep API/type/field availability separate from optional runtime validation. A C# constant or property cannot
  define another file's preprocessor symbols. Retain compile-time guards where declarations or layout require them.
- When existing inline validation uses a local preprocessor block, match the owning policy's enabled and disabled
  profiles exactly. The following illustrates the common opt-in shape, not an exhaustive policy for every module:

  ```csharp
  #if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
  #define __ENCOSY_NO_VALIDATION__
  #else
  #define __ENCOSY_VALIDATION__
  #endif
  ```

- Keep additional symbol families, precedence and profile matrices with their policy owner rather than copying
  a feature-specific symbol catalog into these conventions. Use an established runtime policy property where
  that contract calls for runtime branching; do not duplicate its formula in each caller.
- Keep feature/API availability wrappers outermost. Do not replace Unity's native-container safety guards with
  application validation symbols or assume an application opt-out disables the platform's own safety mechanism.

- Editor-only files: wrapped whole in `#if UNITY_EDITOR`, live in an `Editor*` folder, use an `EncosyTower.Editor.*` namespace, and mark public APIs with `[ApiForEditor]`.

  ```csharp
  #if UNITY_EDITOR

  namespace EncosyTower.Editor.Common
  {
      [ApiForEditor]
      public static class ExampleEditorExtensions
      {
          ...
      }
  }

  #endif
  ```

## 7. Member Ordering

Group backing fields before properties; do not alternate each serialized field with its accessor. In Unity
types, keep serialized fields before nonserialized instance state, preserving initializer evaluation order.

Members appear in this order of member kinds:

1. Fields
2. Constructors (static constructor first)
3. Indexers
4. Properties
5. Operators
6. Methods
7. Nested types (interfaces, then static classes, then other types)

Within each kind, sort twice:

- First by modifier: `const` → `static readonly` → `static` → instance.
- Then by visibility: `public` → `protected` → `private`.

Separate each member-kind, modifier, and visibility group with one blank line. Inside a field group,
keep single-line declarations adjacent; separate multiline attributed declarations as specified in Section 2. Adjacent property, method, and nested-type
declarations remain separated by one blank line as required by Section 2.

Example skeleton:

```csharp
public const int MAX = 10;

private const int MIN = 0;

public static readonly Foo Default = new();

private static int s_counter;

private int _value;

public Foo(int value) { ... }

public int Value => _value;

public static Foo operator +(Foo a, Foo b) { ... }

public static Foo Create() { ... }

public void DoWork() { ... }

private void DoWorkCore() { ... }
```

### 7.1. Unity types (`MonoBehaviour`, `ScriptableObject`, ...)

Same rules, with one change: serialized fields come first, because Unity serializes `public` fields by default.

1. `public` fields (serialized by default)
2. `[SerializeField] protected` fields
3. `[SerializeField] private` fields
4. Everything else, in the plain-C# order above.

## 8. Type & API Design

### 8.1. Choosing a type shape

- `struct` for value-like models and small data carriers.
- `readonly struct` and `readonly record struct` for immutable value types.
- Positional record parameters use PascalCase because they define synthesized properties.
- Use positional declarations as the default for data records: `readonly record struct Name(Type Member, ...)`
  for immutable values and explicit `record class Name(Type Member, ...)` for reference records. Preserve deliberate
  mutability, sealing, inheritance, visibility, generic arity and class-versus-struct semantics. A record without
  additional behavior ends with `;`.
- Do not repeat positional data as assignment-only fields/properties and constructors. A body needs a concrete
  reason: validation, a computed convenience constructor, required member documentation/attributes, deliberate
  accessor behavior/visibility, serialization/layout/ref-access constraints or actual methods. Prefer a small
  explicit member override with a positional header; initialize a matching property/field from its primary
  parameter. Record the reason in the task/review, not a new source comment unless comments were requested.
- Empty tag records may omit the parameter list. Additional generated partial declarations must not repeat the
  owning declaration's primary parameters. Preserve intentionally shaped diagnostic/behavior test inputs.
- Converting an existing manual record changes its member shape, not only whitespace. Synthesized public
  init-properties, primary argument names, Deconstruct order and diagnostic ToString output can change. Obtain
  scope authority, update consumers directly and check defaults, equality/hash agreement and shallow copies.
  Records already support `with`; positional init-properties enable assignments to those members in the copy.
  Preserve evaluation order when arguments move, especially when all arguments have the same type.
- This default does not authorize converting ordinary classes/structs to records or removing a proven special-case
  contract. New/rewritten records follow it; existing shape conversions remain within their named task scope.
- Prefer an immutable value representation for wrappers and identifiers when it fits their API and layout.
  A readonly record struct can express that shape; required generation attributes belong to the owning contract.
  This does not authorize changing existing public, serialized or fixed-layout representations for style alone.

  ```csharp
  public readonly record struct ValueId(int Value) : IIsValid
  {
      public bool IsValid
      {
          [MethodImpl(MethodImplOptions.AggressiveInlining)]
          get => Value > 0;
      }
  }
  ```

- Implement `IEquatable<T>` on every struct used in equality comparisons or as a cache key.
- Marker types with no members are `internal readonly struct`.
- Nested types are fine for closely related helpers. A nested static class can group related API helpers:

  ```csharp
  public static partial class ExampleContainer
  {
      public static class API { ... }
  }
  ```

### 8.2. API rules

- Remove an unused internal/private parameter and its corresponding call arguments together after checking all
  callers, named arguments, delegates, reflection and generated consumers. Preserve argument side effects.
  Do not carry obsolete state through constructors for a removed feature. Public/serialized contracts require
  their existing compatibility authority; unused syntax alone does not authorize changing them.

- Reuse the shared capability interfaces instead of inventing new members: `IIsCreated`, `IIsValid`, `IHasValue`, `IHasCount`, `IHasCapacity`, `IClearable`, `IToArray<T>`, `IAsSpan<T>`, ...
- Sentinel and default instances are `static readonly` fields:

  ```csharp
  public readonly static Option<T> None = default;
  public static readonly DevLogger Default = new();
  ```

- A method that can fail returns `Option<T>` or follows the `bool TryXxx(...)` pattern. Never return `null`, never throw for an expected failure.

  ```csharp
  public Option<T> Find([NotNull] Predicate<T> match) { ... }
  public bool TryAdd<T>(T instance) { ... }
  ```

- Custom attributes always declare `[AttributeUsage(...)]` and validate their constructor arguments.
- Unity-serialized structs use `[Serializable]` with `[field: SerializeField]` on auto-properties, not public fields:

  ```csharp
  [Serializable]
  public struct SampleData
  {
      [field: SerializeField]
      public string Name { get; set; }
  }
  ```

### 8.3. Access modifiers

- Always write the accessibility on non-interface members.
- Modifier order: `public`, `private`, `protected`, `internal`, `static`, `extern`, `new`, `virtual`, `abstract`, `sealed`, `override`, `readonly`, `unsafe`, `volatile`, `async`.
- Never change an existing public API for style alone — type names, namespaces, signatures, receivers, `in`/`ref`/`readonly`, or field shapes stay as they are.
- Interface members with a default implementation include `public`:

  ```csharp
  public interface IItemSource
  {
      public bool TryGetItem(int index, out object result)
      {
          result = default;
          return false;
      }
  }
  ```

### 8.4. Inheritance & constraint wrapping

- The base list stays on the same line while the whole declaration fits within 120 characters:

  ```csharp
  public struct LocationInfo : IEquatable<LocationInfo>
  ```

- Past 120 characters, wrap with leading commas, one entry per line, indented one level. Interfaces
  gated by defines come last:

  ```csharp
  internal struct PropertyDefinition
      : IEquatable<PropertyDefinition>
      , ICloneWithDim<PropertyDefinition>
      , ICast<PropertySignature>

  public partial struct NativeBuffer<T> : IDisposable, IReadOnlyList<T>, IIndexer<T>
      , IAsSpan<T>, IAsReadOnlySpan<T>, IToArray<T>
      , IIncreaseCapacity, IClearable
  #if UNITY_COLLECTIONS
      , INativeDisposable
  #endif
      where T : unmanaged
  ```

- `where` clauses always go on the next line, one per line, indented one level:

  ```csharp
  public sealed class ObjectPool<T>
      where T : class

  public static string PrintToString<T>(T printable)
      where T : struct, IPrintable
  ```

### 8.5. Variant state and deferred string composition

- Store the inputs of a derived string, not the composed string. Messages, warnings, labels and other text built
  from dynamic values are composed only when a consumer explicitly requests the text: logging, an exception,
  a UI display or `ToString`. Cached state, registration records, results and error values keep references to the
  existing source values (strings such as `type.FullName` or an attribute's `Message`, numbers, enums, `Type`).
  An explicit, cold method composes the final text: `ToMessage`, `ToWarning` or `ToString`, marked
  `[MethodImpl(MethodImplOptions.NoInlining)]`.
- Do not encode which variant a state is with sentinel values: `string.Empty` or `null` meaning "no warning",
  `-1` meaning "none", or a combination of flags that only some payloads use. When a state has variants whose
  payload or behavior differs, model it as a `[PolyEnumStruct]`:
  - one case per variant; each case stores only its own payload, as a positional `readonly partial record struct`
    or a `readonly partial struct` when it has no payload;
  - behavior shared by the cases is declared on `IEnumCase` and implemented by the cases that differ from its
    default. Consumers call the generated method; they do not switch on cases, chain `TryGetValue` branches or
    cast the value to `IEnumCase`, which can box;
  - give `Undefined` an explicit case implementation when its fallback is not zero, `false` or `default`.
- Keep bookkeeping that does not belong to one variant, such as a warned-once flag, in the owning state struct
  next to the PolyEnumStruct value.
- Preserve the composed text, its trigger and its once-only reporting behavior. Where the owning assembly cannot
  use the PolyEnumStruct generator, still store the source values and compose on request; do not add an assembly
  reference for this rule alone without authority.

  ```csharp
  // ❌ SHOULDN'T — the warning is composed eagerly, and string.Empty encodes "no warning"
  private struct TypeState
  {
      internal readonly bool _isEligible;
      internal readonly string _warning;
      internal bool _warned;
  }

  var warning = obsolete == null || obsolete.IsError ? string.Empty
      : $"Type '{type.FullName}' is obsolete."
          + (string.IsNullOrEmpty(obsolete.Message) ? string.Empty : $" {obsolete.Message}");

  state = new TypeState(eligible, warning);
  ```

  ```csharp
  // ✅ SHOULD — each case keeps references to its inputs; the text is composed only on request
  private struct TypeState
  {
      internal readonly TypeEligibility _eligibility;
      internal bool _warned;

      internal TypeState(in TypeEligibility eligibility)
      {
          _eligibility = eligibility;
          _warned = false;
      }
  }

  [PolyEnumStruct]
  internal readonly partial struct TypeEligibility
  {
      partial interface IEnumCase
      {
          bool IsEligible => false;

          bool HasWarning => false;

          string ToWarning()
              => string.Empty;
      }

      public readonly partial struct Ineligible { }

      public readonly partial struct Eligible
      {
          public readonly bool IsEligible => true;
      }

      public readonly partial record struct Obsolete(string TypeName, string Message)
      {
          public readonly bool IsEligible => true;

          public readonly bool HasWarning => true;

          [MethodImpl(MethodImplOptions.NoInlining)]
          public readonly string ToWarning()
              => string.IsNullOrEmpty(Message)
                  ? $"Type '{TypeName}' is obsolete."
                  : $"Type '{TypeName}' is obsolete. {Message}";
      }
  }

  TypeEligibility eligibility;

  if (eligible == false)
  {
      eligibility = new TypeEligibility.Ineligible();
  }
  else if (obsolete == null)
  {
      eligibility = new TypeEligibility.Eligible();
  }
  else
  {
      eligibility = new TypeEligibility.Obsolete(type.FullName, obsolete.Message);
  }

  state = new TypeState(eligibility);

  // The consumer requests the text only when it reports it.
  if (state._eligibility.HasWarning && state._warned == false)
  {
      state._warned = true;
      s_types[type] = state;
      ThrowHelper.LogObsoleteBindingType(role, state._eligibility.ToWarning());
  }
  ```

## 9. Members & Attributes

- Related attributes may share a line: `[SerializeField, HideInInspector]`.
- Long or documentation-relevant attributes get their own lines:

  ```csharp
  [SerializeField]
  [HideInInspector]
  internal string _subtitle;
  ```

- `[MethodImpl]` can share a line with other attributes on a small method, or sit alone above the signature.
- Use `[field: SerializeField]` to serialize an auto-property's backing field:

  ```csharp
  [field: SerializeField]
  public string Name { get; set; }
  ```

- On properties, `[MethodImpl]` goes on the accessor, not the property:

  ```csharp
  public readonly int Count
  {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _count;
  }
  ```

- Multi-line accessors keep the same shape:

  ```csharp
  public T this[int index]
  {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
          ThrowHelper.ThrowIfIndexOutOfRange((uint)index < (uint)_count);
          return _buffer[index];
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
          ThrowHelper.ThrowIfIndexOutOfRange((uint)index < (uint)_count);
          _version++;
          _buffer[index] = value;
      }
  }
  ```

- Value APIs come in pairs — one overload takes `T item`, one takes `in T item`:

  ```csharp
  public void Add(T item) { ... }
  public void Add(in T item) { ... }
  ```

- Extension methods mark the receiver `[NotNull]` and take structs by `in`:

  ```csharp
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Contains<T>([NotNull] this in NativeBuffer<T> self, T item)
      where T : unmanaged, IEquatable<T>
  ```

- Every struct instance method that does not mutate state is marked `readonly`:

  ```csharp
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly override int GetHashCode()
      => _value.GetHashCode();
  ```

- Use `in` for readonly struct parameters on hot paths to avoid copies. Never use `in` for classes or `ref struct` types.

  ```csharp
  public static bool Equals(in Option<T> a, in Option<T> b)
      where T : IEquatable<T>
  ```

### 9.1. The `IEquatable<T>` pattern

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly bool Equals(Foo other)
    => string.Equals(fieldA, other.fieldA, StringComparison.Ordinal)
    && fieldB == other.fieldB
    ;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly override bool Equals(object obj)
    => obj is Foo other && Equals(other);

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly override int GetHashCode()
    => HashCode.Combine(fieldA, fieldB);

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static bool operator ==(Foo left, Foo right)
    => left.Equals(right);

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static bool operator !=(Foo left, Foo right)
    => left.Equals(right) == false;
```

- Every string comparison in `Equals` uses `string.Equals(..., StringComparison.Ordinal)`.
- Combine hashes with `HashCode.Combine` (or a project equivalent). Never sum raw field hashes by hand.
- Multi-line boolean chains end with a trailing `;` on its own line.

## 10. Errors & Validation

The goal: keep the happy path fast and clean, keep throwing and logging out of it.

### 10.1. Ground rules

- Never throw a raw exception at the call site. Use a dedicated helper.
- One helper checks one rule. Name it after what it checks.
- Every authored `Throw*`, `Log*`, `Create*Exception` and rethrow helper belongs to a static class named
  `ThrowHelper` owned by its module/submodule, including rules with only one caller. Do not declare these helpers
  on collection, attribute, registry, view or controller types. Ordinary predicates/calculations remain with their owner.
- Check existing shared guards/factories before adding a specialization. Verify accessibility and preserve the
  required exception, parameter/message, logging and conditional-call contract. Prefer a direct matching shared
  guard or a matching factory on the cold branch; do not copy it or widen internal APIs to force a mismatch to fit.
- Preserve helper namespaces, public signatures, metadata identities and generated call sites within the authorized
  change scope. Compiler/test-only failure helpers stay in their own module/assembly; do not turn them into runtime
  APIs or introduce inappropriate dependencies.

- The local static `CreateException` pattern below is allowed only inside a `ThrowHelper` member. A reusable factory
  may be a type member on that helper. Never move the exception construction into an ordinary caller's local function.
- Never call generic `ThrowHelper.ThrowIfFalse(...)` from collection call sites.
- Never write a catch-all `Validate(...)` that mixes unrelated checks.
- Every throw helper carries `[HideInCallstack, StackTraceHidden]` so it stays out of Unity console stack traces.
- Keep internal message categories internal and format their names only on the cold exception path.
  Preserve the owning helper's fallback for unknown categories; use a matching accessible factory or a justified
  local specialization instead of exposing internal types solely for reuse.

### 10.2. Validation contracts

- Distinguish required failures from optional validation and diagnostic reporting. The owning API contract defines
  enabled and disabled behavior; do not infer it solely from a helper's name or change it during a style cleanup.
- Required terminal failures and exception factories remain available under the applicable API contract.
  A disabled diagnostic must not turn failure into reported success or remove required cleanup.
- Keep flow-analysis annotations truthful. Do not return normally from a helper on a path that its
  DoesNotReturn/DoesNotReturnIf contract says cannot return.
- Where ordinary helpers use caller-gated validation, put the gate around the automatic call before evaluating
  validation-only arguments. The explicitly called helper still enforces its predicate.
- Keep mutation, user callbacks and computations required by the real operation outside removable validation.
  Preserve documented Try/false/default fallbacks and exception/cleanup order.

### 10.3. Validation policy ownership

- Keep symbol names and enablement rules in the established policy owner. Do not copy a policy formula into every
  helper or caller, and do not make a task-specific migration a repository-wide rule.
- Multiple Conditional attributes combine with OR at the caller. An omitted conditional call also omits its
  argument evaluation. File-local defines do not propagate to other source files or assemblies.
- Attribute constants are compiled into the declaring assembly; call inclusion depends on the caller's symbols.
  Runtime policy getters likewise reflect their owning compilation. Rebuild affected owners and consumers when
  changing policy, and verify separate compilations when assembly-local behavior matters.
- Follow the complete call chain, including forwarding wrappers, inline branches and logging sinks.
  Changing one attribute or outer gate does not prove the intended behavior reaches the final operation.
- Generated code can reference only accessible policy/helper APIs. Preserve private implementation boundaries;
  do not expose an internal policy merely to make generated consumers or tests compile.
- Keep concrete symbol sets, default-on versus opt-in decisions, migration lists and profile-specific acceptance
  in the owning module's documentation. These conventions do not assign one feature's defaults to another.

### 10.4. Conditional guards (`ThrowIfXxx`)

- The guard takes the already-evaluated condition as a `bool`, marked `[DoesNotReturnIf(false)]` (or `[DoesNotReturnIf(true)]` when true means throw).
- Import the established validation-policy constants and use their applicable Conditional set when the owning
  contract calls for removable validation. Common opt-in guards use UNITY_EDITOR, DEBUG and RUNTIME_CHECKS;
  any additional constants and precedence come from the policy owner.
- The guard itself must **not** be `NoInlining` — the fast path stays inlineable. Exception construction goes into a
  static `CreateException` local function inside that ThrowHelper member, marked `[MethodImpl(NoInlining)]`, or a
  matching existing Core factory. Factories return normally and must not be marked `[DoesNotReturn]`.
- Method-declaration fragments in this section belong to the appropriate static ThrowHelper. Separate predicates and
  caller fragments are labelled; their placement is not permission to put a guard on a business type.

  The two snippets below are unrelated checks; they only show the wrong and right shape of the pattern.

  ```csharp
  using static EncosyTower.Debugging.ValidationDefines;

  // ❌ SHOULDN'T
  ThrowHelper.ThrowIfFalse((uint)startIndex < (uint)_count, "out of bound start index");
  ThrowHelper.ThrowIfFalse((uint)length <= (uint)(_count - startIndex), "out of bound length");

  // ✅ SHOULD
  [HideInCallstack, StackTraceHidden]
  [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
  [Conditional(RUNTIME_CHECKS)]
  internal static void ThrowIfCountInvalid([DoesNotReturnIf(false)] bool validCount)
  {
      if (validCount == false)
      {
          throw CreateException();
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      static ArgumentOutOfRangeException CreateException()
          => new("count", "Count must be non-negative.");
  }
  ```

- The `[DoesNotReturnIf(true)]` variant, for checks where true means failure:

  ```csharp
  [HideInCallstack, StackTraceHidden]
  internal static void ThrowIfSameType([DoesNotReturnIf(true)] bool check)
  {
      if (check)
      {
          throw CreateException();
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      static InvalidOperationException CreateException()
          => new("Value type and error type must be different.");
  }
  ```

- Complex conditions (`sizeof`, type comparisons, ...) get their own `[MethodImpl(AggressiveInlining)]` helper, evaluated at the call site:

  ```csharp
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool IsValidSize()
      => UnsafeUtility.SizeOf<TBuffer>() >= UnsafeUtility.SizeOf<T>();

  static MyType()
  {
      ThrowHelper.ThrowIfInvalidSize<TBuffer, T>(IsValidSize());
  }

  [HideInCallstack, StackTraceHidden]
  internal static void ThrowIfInvalidSize<TBuffer, T>([DoesNotReturnIf(false)] bool check)
  {
      if (check == false)
      {
          throw CreateException();
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      static InvalidOperationException CreateException()
          => new($"sizeof({typeof(TBuffer)}) must be >= sizeof({typeof(T)})");
  }
  ```

### 10.5. Unconditional throw helpers (`ThrowXxx`)

- A method that always throws combines `[MethodImpl(NoInlining)]`, `[HideInCallstack, StackTraceHidden]`, and `[DoesNotReturn]`:

  ```csharp
  [MethodImpl(MethodImplOptions.NoInlining)]
  [HideInCallstack, StackTraceHidden, DoesNotReturn]
  internal static void ThrowArgumentNullException(string paramName)
      => throw new ArgumentNullException(paramName);
  ```

- If an always-throwing diagnostic is intentionally conditional, use its owning validation-policy set.
  Required terminal failures remain outside optional suppression.

### 10.6. Recoverable failures

- When a `Try*` failure requires logging, call its named `ThrowHelper.LogError_Xxx` helper and return `false`.
  Preserve a feature's ordinary silent-false contract; do not invent warnings for expected misses:

  ```csharp
  if (_items.ContainsKey(key))
  {
      ThrowHelper.LogError_KeyAlreadyExists(key);
      return false;
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  [HideInCallstack, StackTraceHidden]
  internal static void LogError_KeyAlreadyExists<TKey>(TKey key)
      => StaticDevLogger.LogError($"An entry with key '{key}' already exists.");
  ```

- A method that throws in dev builds but must keep working in release builds uses both paths:

  ```csharp
  #if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
  #define __ENCOSY_NO_VALIDATION__
  #else
  #define __ENCOSY_VALIDATION__
  #endif

  if (instance == null)
  {
  #if __ENCOSY_VALIDATION__
      Debugging.ThrowHelper.ThrowIfNull(instance);
  #else
      return false;
  #endif
  }
  ```

### 10.7. Messages & checks

- Error messages are plain sentences. Say what is required or what went wrong.
- Long messages split across concatenated interpolated strings, one per line:

  ```csharp
  throw new InvalidCastException(
      $"Cannot cast an instance of type {obj.GetType()} to {typeof(T)} " +
      $"even though it is registered for {typeof(T)}"
  );
  ```

- Range checks use the unsigned-cast trick: `(uint)index < (uint)_count`. One cast catches both negative and too-large values.
- Prefer string interpolation over concatenation. Raw string literals (`"""..."""`) in tests and generated code.

## 11. Performance Annotations

### 11.1. `[MethodImpl(AggressiveInlining)]` — hot paths

The JIT already inlines tiny methods on its own (bodies within ~32 IL bytes, roughly up to 5 simple statements). The attribute pays off on bodies of **5 to 9 statements** in these shapes:

- Property accessors with real logic
- Operator overloads and conversion operators
- Wrappers that forward to a single call
- `return new(...)` factory methods

Annotating a 1–4 statement body is redundant, but acceptable on hot-path structs for explicitness.

Do **not** apply when:

- the body has more than 9 statements
- the method contains loops, deep branching, `async`/`await`, or `try`/`catch`/`finally`
- the method is on a cold path

### 11.2. `[MethodImpl(NoInlining)]` — cold paths

Cold code must opt out of inlining. Otherwise the JIT may inline it into hot callers, bloating the call site and polluting the instruction cache.

Always annotate:

- **Unconditional throw helpers** — methods that always throw
- **Exception factories** — type members or local functions inside ThrowHelper guards
- **Logging methods** — string formatting and I/O are never hot
- **Error and fallback paths** — anything that only runs when things go wrong

Conditional attributes control call inclusion independently. Keep NoInlining on cold throw/log/factory methods
that remain in a build; do not add Conditional attributes to mandatory failures merely to omit their work.

```csharp
// Unconditional throw helper — always throws, annotate the method directly
[MethodImpl(MethodImplOptions.NoInlining)]
[HideInCallstack, StackTraceHidden, DoesNotReturn]
internal static void ThrowArgumentNullException(string paramName)
    => throw new ArgumentNullException(paramName);

// Logging — cold path, never inline
[MethodImpl(MethodImplOptions.NoInlining)]
internal static void LogWarningInvalidUserId(ILogger logger)
{
    logger.LogWarning("User id is invalid.");
}

// Debug-only — [Conditional] compiles it away; no annotation needed
[Conditional("DEBUG")]
private static void AssertIndexInRange(int index, int length)
{
    Debug.Assert((uint)index < (uint)length);
}
```

## 12. Async, Logging & Conditional Compilation

### 12.1. Async

- Async method names end with `Async`.
- `CancellationToken token = default` is always the last parameter. Pass the token to every async call.
- Check the token at the start of significant work: `if (token.IsCancellationRequested) { return ...; }`.
- Always re-throw `OperationCanceledException`. Never swallow it.
- Async code supports both UniTask and Unity `Awaitable` through the `UNITASK` define and the `UnityTask` alias (see Section 3).

  ```csharp
  public static async Awaitable WaitUntilAsync(
        this MonoBehaviour target
      , Func<bool> predicate
      , CancellationToken token = default
  )
  ```

- `IDisposable` classes use the `Dispose(bool disposing)` + `GC.SuppressFinalize(this)` pattern.

### 12.2. Logging

- Log through `EncosyTower.Logging` (`StaticDevLogger`, `DevLogger`, `Logger`, ...). Never call `UnityEngine.Debug.Log*` directly.
- Log wrappers carry `[HideInCallstack, StackTraceHidden]` to stay out of user stack traces:

  ```csharp
  // Member of the owning module's static ThrowHelper:
  [HideInCallstack, StackTraceHidden, MethodImpl(MethodImplOptions.NoInlining)]
  internal static void LogInfo(object message)
  {
      StaticDevLogger.LogInfo(message);
  }
  ```

- Dev-only logging goes through the `Dev` loggers — they compile away in release builds.
- Standalone logging methods are cold paths — annotate with `[MethodImpl(NoInlining)]` (see Section 11).

### 12.3. Conditional compilation

- Guard async or platform-specific code at the file level (`#if UNITASK || UNITY_6000_0_OR_NEWER`).
- Define internal compile symbols at the top of the files that use them (see Section 6).
- Prefer `[Conditional("SYMBOL")]` on methods over wrapping every call site in `#if`.
- Platform-specific type aliases go at namespace scope.

## 13. Unsafe Code & Native Containers

- Every `unsafe` block gets a `// SAFETY:` comment directly above it. The comment explains why this specific operation is sound — not a generic sentence pasted everywhere.

  ```csharp
  // ❌ SHOULDN'T — inline block, trailing comment, generic copy-pasted text
  public void Add(T item) { CheckResizeWrite(); // SAFETY: The checked owner, allocator, or caller contract...
      unsafe { m_Data->Add(item); } }

  // ✅ SHOULD
  public void Add(T item)
  {
      CheckResizeWrite();

      // SAFETY: CheckResizeWrite validates the live header before the write.
      unsafe
      {
          m_Data->Add(item);
      }
  }
  ```

- `unsafe` blocks are always multi-line, brace on its own line, indented to match the surrounding code. Never one-liners.
- Method body order: safety check first, then the SAFETY comment, then the `unsafe` block.

  ```csharp
  public readonly int Count
  {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
  #if ENABLE_UNITY_COLLECTIONS_CHECKS
          AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
  #endif
          // SAFETY: The read check above validates the live header before reading its count.
          unsafe
          {
              return m_Data->Count;
          }
      }
  }
  ```

- Native wrapper structs carry these attributes and fields:

  ```csharp
  [StructLayout(LayoutKind.Sequential)]
  [NativeContainer]
  public partial struct NativeBuffer<T> : ...
      where T : unmanaged
  {
  #pragma warning disable IDE1006 // Naming Styles
      [NativeDisableUnsafePtrRestriction]
      internal unsafe ListUnsafe<T>* m_Data;

  #if ENABLE_UNITY_COLLECTIONS_CHECKS
      internal AtomicSafetyHandle m_Safety;

  #if UNITY_BURST
      private static readonly Unity.Burst.SharedStatic<int> s_SafetyId
          = Unity.Burst.SharedStatic<int>.GetOrCreate<NativeBuffer<T>>();
  #else
      private static int s_SafetyId;
  #endif
  #endif
  #pragma warning restore IDE1006 // Naming Styles
  ```

### 13.1. Planned C# memory-safety model (reference)

The .NET team's [Improving C# Memory Safety](https://devblogs.microsoft.com/dotnet/improving-csharp-memory-safety/#the-model-in-a-nutshell)
model is not available in Unity's C# version. It is reference material; the rules above stay the project rules. The
model points the same way:

- Every memory access must target live, allocated, initialized memory. In unsafe code the compiler cannot check this,
  so the author must.
- Every unsafe operation sits in an inner `unsafe { }` block. A member marked `unsafe` passes its obligations to its
  callers; a method that contains an inner block without marking itself `unsafe` is the boundary and must discharge
  those obligations with runtime guards, static reasoning or documented invariants. Its `// SAFETY:` comment says which.
- Unsafe members document their caller contract (the model uses a `/// <safety>` block).
- Only pointer dereferences are unsafe, not pointer types in signatures. Prefer typed pointers (`byte*`, or `void*` for
  truly opaque data) over `IntPtr`, `SafeHandle` for opaque handles, and document a genuinely native-sized `nint`.
- The model also forbids `unsafe` on types, delegates, static constructors and finalizers, adds a `safe` keyword for
  `extern` declarations, and lets `new()` match only a safe parameterless constructor.

## 14. Quick Checklist

Formatting & style:

- [ ] 4-space indent, LF endings, final newline, one statement per line.
- [ ] Braces on their own line; braces on ALL control blocks, no exceptions.
- [ ] Blank line around every block-scoped statement.
- [ ] One blank line before/after each multiline statement, with parent-boundary exceptions.
- [ ] Meaningful statement groups; no consecutive empty source lines or changed literal payloads.
- [ ] One blank line between adjacent property, method, and type declarations.
- [ ] Authored physical-file line length: keep new simple constructs inline through 120 characters; wrap past 120
      or for structural clarity. Normalize existing/Owner-approved wrapped layouts without opportunistic compaction,
      except logical chains and parameter/argument lists, which collapse whenever they fit.
      Generated output/snapshots have no width ceiling or width-driven wrapping.
- [ ] No column alignment; one space before `=`.
- [ ] Spaces after commas, around operators, in `where T : unmanaged`; each `where` on its own line.
- [ ] Method, local-function, and operator `=>` arrows go on the next line, indented one level.
      Property `=>` stays inline through 120 characters, then moves to a new indented line; inline
      accessor/lambda/switch-arm arrows are fine.
- [ ] Parameter/argument lists (declarations, calls, `new`, initializers): inline when the complete line fits within
      120 characters, even if previously wrapped; past 120, one top-level item per line with leading commas and a
      delimiter-only closing line, without splitting a method declaration prefix.
- [ ] Wrapped lists have one top-level item per line, including the first; inline single-argument wrappers remain
      valid. Use the documented layout baseline for indentation and align combined closing groups correctly.
- [ ] Inspect every multiline control condition in the owned review scope from opening to matching closing
      parenthesis: the outer `)` starts a delimiter-only line aligned with the control keyword, never attached
      to the last predicate. Include short two-predicate conditions and unchanged conditions inside reviewed
      members; distinguish inner call parentheses. Logical chains break between complete predicates.
      Lambda-call wrapping expands the argument list rather than moving a packed body to another line.
- [ ] Logical `&&`/`||` chains: one line when the complete line fits within 120 characters, even if previously
      wrapped; past 120, one top-level clause per line with a leading operator, never two clauses on one line.
- [ ] Compatible same-line string or interpolated-string concatenations are merged safely.
- [ ] Field groups ordered `const` → `static readonly` → `static` → instance; single-line fields adjacent,
  multiline attributed field blocks separated by exactly one blank line; serialized/transient state grouped.

Naming & files:

- [ ] Naming table respected; positional record properties PascalCase; consts and `goto` labels ALL_UPPER; native fields `m_PascalCase` with IDE1006 pragmas.
- [ ] One primary type per file; partials in `TypeName+Part.cs`; generics in `` TypeName`N.cs ``; one extension class per file.
- [ ] `Encosy<Type>Extensions` for extensions on BCL/Unity types.
- [ ] Local validation define blocks only where the existing inline preprocessor design needs them; match the policy owner and keep API/type availability wrappers outermost.
- [ ] Editor-only code in `Editor*` folders, `EncosyTower.Editor.*` namespace, `[ApiForEditor]`.
- [ ] Usings outside namespace, grouped `System*` → `EncosyTower*` → `Unity*`; block-scoped namespaces.

Code style:

- [ ] `== false` instead of `!` in conditions.
- [ ] No empty control blocks; flatten nested ifs; `goto FAILED` early-exit in parse/validate methods.
- [ ] `for` with invariant cached count where an indexer exists; preserve live state; `static` lambdas.
- [ ] Local functions, not delegate locals or capturing retained callbacks; inline static or unretained lambdas only.
- [ ] Derived strings composed on request from stored inputs; variant state as a PolyEnumStruct, not sentinel values.
- [ ] Calls with positional `true`/`false`, `null`, `default`, numeric literals or several discards name every argument.
- [ ] No nested ternaries; named domain predicates preserve short-circuiting and evaluation order.
- [ ] Shared-action fixed values use stacked switch cases; reusable semantic strings use named constants.
- [ ] Methods have coherent responsibilities; local/private extraction preserves lifetime, ref state and allocations.
- [ ] Member ordering per Section 7 (Unity types: serialized fields first).
- [ ] No new comments or XML docs unless the Project Owner asks (SAFETY, license headers, `#region` exempt); never touch existing comments.

Types & APIs:

- [ ] `readonly struct` for small values; immutable identifiers that preserve their API/layout contract; capability interfaces over ad-hoc members.
- [ ] `IEquatable<T>` pattern: Ordinal string compares, `HashCode.Combine`, operators delegate to `Equals`.
- [ ] `T item` / `in T item` overload pairs; `readonly` on non-mutating struct methods; `in` for readonly struct params on hot paths.
- [ ] Data records use positional declarations and explicit `record class`/`record struct`; bodies have concrete
      reasons. Check approved conversions for member-shape and constructor-order changes.
- [ ] No public API changes for style alone.
- [ ] Failures return `Option<T>` or `bool TryXxx(...)`, never `null`, never throw for expected failures.

Errors, performance & the rest:

- [ ] Validation via dedicated `ThrowIfXxx` / `ThrowXxx` helpers with `[HideInCallstack, StackTraceHidden]`, `[DoesNotReturnIf]`/`[DoesNotReturn]`, the applicable `ValidationDefines` `Conditional` set for dev-only, and cold `NoInlining` factories inside the module's static ThrowHelper. Reuse matching Core APIs first. No type-local error helpers, inline `ThrowHelper.ThrowIfFalse` or generic `Validate` helpers.
- [ ] `Try*` failures return `false`; use a named logging helper only when the API contract requires reporting, preserving ordinary silent misses.
- [ ] `[MethodImpl(AggressiveInlining)]` per Section 11; cold paths (`ThrowXxx`, `CreateException`, logging) get `NoInlining`.
- [ ] Async methods end with `Async`; `CancellationToken token = default` last; never swallow `OperationCanceledException`.
- [ ] Logging via `EncosyTower.Logging`, never `UnityEngine.Debug` directly; wrappers get `[HideInCallstack, StackTraceHidden]`.
- [ ] `// SAFETY:` comment above every `unsafe` block; block multi-line; comment specific; check → SAFETY → `unsafe` order.

## 15. Convention Review and Acceptance

Express convention rules as durable requirements, without depending on specific source files or commits.
Review these recurrence risks explicitly in owned new/rewritten code: event versus field naming; field grouping; unused imports;
ranges/indexes; redundant casts; flat guards; interpolation; callback identity/cleanup; Unity object validity;
and shared Editor loaders and asset paths.

Tests verify functionality, never conventions. A test executes the code under test (generator, analyzer, code fix,
runtime or Editor API, asset load) and asserts an observable result: emitted source, diagnostics, returned values,
state changes, thrown exceptions and messages, logs, or files and assets produced. Do not write or keep a test whose
only subject is how code is written or organized:
- source substrings or syntax-tree shape of authored implementation, such as which Printer calls or helpers a writer
  uses, where cancellation is checked, forbidden APIs, exact helper names or file line numbers;
- file, folder, namespace, assembly, solution or project layout, and csproj/asmdef/slnx/README text;
- reflection-only declaration checks: attribute presence (`[Conditional]`, `[HideInCallstack]`, `NoInlining`,
  `[ApiForEditor]`, `[SerializeField]`), member or overload inventories and counts, parameter names or defaults,
  constructor counts, setter presence, private field layout or IL shape;
- literal pins of constants, labels, menu paths or session keys, and copied UXML/USS/label text;
- test-suite bookkeeping, such as every generator having a contract provider or test classes matching folders.

A mixed test keeps its behavior assertions and drops its convention assertions. Enforce conventions through the
source/semantic review below, not through tests. Generated-output snapshots and semantic compiler tests remain valid
when emitted code or diagnostics are the actual contract. Do not recreate removed convention tests to satisfy stale
tasks. Keep native UI appearance proof separate.

1. **Before editing:** read these rules, identify owned members and generated consequences, and include the
   applicable requirements in worker briefs. Preserve existing owners, review assignments and task scope.
2. **Before handoff:** inspect complete new/substantially rewritten files. For a narrow edit, inspect the complete
   changed member, enclosing layout and affected declarations. New code cannot add convention debt; rewritten
   methods cannot retain these violations. Record pre-existing untouched debt without expanding a narrow rename
   into unrelated refactoring. A separately authorized cleanup owns that debt.
3. **Run deterministic text checks** on the exact authored file list with the installed `encosy-conventions`
   `scripts/audit_csharp_text.py` or an equivalent read-only tool: tabs, LF, final newline and line length.
   Exclude generated output/snapshots from line-length enforcement; generator implementations and authored tests
   remain subject to it. Check generated text separately for other conventions, including tabs, LF, final newline,
   indentation and structural layout, without introducing any line-length cap. A checker that skips generated
   files does not prove their conformance. Classify structural, literal and protected-comment exceptions precisely.
   Text checks do not prove
   grouping, delimiters, naming, constants, control flow, short-circuit safety or method responsibilities.
4. **Review source and semantics** for the remaining rules, using Roslyn where binding or extraction matters.
   Verify multiline-condition closing delimiters explicitly across the owned review scope, including conditions
   untouched by other repairs. A full-file read range, a generic formatting pass, or a passing text audit does
   not establish this check. Record it as checked only after inspecting the actual condition boundaries; leave
   it unverified if that inspection has not occurred. Do not infer structural compliance from nearby fixes.
   Existing formatter actions are editing aids, not acceptance proof. Follow project ownership rules; never run
   dotnet format against Unity-generated projects without the exact authorization required by project conventions.
5. **Record a compact receipt** in the existing task/review: candidate/file scope, text result, reviewer and rules
   checked, behavior checks, exact file/member/reason exceptions and unresolved findings. Newly introduced
   violations and violations retained in rewritten members prevent completion. A non-applicable screen finding
   is not a waived violation; compilation success, silence or an old baseline is not convention acceptance.
6. **Keep review proportional:** use existing assigned reviewers; otherwise use owner self-review and the established
   integration review. Validation-only work checks matching receipts and changed inputs without authoring cleanup,
   duplicating checks or adding agents. Later changes require current-candidate proof, not stale receipts.
7. **Separate proof layers:** formatting alone needs text/diff review, not new product tests. Refactoring needs
   matching existing behavior checks and justified missing regressions. Keep source, generated/artifact, build,
   Unity, behavior, native, allocation and player results separate. Unchanged UI needs no appearance gate or
   presentation timing/geometry tests. Add no analyzer package, formatter framework or CI job solely to claim
   enforcement; such automation needs its own authorized scope and its absence does not waive this review gate.

### 15.1. Exhaustive convention review when explicitly requested

- Review the entire convention document, including every normative subrule. The quick checklist is an index,
  not an exhaustive substitute. Formatting examples are calibration cases, not the scope of a full review.
- Build a rule-to-evidence checklist across naming; formatting; usings/namespaces; everyday control flow, loops,
  lambdas and constants; comments; files/defines/generated ownership; member ordering; type/API/record design;
  members/attributes/equality; errors/guards; performance annotations; async/logging/conditional compilation;
  unsafe/native constraints; and acceptance. Mark each applicable rule verified/fixed or give a specific reason
  it does not apply. A vague label such as "legacy", "generated" or "tests" is not a blanket exception.
- Inspect each owned file from its first line through EOF, including unchanged lines, inactive branches and
  relevant cross-member/type context. For large files use consecutive bounded reads with overlap; never count
  truncated/unseen output as reviewed. A search, formatter run, token comparison or compilation cannot substitute
  for full source review of rules it does not check.
- Keep a coverage record keyed to the exact final file hash: actual read line ranges, applicable-rule results,
  specific exceptions/findings, fixes and reviewer result. Verify full range coverage and re-read corrected files
  at their final hash. A subsequent edit invalidates that file's prior receipt.
- The owning task defines the full inventory and independent review. For an exhaustive request, do not infer
  unchanged/unflagged files compliant from sampling. Generated output loses only its 120-character requirement;
  review its other rules through actual output and its generator owner. Preserve intentional fixture payloads.
