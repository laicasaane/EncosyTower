using System;
using System.Collections.Generic;
using System.Reflection;
using Cathei.BakingSheet;
using EncosyTower.Common;
using EncosyTower.Databases.Authoring.SourceGen;
using EncosyTower.Naming;

namespace EncosyTower.Databases.Authoring
{
    public class NamingMap
    {
        private readonly Dictionary<string, string> _serializedNameToProperName = new();
        private readonly Dictionary<string, string> _properNameToSerializedName = new();
        private readonly NameCasing _strategy;

        public NamingMap(NameCasing strategy)
        {
            _strategy = strategy;
        }

        public void AddProperName(string properName)
        {
            AddProperName(properName, preserveProperNameAlias: false);
        }

        private void AddProperName(string properName, bool preserveProperNameAlias)
        {
            if (properName.IsEmptyOrWhiteSpace())
            {
                return;
            }

            var serializedName = _strategy.ConvertName(properName);
            _serializedNameToProperName[serializedName] = properName;
            _properNameToSerializedName[properName] = serializedName;

            if (preserveProperNameAlias)
            {
                _serializedNameToProperName[properName] = properName;
            }
        }

        public bool Validate(string properName, string serializedName)
        {
            if (_properNameToSerializedName.TryGetValue(properName, out var value))
            {
                return value == serializedName;
            }

            return false;
        }

        public string GetSerializedName(string properName)
            => _properNameToSerializedName.GetValueOrDefault(properName, properName);

        public string GetProperName(string serializedName)
            => _serializedNameToProperName.GetValueOrDefault(serializedName, serializedName);

        internal static NamingMap Create(ISheet sheet, string sheetName, NameCasing nameCasing)
        {
            var map = new NamingMap(nameCasing);
            var uniqueTypes = new HashSet<Type>();
            var typeQueue = new Queue<Type>();

            AddType(sheet.RowType);

            map.AddProperName(sheet.GetType().Name);
            map.AddProperName(sheetName);
            map.AddProperName(SheetTokens.Dictionary.Header.Key, preserveProperNameAlias: true);
            map.AddProperName(SheetTokens.Dictionary.Header.Value, preserveProperNameAlias: true);

            while (typeQueue.TryDequeue(out var type))
            {
                foreach (var property in SheetTokens.GetEligibleProperties(type))
                {
                    map.AddProperName(property.Name);
                    AddPropertyTypes(property.PropertyType);
                }
            }

            return map;

            void AddPropertyTypes(Type propertyType)
            {
                if (IsTerminalType(propertyType))
                {
                    return;
                }

                if (propertyType.IsArray)
                {
                    AddType(propertyType.GetElementType());
                    return;
                }

                if (propertyType.IsGenericType)
                {
                    var genericArguments = propertyType.GetGenericArguments();

                    for (var i = 0; i < genericArguments.Length; i++)
                    {
                        AddType(genericArguments[i]);
                    }
                }

                if (propertyType.Namespace?.StartsWith("System", StringComparison.Ordinal) == false)
                {
                    AddType(propertyType);
                }
            }

            void AddType(Type type)
            {
                if (type == null || IsTerminalType(type) || uniqueTypes.Add(type) == false)
                {
                    return;
                }

                typeQueue.Enqueue(type);
            }
        }

        private static bool IsTerminalType(Type type)
            => type.IsPrimitive
            || type.IsEnum
            || type == typeof(decimal)
            || type == typeof(string)
            || type == typeof(object)
            ;
    }

    internal sealed class NamingMapCache
    {
        private readonly Dictionary<ISheet, NamingMap> _maps = new();

        public NamingMap GetOrCreate(PropertyInfo sheetProperty, ISheet sheet)
        {
            if (_maps.TryGetValue(sheet, out var map))
            {
                return map;
            }

            var (sheetName, nameCasing) = DatabaseSheetNaming.GetSettings(sheetProperty);
            map = NamingMap.Create(sheet, sheetName, nameCasing);
            _maps.Add(sheet, map);
            return map;
        }

        public void Clear()
        {
            _maps.Clear();
        }
    }

    internal static class DatabaseSheetNaming
    {
        public static bool ShouldProcessSheet(SheetConvertingContext context, PropertyInfo sheetProperty)
            => context.Container is not DataSheetContainerBase container
            || container.CheckSheetPropertyIsIgnored(sheetProperty.Name) == false;

        public static string GetExternalSheetName(PropertyInfo sheetProperty)
        {
            var (sheetName, nameCasing) = GetSettings(sheetProperty);
            return nameCasing.ConvertName(sheetName);
        }

        public static (string sheetName, NameCasing nameCasing) GetSettings(PropertyInfo sheetProperty)
        {
            var attribute = sheetProperty.PropertyType.GetCustomAttribute<TableNamingAttribute>();

            return attribute == null
                ? (sheetProperty.Name, NameCasing.Pascal)
                : (attribute.SheetName, attribute.NameCasing);
        }
    }
}
