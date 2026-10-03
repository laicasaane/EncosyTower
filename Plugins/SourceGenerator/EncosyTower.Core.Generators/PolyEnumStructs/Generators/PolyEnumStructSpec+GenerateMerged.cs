using EncosyTower.Core.Generators.EnumExtensions;

namespace EncosyTower.Core.Generators.PolyEnumStructs
{
    partial struct PolyEnumStructSpec
    {
        public struct StructRef
        {
            public StructSpec Value { get; set; }

            public Dictionary<string, string> FieldToMergedFieldMap { get; set; }

            public Dictionary<string, string> HiddenFieldToMergedFieldMap { get; set; }
        }

        public struct MergedFieldRef
        {
            public FieldSpec Value { get; set; }

            public string Name { get; set; }

            public sealed class SizeComparer : IComparer<MergedFieldRef>
            {
                public static readonly SizeComparer Default = new();

                public int Compare(MergedFieldRef x, MergedFieldRef y)
                {
                    return y.Value.size.CompareTo(x.Value.size);
                }
            }
        }

        public class MergedStructRef
        {
            public List<MergedFieldRef> FieldRefs { get; } = new();

            public Dictionary<PropertyDeclaration, bool> PropertyDimMap { get; } = new();

            public Dictionary<IndexerDeclaration, bool> IndexerDimMap { get; } = new();

            public Dictionary<MethodDeclaration, bool> MethodDimMap { get; } = new();

            public Dictionary<TypeSpec, Dictionary<ConstructionValue, StructId>> TypeValueToStructMap { get; } = new();

            public Dictionary<TypeSpec, HashSet<StructId>> TypeToStructsMap { get; } = new();

            public Dictionary<StructId, Dictionary<TypeSpec, List<ConstructionValue>>> StructToValuesMap { get; } = new();

            public int Size { get; set; }

            public int EnumCaseSize { get; set; }

            public int GetMaxCount()
                => Math.Max(Math.Max(PropertyDimMap.Count, IndexerDimMap.Count), MethodDimMap.Count);
        }

        public class PartialInterfaceRef
        {
            public List<PropertyDeclaration> Properties { get; } = new();

            public List<PropertyDeclaration> GenericProperties { get; } = new();

            public List<IndexerDeclaration> Indexers { get; } = new();

            public List<IndexerDeclaration> GenericIndexers { get; } = new();

            public List<MethodDeclaration> Methods { get; } = new();

            public List<MethodDeclaration> GenericMethods { get; } = new();
        }

        public class DimCollections
        {
            public List<PropertyDeclaration> Properties { get; } = new();

            public List<IndexerDeclaration> Indexers { get; } = new();

            public List<MethodDeclaration> Methods { get; } = new();
        }

        public class EnumCaseType
        {
            public string UnderlyingType { get; set; }

            public int MaxByteCount { get; set; }

            public List<EnumMemberSpec> Members { get; } = new();
        }

        private readonly void GenerateMerged(
              out List<StructRef> structRefs
            , out MergedStructRef mergedStructRef
            , out PartialInterfaceRef partialInterfaceRef
            , out string undefinedType
            , out EnumCaseType enumCaseType
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var structs = this.structs;
            var structCount = structs.Count;

            var enumCaseSize = (ulong)(structCount + 1) switch {
                > uint.MaxValue => 8,
                > ushort.MaxValue => 4,
                > byte.MaxValue => 2,
                _ => 1,
            };

            structRefs = new List<StructRef>(structCount);
            mergedStructRef = new MergedStructRef { Size = enumCaseSize, EnumCaseSize = enumCaseSize };
            partialInterfaceRef = new PartialInterfaceRef();
            enumCaseType = new EnumCaseType {
                UnderlyingType = (ulong)(structCount + 1) switch {
                    > uint.MaxValue => "ulong",
                    > ushort.MaxValue => "uint",
                    > byte.MaxValue => "ushort",
                    _ => "byte",
                },
            };

            mergedStructRef.FieldRefs.Add(new MergedFieldRef {
                Name = "enumCase",
                Value = new FieldSpec {
                    name = "enumCase",
                    returnType = new TypeSpec { name = "EnumCase", isEnum = true },
                    size = enumCaseSize,
                }
            });

            var propertySignatures = new HashSet<PropertySignature>();
            var indexerSignatures = new HashSet<IndexerSignature>();
            var methodSignatures = new HashSet<MethodSignature>();

            Aggregator.AggregateDimMap(
                  interfaceDef.properties
                , mergedStructRef.PropertyDimMap
                , propertySignatures
                , true
                , token
            );
            Aggregator.AggregateDimMap(
                  interfaceDef.indexers
                , mergedStructRef.IndexerDimMap
                , indexerSignatures
                , true
                , token
            );
            Aggregator.AggregateDimMap(
                  interfaceDef.methods
                , mergedStructRef.MethodDimMap
                , methodSignatures
                , true
                , token
            );
            Aggregator.AggregateDimMap(
                  genericInterfaceDef.properties
                , mergedStructRef.PropertyDimMap
                , propertySignatures
                , true
                , token
            );
            Aggregator.AggregateDimMap(
                  genericInterfaceDef.indexers
                , mergedStructRef.IndexerDimMap
                , indexerSignatures
                , true
                , token
            );
            Aggregator.AggregateDimMap(
                  genericInterfaceDef.methods
                , mergedStructRef.MethodDimMap
                , methodSignatures
                , true
                , token
            );

            var usedIndexesInList = new HashSet<int>();
            var fieldRefPool = new Queue<MergedFieldRef>();

            var propertyCountMap = new Dictionary<PropertyDeclaration, int>(structCount);
            var indexerCountMap = new Dictionary<IndexerDeclaration, int>(structCount);
            var methodCountMap = new Dictionary<MethodDeclaration, int>(structCount);
            var enumCaseMembers = enumCaseType.Members;
            var mergedStructSize = 0;
            var structNameByteCountMax = 0;

            enumCaseMembers.Add(new EnumMemberSpec {
                name = UNDEFINED_NAME,
                displayName = UNDEFINED_NAME,
                order = 0,
            });

            var lastStructIndex = structCount - 1;

            for (var i = 0; i < structCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var structRef = new StructRef() {
                    Value = structs[i],
                    FieldToMergedFieldMap = new(),
                    HiddenFieldToMergedFieldMap = new(),
                };

                var @struct = structRef.Value;

                Aggregator.AggregateMergedFieldRefs(
                      ref structRef
                    , mergedStructRef.FieldRefs
                    , usedIndexesInList
                    , fieldRefPool
                    , ref mergedStructSize
                    , token
                );

                Aggregator.AggregateCountMap(@struct.properties, propertyCountMap, propertySignatures, token);
                Aggregator.AggregateCountMap(@struct.indexers, indexerCountMap, indexerSignatures, token);
                Aggregator.AggregateCountMap(@struct.methods, methodCountMap, methodSignatures, token);
                Aggregator.AggregateConstructionMaps(@struct, mergedStructRef, token);

                if (i < lastStructIndex)
                {
                    structNameByteCountMax = Math.Max(
                          structNameByteCountMax
                        , Encoding.UTF8.GetByteCount(@struct.identifier)
                    );
                    enumCaseMembers.Add(new EnumMemberSpec {
                        name = @struct.identifier,
                        displayName = @struct.displayName,
                        order = (ulong)(i + 1),
                    });
                }

                structRefs.Add(structRef);
            }

            var maxCount = definedUndefinedStruct == DefinedUndefinedStruct.None
                ? structCount - 1
                : structCount;

            Aggregator.CopyMaxCount(
                  propertyCountMap
                , mergedStructRef.PropertyDimMap
                , partialInterfaceRef.Properties
                , maxCount
                , token
            );
            Aggregator.CopyMaxCount(
                  indexerCountMap
                , mergedStructRef.IndexerDimMap
                , partialInterfaceRef.Indexers
                , maxCount
                , token
            );
            Aggregator.CopyMaxCount(
                  methodCountMap
                , mergedStructRef.MethodDimMap
                , partialInterfaceRef.Methods
                , maxCount
                , token
            );
            MoveGenericProperties(partialInterfaceRef.Properties, partialInterfaceRef.GenericProperties);
            MoveGenericIndexers(partialInterfaceRef.Indexers, partialInterfaceRef.GenericIndexers);
            MoveGenericMethods(partialInterfaceRef.Methods, partialInterfaceRef.GenericMethods);

            undefinedType = definedUndefinedStruct switch {
                DefinedUndefinedStruct.Default => "Undefined",
                _ => $"{typeName}_Undefined",
            };

            enumCaseType.MaxByteCount = structNameByteCountMax;
            mergedStructRef.Size += mergedStructSize;

            if (sortFieldsBySize)
            {
                mergedStructRef.FieldRefs.Sort(MergedFieldRef.SizeComparer.Default);
                token.ThrowIfCancellationRequested();
            }

            return;

            static void MoveGenericProperties(
                  List<PropertyDeclaration> source
                , List<PropertyDeclaration> destination
            )
            {
                for (var i = source.Count - 1; i >= 0; i--)
                {
                    if (source[i].genericInterface)
                    {
                        destination.Insert(0, source[i]);
                        source.RemoveAt(i);
                    }
                }
            }

            static void MoveGenericIndexers(
                  List<IndexerDeclaration> source
                , List<IndexerDeclaration> destination
            )
            {
                for (var i = source.Count - 1; i >= 0; i--)
                {
                    if (source[i].genericInterface)
                    {
                        destination.Insert(0, source[i]);
                        source.RemoveAt(i);
                    }
                }
            }

            static void MoveGenericMethods(
                  List<MethodDeclaration> source
                , List<MethodDeclaration> destination
            )
            {
                for (var i = source.Count - 1; i >= 0; i--)
                {
                    if (source[i].genericInterface)
                    {
                        destination.Insert(0, source[i]);
                        source.RemoveAt(i);
                    }
                }
            }
        }

        internal readonly struct Aggregator
        {
            public static void AggregateMergedFieldRefs(
                  ref StructRef structRef
                , List<MergedFieldRef> mergedFieldRefs
                , HashSet<int> usedIndexesInList
                , Queue<MergedFieldRef> pool
                , ref int structSize
                , CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();
                usedIndexesInList.Clear();

                var fieldMergedFieldMap = structRef.FieldToMergedFieldMap;
                var parameters = structRef.Value.parameters;
                var parameterCount = parameters.Count;

                for (int i = 0; i < parameterCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    ref readonly var field = ref parameters[i].field;
                    fieldMergedFieldMap[field.name] = AddMergedField(
                          field
                        , mergedFieldRefs
                        , usedIndexesInList
                        , pool
                        , ref structSize
                        , token
                    );
                }

                var fields = structRef.Value.fields;
                var fieldCount = fields.Count;

                for (int i = 0; i < fieldCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    ref readonly var field = ref fields[i];
                    fieldMergedFieldMap[field.name] = AddMergedField(
                          field
                        , mergedFieldRefs
                        , usedIndexesInList
                        , pool
                        , ref structSize
                        , token
                    );
                }

                var hiddenFieldMergedFieldMap = structRef.HiddenFieldToMergedFieldMap;
                var hiddenFields = structRef.Value.hiddenFields;
                var hiddenFieldCount = hiddenFields.Count;

                for (int i = 0; i < hiddenFieldCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    ref readonly var field = ref hiddenFields[i];
                    hiddenFieldMergedFieldMap[field.name] = AddMergedField(
                          field
                        , mergedFieldRefs
                        , usedIndexesInList
                        , pool
                        , ref structSize
                        , token
                    );
                }
            }

            private static string AddMergedField(
                  in FieldSpec field
                , List<MergedFieldRef> mergedFieldRefs
                , HashSet<int> usedIndexesInList
                , Queue<MergedFieldRef> pool
                , ref int structSize
                , CancellationToken token
            )
            {
                var matchingListIndex = -1;
                var mergedFieldRefCount = mergedFieldRefs.Count;

                for (int mergedIndex = 0; mergedIndex < mergedFieldRefCount; mergedIndex++)
                {
                    token.ThrowIfCancellationRequested();
                    if (usedIndexesInList.Contains(mergedIndex) == false)
                    {
                        if (field.returnType.Equals(mergedFieldRefs[mergedIndex].Value.returnType))
                        {
                            matchingListIndex = mergedIndex;
                            break;
                        }
                    }
                }

                MergedFieldRef mergedField;

                if (matchingListIndex < 0)
                {
                    int newListIndex = mergedFieldRefs.Count;

                    if (pool.Count > 0)
                    {
                        mergedField = pool.Dequeue();
                    }
                    else
                    {
                        mergedField = new MergedFieldRef();
                    }

                    mergedField.Value = field;
                    mergedField.Name = $"field_{field.returnType.identifier}_{newListIndex}";

                    structSize += field.size;
                    mergedFieldRefs.Add(mergedField);
                    usedIndexesInList.Add(newListIndex);
                }
                else
                {
                    mergedField = mergedFieldRefs[matchingListIndex];
                    usedIndexesInList.Add(matchingListIndex);
                }

                return mergedField.Name;
            }

            public static void AggregateCountMap<TDef, TSig>(
                  EquatableArray<TDef> items
                , Dictionary<TDef, int> countMap
                , HashSet<TSig> ignoredSignatures
                , CancellationToken token
            )
                where TDef : struct, IEquatable<TDef>, ICast<TSig>
                where TSig : struct, IEquatable<TSig>
            {
                token.ThrowIfCancellationRequested();
                var itemCount = items.Count;

                for (var i = 0; i < itemCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    ref readonly var item = ref items[i];

                    if (ignoredSignatures.Contains(item.Cast()))
                    {
                        continue;
                    }

                    if (countMap.TryGetValue(item, out var count) == false)
                    {
                        count = 0;
                    }

                    countMap[item] = count + 1;
                }
            }

            public static void AggregateDimMap<TDef, TSig>(
                  EquatableArray<TDef> items
                , Dictionary<TDef, bool> dimMap
                , HashSet<TSig> signatures
                , bool alwaysAddToSignature
                , CancellationToken token
            )
                where TDef : struct, IEquatable<TDef>, ICloneWithDim<TDef>, ICast<TSig>
                where TSig : struct, IEquatable<TSig>, ICloneWithDim<TSig>
            {
                token.ThrowIfCancellationRequested();
                var itemCount = items.Count;

                for (var i = 0; i < itemCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    ref readonly var item = ref items[i];
                    var isDim = item.IsDim;
                    var cloned = item.Clone(false);

                    if (dimMap.ContainsKey(cloned) == false)
                    {
                        dimMap[cloned] = isDim;
                    }

                    if (isDim || alwaysAddToSignature)
                    {
                        signatures.Add(item.Cast());
                    }
                }
            }

            public static void CopyMaxCount<T>(
                  Dictionary<T, int> countMap
                , Dictionary<T, bool> dimMap
                , List<T> list
                , int maxCount
                , CancellationToken token
            )
                where T : struct, IEquatable<T>, ICloneWithDim<T>
            {
                token.ThrowIfCancellationRequested();
                foreach (var kv in countMap)
                {
                    token.ThrowIfCancellationRequested();
                    if (kv.Value == maxCount)
                    {
                        var item = kv.Key;
                        var cloned = item.Clone(false);

                        if (dimMap.ContainsKey(cloned) == false)
                        {
                            dimMap[cloned] = item.IsDim;
                            list.Add(cloned);
                        }
                    }
                }
            }

            public static void AggregateConstructionMaps(
                  StructSpec def
                , MergedStructRef mergedStructRef
                , CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();
                var typeValueToStructMap = mergedStructRef.TypeValueToStructMap;
                var typeToStructsMap = mergedStructRef.TypeToStructsMap;
                var structToValuesMap = mergedStructRef.StructToValuesMap;
                var constructions = def.constructions.AsReadOnlySpan();

                foreach (var construction in constructions)
                {
                    token.ThrowIfCancellationRequested();
                    var type = construction.type;
                    var value = construction.value;
                    var structId = new StructId {
                        name = def.name,
                        identifier = def.identifier,
                    };

                    if (typeValueToStructMap.TryGetValue(type, out var valueToStruct) == false)
                    {
                        typeValueToStructMap[type] = valueToStruct = new();
                    }

                    if (typeToStructsMap.TryGetValue(type, out var structs) == false)
                    {
                        typeToStructsMap[type] = structs = new();
                    }

                    if (structToValuesMap.TryGetValue(structId, out var typeToValues) == false)
                    {
                        structToValuesMap[structId] = typeToValues = new();
                    }

                    if (valueToStruct.ContainsKey(value) == false)
                    {
                        valueToStruct[value] = structId;
                    }

                    structs.Add(structId);

                    if (typeToValues.TryGetValue(type, out var values) == false)
                    {
                        typeToValues[type] = values = new();
                    }

                    values.Add(value);
                }
            }
        }
    }
}
