using EncosyTower.SourceGen.Data.Helpers;
using static EncosyTower.Databases.Authoring.Generators.Helpers;

namespace EncosyTower.Databases.Authoring.Generators
{
    partial struct DatabaseSpec
    {
        private static void ExtractConverterAttributes(
              INamedTypeSymbol authoringTypeSymbol
            , List<ConverterForTableEntry> cfgTableEntries
            , List<ConverterForDataPropertyEntry> cfgPropEntries
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var attrib in authoringTypeSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attribClass = attrib.AttributeClass;

                if (attribClass == null)
                {
                    continue;
                }

                if (attribClass.HasFullName(CONVERTER_FOR_TABLE_ATTRIBUTE, token))
                {
                    var args = attrib.ConstructorArguments;

                    if (args.Length != 2
                        || args[1].Value is not INamedTypeSymbol converterType
                        || TryGetConvertMethod(converterType, token, out var convertMethod) == false
                    )
                    {
                        continue;
                    }

                    var spec = MakeConverterModelFromMethod(
                          converterType
                        , convertMethod
                        , ignoredTypes
                        , resultTypes
                        , token
                    );

                    string tableName = null;
                    INamedTypeSymbol tableType = null;

                    if (args[0].Value is string nameValue)
                    {
                        tableName = nameValue;
                    }
                    else if (args[0].Value is INamedTypeSymbol typeValue)
                    {
                        tableType = typeValue;
                    }
                    else
                    {
                        continue;
                    }

                    cfgTableEntries.Add(new ConverterForTableEntry {
                        tableName = tableName,
                        tableType = tableType,
                        converter = spec,
                        targetTypeFullName = spec.destType.fullName,
                    });

                    continue;
                }

                if (attribClass.HasFullName(CONVERTER_FOR_DATA_PROPERTY_ATTRIBUTE, token))
                {
                    var args = attrib.ConstructorArguments;

                    if (args.Length < 3
                        || args[0].Value is not INamedTypeSymbol dataType
                        || args[1].Value is not string propertyName
                        || string.IsNullOrWhiteSpace(propertyName)
                        || args[2].Value is not INamedTypeSymbol converterType
                        || TryGetConvertMethod(converterType, token, out var convertMethod) == false
                    )
                    {
                        continue;
                    }

                    var spec = MakeConverterModelFromMethod(
                          converterType
                        , convertMethod
                        , ignoredTypes
                        , resultTypes
                        , token
                    );

                    string tableName = null;
                    INamedTypeSymbol tableType = null;

                    if (args.Length >= 4)
                    {
                        if (args[3].Value is string nameValue)
                        {
                            tableName = nameValue;
                        }
                        else if (args[3].Value is INamedTypeSymbol typeValue)
                        {
                            tableType = typeValue;
                        }
                    }

                    cfgPropEntries.Add(new ConverterForDataPropertyEntry {
                        dataTypeFullName = dataType.ToFullName(),
                        propertyName = propertyName,
                        tableName = tableName,
                        tableType = tableType,
                        converter = spec,
                    });
                }
            }
        }

        private static void ResolveConverters(
              List<TableInfo> tableInfoList
            , Dictionary<string, DataSpec> dataMap
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> databaseMap
            , List<ConverterForTableEntry> cfgTableEntries
            , List<ConverterForDataPropertyEntry> cfgPropEntries
            , ref ImmutableArrayBuilder<ScopedConverterSpec> scopedConvertersBuilder
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var added = new HashSet<ScopedDedupKey>();
            var sb = new StringBuilder(1024);

            for (var t = 0; t < tableInfoList.Count; t++)
            {
                token.ThrowIfCancellationRequested();

                var tableInfo = tableInfoList[t];

                var cfgTable = BuildScopedTableMap(cfgTableEntries, tableInfo);
                var cfgProp = BuildScopedPropMap(cfgPropEntries, tableInfo);

                var temp = new List<ScopedConverterSpec>();
                var nested = new List<string>();
                var visited = new HashSet<string>(StringComparer.Ordinal);
                var queue = new Queue<string>();

                EnqueueRoot(tableInfo.idTypeFullName, dataMap, visited, queue);
                EnqueueRoot(tableInfo.dataTypeFullName, dataMap, visited, queue);

                while (queue.Count > 0)
                {
                    token.ThrowIfCancellationRequested();

                    var full = queue.Dequeue();

                    if (dataMap.TryGetValue(full, out var shape) == false)
                    {
                        continue;
                    }

                    if (string.Equals(full, tableInfo.idTypeFullName, StringComparison.Ordinal) == false
                        && string.Equals(full, tableInfo.dataTypeFullName, StringComparison.Ordinal) == false
                    )
                    {
                        nested.Add(full);
                    }

                    ResolveTypeMembers(
                          shape.fullName
                        , shape.propRefs
                        , cfgProp
                        , cfgTable
                        , authorDbMap
                        , tableInfo.tableConverterMap
                        , databaseMap
                        , dataMap
                        , visited
                        , queue
                        , temp
                        , token
                     );

                    ResolveTypeMembers(
                          shape.fullName
                        , shape.fieldRefs
                        , cfgProp
                        , cfgTable
                        , authorDbMap
                        , tableInfo.tableConverterMap
                        , databaseMap
                        , dataMap
                        , visited
                        , queue
                        , temp
                        , token
                    );

                    foreach (var layer in shape.baseTypeRefs)
                    {
                        ResolveTypeMembers(
                              layer.fullName
                            , layer.propRefs
                            , cfgProp
                            , cfgTable
                            , authorDbMap
                            , tableInfo.tableConverterMap
                            , databaseMap
                            , dataMap
                            , visited
                            , queue
                            , temp
                            , token
                        );

                        ResolveTypeMembers(
                              layer.fullName
                            , layer.fieldRefs
                            , cfgProp
                            , cfgTable
                            , authorDbMap
                            , tableInfo.tableConverterMap
                            , databaseMap
                            , dataMap
                            , visited
                            , queue
                            , temp
                            , token
                        );
                    }
                }

                var scopeKey = ComputeScopeKey(temp, sb, token);
                var horizontalCollections = ResolveHorizontalCollections(
                      tableInfo.horizontalSelections
                    , dataMap
                    , visited
                    , temp
                    , token
                );
                var generatedKeyTypeFullNames = CollectGeneratedKeyTypeFullNames(
                      tableInfo.idTypeFullName
                    , dataMap
                    , visited
                    , temp
                    , token
                );

                tableInfo.scopeKey = scopeKey;
                tableInfo.converterEntries = temp.ToImmutableArray().AsEquatableArray();
                tableInfo.nestedDataTypeFullNames = ToEquatableArray(nested, token);
                tableInfo.horizontalCollections = horizontalCollections;
                tableInfo.generatedKeyTypeFullNames = generatedKeyTypeFullNames;
                tableInfoList[t] = tableInfo;

                foreach (var entry in temp)
                {
                    token.ThrowIfCancellationRequested();

                    var dedupKey = new ScopedDedupKey {
                        scopeKey = scopeKey,
                        declaringDataTypeFullName = entry.declaringDataTypeFullName,
                        propertyName = entry.propertyName,
                    };

                    if (added.Add(dedupKey))
                    {
                        var scopedConverter = entry;
                        scopedConverter.scopeKey = scopeKey;
                        scopedConvertersBuilder.Add(scopedConverter);
                    }
                }
            }
        }

        private static void ResolveTypeMembers(
              string declaringTypeFullName
            , EquatableArray<MemberSpec> members
            , Dictionary<string, ConverterSpec> cfgProp
            , Dictionary<string, ConverterSpec> cfgTable
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> tableMap
            , Dictionary<string, ConverterSpec> databaseMap
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> visited
            , Queue<string> queue
            , List<ScopedConverterSpec> temp
            , CancellationToken token
        )
        {
            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                ResolveMemberConverter(
                      declaringTypeFullName
                    , member
                    , cfgProp
                    , cfgTable
                    , authorDbMap
                    , tableMap
                    , databaseMap
                    , out var converter
                    , out var sheetConverter
                );

                temp.Add(new ScopedConverterSpec {
                    declaringDataTypeFullName = declaringTypeFullName,
                    propertyName = member.propertyName,
                    converter = converter,
                    sheetConverter = sheetConverter,
                });

                var resolvedMember = member;
                resolvedMember.converter = converter;

                var manualAuthoring = member.manualAuthoring;

                if (manualAuthoring.defined && manualAuthoring.type.IsValid)
                {
                    EnqueueSheetSideType(manualAuthoring.collection, manualAuthoring.type, dataMap, visited, queue);
                }

                EnqueueSheetSideType(
                      resolvedMember.SelectCollection()
                    , resolvedMember.SelectType()
                    , dataMap
                    , visited
                    , queue
                );
            }
        }

        private static void ResolveMemberConverter(
              string declaringTypeFullName
            , MemberSpec member
            , Dictionary<string, ConverterSpec> cfgProp
            , Dictionary<string, ConverterSpec> cfgTable
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> tableMap
            , Dictionary<string, ConverterSpec> databaseMap
            , out ConverterSpec converter
            , out ConverterSpec sheetConverter
        )
        {
            converter = default;
            sheetConverter = default;

            var targetTypeFullName = member.type.fullName;
            var propKey = ScopedConverterSpec.MakeMemberKey(declaringTypeFullName, member.propertyName);

            if (cfgProp.TryGetValue(propKey, out var c1))
            {
                converter = c1;
            }
            else if (cfgTable.TryGetValue(targetTypeFullName, out var c2))
            {
                converter = c2;
            }
            else if (authorDbMap.TryGetValue(targetTypeFullName, out var c3))
            {
                converter = c3;
            }
            else if (member.memberConverter.kind != ConverterKind.None)
            {
                converter = member.memberConverter;
            }
            else if (tableMap.TryGetValue(targetTypeFullName, out var c5))
            {
                converter = c5;
            }
            else if (databaseMap.TryGetValue(targetTypeFullName, out var c6))
            {
                converter = c6;
            }
            else if (member.localConverter.kind != ConverterKind.None)
            {
                converter = member.localConverter;
            }

            if (member.collection.kind == CollectionKind.NotCollection
                && converter.kind != ConverterKind.None
            )
            {
                if (IsStringSourceConverter(converter))
                {
                    sheetConverter = converter;
                    converter = default;
                }
                else if (converter.sourceCollection.kind == CollectionKind.NotCollection
                    && TryGetStringSourceConverterScoped(
                          converter.sourceType.fullName
                        , cfgTable
                        , authorDbMap
                        , tableMap
                        , databaseMap
                        , out var sourceConverter
                    )
                )
                {
                    sheetConverter = sourceConverter;
                }
            }
        }

        private static bool TryGetStringSourceConverterScoped(
              string targetTypeFullName
            , Dictionary<string, ConverterSpec> cfgTable
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> tableMap
            , Dictionary<string, ConverterSpec> databaseMap
            , out ConverterSpec converter
        )
        {
            if (cfgTable.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            if (authorDbMap.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            if (tableMap.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            if (databaseMap.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            converter = default;
            return false;
        }

        private static Dictionary<string, ConverterSpec> BuildScopedTableMap(
              List<ConverterForTableEntry> entries
            , in TableInfo tableInfo
        )
        {
            var map = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                if (MatchesTable(entry.tableName, entry.tableType, tableInfo) == false)
                {
                    continue;
                }

                if (map.ContainsKey(entry.targetTypeFullName) == false)
                {
                    map[entry.targetTypeFullName] = entry.converter;
                }
            }

            return map;
        }

        private static Dictionary<string, ConverterSpec> BuildScopedPropMap(
              List<ConverterForDataPropertyEntry> entries
            , in TableInfo tableInfo
        )
        {
            var map = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                if (entry.tableName != null || entry.tableType != null)
                {
                    continue;
                }

                var key = ScopedConverterSpec.MakeMemberKey(entry.dataTypeFullName, entry.propertyName);

                if (map.ContainsKey(key) == false)
                {
                    map[key] = entry.converter;
                }
            }

            foreach (var entry in entries)
            {
                if (entry.tableName == null && entry.tableType == null)
                {
                    continue;
                }

                if (MatchesTable(entry.tableName, entry.tableType, tableInfo) == false)
                {
                    continue;
                }

                var key = ScopedConverterSpec.MakeMemberKey(entry.dataTypeFullName, entry.propertyName);
                map[key] = entry.converter;
            }

            return map;
        }

        private static bool MatchesTable(string tableName, INamedTypeSymbol tableType, in TableInfo tableInfo)
        {
            if (tableName != null)
            {
                return string.Equals(tableName, tableInfo.propertyName, StringComparison.Ordinal);
            }

            if (tableType != null)
            {
                return SymbolEqualityComparer.Default.Equals(tableType, tableInfo.tableType);
            }

            return false;
        }

        private static void EnqueueRoot(
              string typeFullName
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> visited
            , Queue<string> queue
        )
        {
            if (string.IsNullOrEmpty(typeFullName))
            {
                return;
            }

            if (dataMap.ContainsKey(typeFullName) && visited.Add(typeFullName))
            {
                queue.Enqueue(typeFullName);
            }
        }

        private static void EnqueueSheetSideType(
              CollectionSpec collection
            , TypeSpec type
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> visited
            , Queue<string> queue
        )
        {
            if (collection.kind == CollectionKind.Dictionary)
            {
                EnqueueArgument(collection.keyType, collection.argumentCollections, 0, dataMap, visited, queue);
                EnqueueArgument(collection.elementType, collection.argumentCollections, 1, dataMap, visited, queue);
            }
            else if (collection.kind != CollectionKind.NotCollection)
            {
                EnqueueArgument(collection.elementType, collection.argumentCollections, 0, dataMap, visited, queue);
            }
            else
            {
                TryEnqueue(type.fullName, dataMap, visited, queue);
            }

            static void EnqueueArgument(
                  TypeSpec argumentType
                , CollectionSpecArray argumentCollections
                , int index
                , Dictionary<string, DataSpec> dataMap
                , HashSet<string> visited
                , Queue<string> queue
            )
            {
                var argumentCollection = index < argumentCollections.Count ? argumentCollections[index] : default;

                if (argumentCollection.kind == CollectionKind.NotCollection)
                {
                    TryEnqueue(argumentType.fullName, dataMap, visited, queue);
                    return;
                }

                EnqueueSheetSideType(argumentCollection, argumentType, dataMap, visited, queue);
            }
        }

        private static EquatableArray<HorizontalCollectionSpec> ResolveHorizontalCollections(
              List<HorizontalSelectionInfo> selections
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> reachableTypes
            , List<ScopedConverterSpec> converters
            , CancellationToken token
        )
        {
            var map = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);

            for (var i = 0; i < selections.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                var selection = selections[i];

                if (selection.validTarget == false
                    || TryFindReachableMember(
                      selection.targetTypeFullName
                    , selection.propertyName
                    , dataMap
                    , reachableTypes
                    , out var member
                    , out var declaringTypeFullName
                    ) == false
                )
                {
                    continue;
                }

                ApplyConverter(declaringTypeFullName, ref member, converters);

                var effectiveCollection = member.manualAuthoring.defined
                    ? member.manualAuthoring.collection
                    : member.SelectCollection();

                if (effectiveCollection.kind == CollectionKind.NotCollection)
                {
                    continue;
                }

                if (map.TryGetValue(selection.targetTypeFullName, out var propertyNames) == false)
                {
                    map[selection.targetTypeFullName] = propertyNames = new(StringComparer.Ordinal);
                }

                propertyNames.Add(selection.propertyName);
            }

            using var builder = ImmutableArrayBuilder<HorizontalCollectionSpec>.Rent();

            foreach (var pair in map)
            {
                builder.Add(new HorizontalCollectionSpec {
                    targetTypeFullName = pair.Key,
                    propertyNames = pair.Value.ToImmutableArray().AsEquatableArray(),
                });
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private static EquatableArray<string> CollectGeneratedKeyTypeFullNames(
              string idTypeFullName
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> reachableTypes
            , List<ScopedConverterSpec> converters
            , CancellationToken token
        )
        {
            var result = new SortedSet<string>(StringComparer.Ordinal);

            if (dataMap.ContainsKey(idTypeFullName))
            {
                result.Add(idTypeFullName);
            }

            foreach (var typeFullName in reachableTypes)
            {
                token.ThrowIfCancellationRequested();

                if (dataMap.TryGetValue(typeFullName, out var data) == false)
                {
                    continue;
                }

                CollectDictionaryKeyTypes(data.fullName, data.propRefs);
                CollectDictionaryKeyTypes(data.fullName, data.fieldRefs);

                foreach (var baseType in data.baseTypeRefs)
                {
                    CollectDictionaryKeyTypes(baseType.fullName, baseType.propRefs);
                    CollectDictionaryKeyTypes(baseType.fullName, baseType.fieldRefs);
                }
            }

            var queue = new Queue<string>(result);

            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                if (dataMap.TryGetValue(queue.Dequeue(), out var data) == false)
                {
                    continue;
                }

                CollectComponentTypes(data.fullName, data.propRefs);
                CollectComponentTypes(data.fullName, data.fieldRefs);

                foreach (var baseType in data.baseTypeRefs)
                {
                    CollectComponentTypes(baseType.fullName, baseType.propRefs);
                    CollectComponentTypes(baseType.fullName, baseType.fieldRefs);
                }
            }

            return result.ToImmutableArray().AsEquatableArray();

            void CollectDictionaryKeyTypes(string declaringTypeFullName, EquatableArray<MemberSpec> members)
            {
                foreach (var sourceMember in members)
                {
                    var member = sourceMember;
                    ApplyConverter(declaringTypeFullName, ref member, converters);
                    CollectDictionaryKeys(member.SelectType(), member.SelectCollection());
                }
            }

            void CollectDictionaryKeys(TypeSpec type, CollectionSpec collection)
            {
                if (collection.kind == CollectionKind.NotCollection)
                {
                    return;
                }

                if (collection.kind == CollectionKind.Dictionary)
                {
                    var keyCollection = GetArgumentCollection(collection, 0);

                    if (keyCollection.kind == CollectionKind.NotCollection
                        && dataMap.ContainsKey(collection.keyType.fullName)
                    )
                    {
                        result.Add(collection.keyType.fullName);
                    }

                    CollectDictionaryKeys(collection.keyType, keyCollection);
                    CollectDictionaryKeys(collection.elementType, GetArgumentCollection(collection, 1));
                    return;
                }

                CollectDictionaryKeys(collection.elementType, GetArgumentCollection(collection, 0));
            }

            void CollectComponentTypes(string declaringTypeFullName, EquatableArray<MemberSpec> members)
            {
                foreach (var sourceMember in members)
                {
                    var member = sourceMember;
                    ApplyConverter(declaringTypeFullName, ref member, converters);
                    CollectDataLeaves(member.SelectType(), member.SelectCollection());
                }
            }

            void CollectDataLeaves(TypeSpec type, CollectionSpec collection)
            {
                if (collection.kind == CollectionKind.NotCollection)
                {
                    if (dataMap.ContainsKey(type.fullName) && result.Add(type.fullName))
                    {
                        queue.Enqueue(type.fullName);
                    }

                    return;
                }

                if (collection.kind == CollectionKind.Dictionary)
                {
                    CollectDataLeaves(collection.keyType, GetArgumentCollection(collection, 0));
                    CollectDataLeaves(collection.elementType, GetArgumentCollection(collection, 1));
                    return;
                }

                CollectDataLeaves(collection.elementType, GetArgumentCollection(collection, 0));
            }
        }

        private static CollectionSpec GetArgumentCollection(CollectionSpec collection, int index)
            => index < collection.argumentCollections.Count ? collection.argumentCollections[index] : default;

        private static bool TryFindReachableMember(
              string targetTypeFullName
            , string propertyName
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> reachableTypes
            , out MemberSpec member
            , out string declaringTypeFullName
        )
        {
            foreach (var reachableType in reachableTypes)
            {
                if (dataMap.TryGetValue(reachableType, out var data) == false)
                {
                    continue;
                }

                if (string.Equals(data.fullName, targetTypeFullName, StringComparison.Ordinal)
                    && TryFindMember(data.propRefs, data.fieldRefs, propertyName, out member)
                )
                {
                    declaringTypeFullName = data.fullName;
                    return true;
                }

                foreach (var baseType in data.baseTypeRefs)
                {
                    if (string.Equals(baseType.fullName, targetTypeFullName, StringComparison.Ordinal)
                        && TryFindMember(baseType.propRefs, baseType.fieldRefs, propertyName, out member)
                    )
                    {
                        declaringTypeFullName = baseType.fullName;
                        return true;
                    }
                }
            }

            member = default;
            declaringTypeFullName = default;
            return false;
        }

        private static bool TryFindMember(
              EquatableArray<MemberSpec> properties
            , EquatableArray<MemberSpec> fields
            , string propertyName
            , out MemberSpec member
        )
        {
            foreach (var candidate in properties)
            {
                if (string.Equals(candidate.propertyName, propertyName, StringComparison.Ordinal))
                {
                    member = candidate;
                    return true;
                }
            }

            foreach (var candidate in fields)
            {
                if (string.Equals(candidate.propertyName, propertyName, StringComparison.Ordinal))
                {
                    member = candidate;
                    return true;
                }
            }

            member = default;
            return false;
        }

        private static void ApplyConverter(
              string declaringTypeFullName
            , ref MemberSpec member
            , List<ScopedConverterSpec> converters
        )
        {
            for (var i = 0; i < converters.Count; i++)
            {
                var converter = converters[i];

                if (string.Equals(converter.declaringDataTypeFullName, declaringTypeFullName, StringComparison.Ordinal)
                    && string.Equals(converter.propertyName, member.propertyName, StringComparison.Ordinal)
                )
                {
                    member.converter = converter.converter;
                    member.sheetConverter = converter.sheetConverter;
                    return;
                }
            }
        }

        private static HashValue64 ComputeScopeKey(
              List<ScopedConverterSpec> entries
            , StringBuilder builder
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (entries.Count < 1)
            {
                return HashValue64.FNV1a(string.Empty);
            }

            entries.Sort(static (a, b) =>
            {
                var compare = string.CompareOrdinal(a.declaringDataTypeFullName, b.declaringDataTypeFullName);
                return compare != 0 ? compare : string.CompareOrdinal(a.propertyName, b.propertyName);
            });

            builder.Clear();

            foreach (var entry in entries)
            {
                token.ThrowIfCancellationRequested();

                builder.Append(entry.declaringDataTypeFullName).Append(".").Append(entry.propertyName).Append('=');

                AppendConverterSignature(builder, entry.converter);

                builder.Append('|');

                AppendConverterSignature(builder, entry.sheetConverter);

                builder.Append('\n');
            }

            return HashValue64.FNV1a(builder.ToString());

            static void AppendConverterSignature(StringBuilder builder, in ConverterSpec converter)
            {
                builder.Append((int)converter.kind).Append(':')
                    .Append(converter.converterTypeFullName).Append(':')
                    .Append(converter.sourceType.fullName).Append(':')
                    .Append(GetCollectionSignature(converter.sourceCollection)).Append(':')
                    .Append(converter.destType.fullName);
            }

            static string GetCollectionSignature(CollectionSpec collection)
            {
                var result = new StringBuilder();
                AppendCollection(ref result, collection);
                return result.ToString();

                static void AppendCollection(ref StringBuilder result, CollectionSpec value)
                {
                    result.Append((int)value.kind).Append('(').Append(value.keyType.fullName).Append('|')
                        .Append(value.elementType.fullName);

                    foreach (var child in value.argumentCollections)
                    {
                        result.Append(',');
                        AppendCollection(ref result, child);
                    }

                    result.Append(')');
                }
            }
        }

        private static EquatableArray<string> ToEquatableArray(List<string> values, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (values.Count < 1)
            {
                return default;
            }

            using var builder = ImmutableArrayBuilder<string>.Rent();

            for (var i = 0; i < values.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                builder.Add(values[i]);
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private struct ConverterForTableEntry
        {
            public string tableName;
            public INamedTypeSymbol tableType;
            public ConverterSpec converter;
            public string targetTypeFullName;
        }

        private struct ConverterForDataPropertyEntry
        {
            public string dataTypeFullName;
            public string propertyName;
            public string tableName;
            public INamedTypeSymbol tableType;
            public ConverterSpec converter;
        }

        private struct ScopedDedupKey : IEquatable<ScopedDedupKey>
        {
            public HashValue64 scopeKey;
            public string declaringDataTypeFullName;
            public string propertyName;

            public readonly override int GetHashCode()
                => HashValue.Combine(scopeKey, declaringDataTypeFullName, propertyName);

            public readonly override bool Equals(object obj)
                => obj is ScopedDedupKey other && Equals(other);

            public readonly bool Equals(ScopedDedupKey other)
                => scopeKey == other.scopeKey
                && string.Equals(declaringDataTypeFullName, other.declaringDataTypeFullName, StringComparison.Ordinal)
                && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
                ;
        }
    }
}
