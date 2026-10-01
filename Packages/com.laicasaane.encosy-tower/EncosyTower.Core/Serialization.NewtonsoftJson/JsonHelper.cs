#if UNITY_NEWTONSOFT_JSON

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using JsonThrowHelper = EncosyTower.Serialization.NewtonsoftJson.ThrowHelper;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Serialization.NewtonsoftJson
{
    public static class JsonHelper
    {
        private static JsonSerializerSettings s_settings;
        private static JsonSerializer s_serializer;

        public static JsonSerializerSettings Settings
        {
            get => s_settings ??= new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                ContractResolver = new ContractResolver(),
            };
        }

        public static JsonSerializer Serializer
        {
            get
            {
                if (s_serializer == null)
                {
                    s_serializer = JsonSerializer.CreateDefault(Settings);
                    s_serializer.CheckAdditionalContent = true;
                }

                return s_serializer;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            s_settings = null;
            s_serializer = null;
        }
#endif

        private sealed class ContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(
                  Type type
                , MemberSerialization memberSerialization
            )
            {
                var props = base.CreateProperties(type, memberSerialization);
                return props.Where(static p => p.Writable).ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySerialize(object data, out string json)
            => TrySerialize(data, out json, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDeserialize<T>([NotNull] string json, out T data)
        {
            DebuggingThrowHelper.ThrowIfNull(json);
            return TryDeserialize(json, out data, null);
        }

        public static bool TrySerialize(object data, out string json, Logging.ILogger logger)
        {
            try
            {
                json = JsonConvert.SerializeObject(data, Settings);

                if (string.Equals(json, "null", StringComparison.OrdinalIgnoreCase))
                {
                    JsonThrowHelper.LogErrorSerializeToNull(logger);
                    return false;
                }

                return string.IsNullOrWhiteSpace(json) == false;
            }
            catch (Exception ex)
            {
                JsonThrowHelper.LogException(ex, logger);
                json = string.Empty;
                return false;
            }
        }

        public static bool TryDeserialize<T>([NotNull] string json, out T data, Logging.ILogger logger)
        {
            DebuggingThrowHelper.ThrowIfNull(json);
            try
            {
                data = JsonConvert.DeserializeObject<T>(json, Settings);
                return data != null;
            }
            catch (Exception ex)
            {
                JsonThrowHelper.LogException(ex, logger);
                data = default;
                return false;
            }
        }

        public static Result<string> Serialize(object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Settings);

                if (string.Equals(json, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<string>.Err(
                        JsonThrowHelper.GetSerializedStringIsNullMessage()
                    );
                }

                return string.IsNullOrWhiteSpace(json) == false
                    ? Result<string>.Succeed(json)
                    : Result<string>.Err(
                        JsonThrowHelper.GetSerializedStringIsEmptyMessage()
                    );
            }
            catch (Exception ex)
            {
                return Result<string>.Err(ex);
            }
        }

        public static Result<T> Deserialize<T>([NotNull] string json)
        {
            DebuggingThrowHelper.ThrowIfNull(json);
            try
            {
                var data = JsonConvert.DeserializeObject<T>(json, Settings);

                return data != null
                    ? Result<T>.Succeed(data)
                    : Result<T>.Err(
                        JsonThrowHelper.GetDeserializedObjectIsNullMessage()
                    );
            }
            catch (Exception ex)
            {
                return Result<T>.Err(ex);
            }
        }

    }
}

#endif
