#if UNITY_LOCALIZATION

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Types;
using EncosyTower.UnityExtensions;
using EncosyTower.Vaults;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace EncosyTower.Localization
{
    /// <summary>
    /// L10n = L(ocalizatio)n
    /// </summary>
    public static partial class L10n
    {
        public static readonly Id<Localization> TypeId = Type<Localization>.Id;

        private static readonly Dictionary<string, Locale> s_codeToLocaleMap = new();
        private static Func<ReadOnlyMemory<L10nLanguage>> s_getLanguages;
        private static Func<SystemLanguage, L10nLanguage> s_toLanguage;

        public static IReadOnlyDictionary<string, Locale> LocaleMap => s_codeToLocaleMap;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            GlobalValueVault<bool>.TrySet(TypeId, false);

            s_getLanguages = null;
            s_toLanguage = null;
            s_codeToLocaleMap.Clear();
        }
#endif

        public static void Initialize(
              [NotNull] Func<ReadOnlyMemory<L10nLanguage>> getLanguages
            , [NotNull] Func<SystemLanguage, L10nLanguage> toLanguage
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(getLanguages);
            Debugging.ThrowHelper.ThrowIfNull(toLanguage);
            GlobalValueVault<bool>.TrySet(TypeId, false);

            s_getLanguages = getLanguages;
            s_toLanguage = toLanguage;
            s_codeToLocaleMap.Clear();

            var locales = LocalizationSettings.AvailableLocales.Locales;

            foreach (var locale in locales)
            {
                s_codeToLocaleMap.TryAdd(locale.Identifier.Code, locale);
            }

            GlobalValueVault<bool>.TrySet(TypeId, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReady()
            => GlobalValueVault<bool>.TryGet(TypeId, out var value) && value;

        public static bool TryGetDefaultLocaleCode(out string localeCode)
        {
            if (IsReady() == false)
            {
                ThrowHelper.ErrorNotReady();
                localeCode = default;
                return false;
            }

            localeCode = s_toLanguage(Application.systemLanguage).ToLocaleCode();

            var localeOpt = FindLocale(localeCode);

            if (localeOpt.HasValue == false)
            {
                ThrowHelper.ErrorCannotFindLanguage(localeCode);
                localeCode = L10nLanguage.Default.ToLocaleCode();
                localeOpt = FindLocale(localeCode);
            }

            return localeOpt.HasValue;
        }

        public static string SetLocale([NotNull] string localeCode)
        {
            Debugging.ThrowHelper.ThrowIfNull(localeCode);
            if (IsReady() == false)
            {
                ThrowHelper.ErrorNotReady();
                return L10nLanguage.Default.ToLocaleCode();
            }

            if (string.IsNullOrWhiteSpace(localeCode))
            {
                localeCode = s_toLanguage(Application.systemLanguage).ToLocaleCode();
            }

            var localeOpt = FindLocale(localeCode);

            if (localeOpt.HasValue == false)
            {
                ThrowHelper.ErrorCannotFindLanguage(localeCode);
                localeCode = s_toLanguage(Application.systemLanguage).ToLocaleCode();
                localeOpt = FindLocale(localeCode);
            }

            if (localeOpt.HasValue == false)
            {
                ThrowHelper.ErrorCannotFindLanguage(localeCode);
                localeCode = L10nLanguage.Default.ToLocaleCode();
                localeOpt = FindLocale(localeCode);
            }

            if (localeOpt.HasValue == false)
            {
                ThrowHelper.ErrorCannotFindLanguage(localeCode);
            }
            else
            {
                var locale = localeOpt.GetValueOrThrow();
                ThrowHelper.InfoChangeLanguage(locale.LocaleName);
                LocalizationSettings.SelectedLocale = locale;
            }

            return localeCode;
        }

        public static void GetActiveLocales([NotNull] ICollection<string> locales)
        {
            Debugging.ThrowHelper.ThrowIfNull(locales);
            locales.Clear();

            var languages = s_getLanguages().Span;
            locales.TryIncreaseCapacityToFast(languages.Length);

            foreach (var locale in languages)
            {
                locales.Add(locale.ToLocaleCode());
            }
        }

        public static void SaveSelectedLanguage([NotNull] Action<string> onSave)
        {
            Debugging.ThrowHelper.ThrowIfNull(onSave);
            if (IsReady() == false)
            {
                ThrowHelper.ErrorNotReady();
                return;
            }

            var locale = LocalizationSettings.SelectedLocale;
            onSave?.Invoke(locale.Identifier.Code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<Locale> FindLocale(string localeCode)
            => Option.SomeIf(s_codeToLocaleMap.TryGetValue(localeCode, out var locale) && locale.IsValid(), locale);

        public readonly struct Localization { }
    }
}

#endif
