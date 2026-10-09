#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Core;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    public readonly struct BuildProfileOrTarget : IIsValid
    {
        private readonly BuildProfile _profile;
        private readonly NamedBuildTarget _target;

        private BuildProfileOrTarget(BuildProfile profile, NamedBuildTarget target)
        {
            _profile = profile;
            _target = target;
        }

        [ApiForEditor]
        public bool IsValid => _profile.IsValid() || _target.TargetName.IsNotEmpty();

        [ApiForEditor]
        public bool IsProfile => _profile.IsValid();

        [ApiForEditor]
        public bool IsTarget => _profile.IsValid() == false;

        [ApiForEditor]
        public string Name
        {
            get
            {
                ThrowIfInvalid(IsValid);

                if (IsProfile)
                {
                    return _profile.name;
                }
                else
                {
                    return _target.TargetName;
                }
            }
        }

        [ApiForEditor]
        public BuildProfile Profile
        {
            get
            {
                if (IsProfile == false)
                {
                    throw new InvalidOperationException("BuildProfileOrTarget does not contain a BuildProfile.");
                }

                return _profile;
            }
        }

        [ApiForEditor]
        public NamedBuildTarget Target
        {
            get
            {
                if (IsTarget == false)
                {
                    throw new InvalidOperationException("BuildProfileOrTarget does not contain a NamedBuildTarget.");
                }

                return _target;
            }
        }

        [ApiForEditor]
        public static implicit operator BuildProfileOrTarget(BuildProfile profile)
            => new(profile, default);

        [ApiForEditor]
        public static implicit operator BuildProfileOrTarget(NamedBuildTarget target)
            => new(default, target);

        [ApiForEditor]
        public EditorBuildSettingsScene[] GetScenesForBuild()
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                return _profile.GetScenesForBuild();
            }
            else
            {
                return EditorBuildSettings.scenes;
            }
        }

        [ApiForEditor]
        public HashSet<string> GetScriptingDefineSymbols()
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                return BuildAPI.GetScriptingDefineSymbols(_profile);
            }
            else
            {
                return BuildAPI.GetScriptingDefineSymbols(_target);
            }
        }

        [ApiForEditor]
        public void SetScriptingDefineSymbols(params string[] symbols)
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                BuildAPI.SetScriptingDefineSymbols(_profile, symbols);
            }
            else
            {
                BuildAPI.SetScriptingDefineSymbols(_target, symbols);
            }
        }

        [ApiForEditor]
        public void SetScriptingDefineSymbols(IEnumerable<string> symbols)
        {
            ThrowIfInvalid(IsValid);

            if (IsProfile)
            {
                BuildAPI.SetScriptingDefineSymbols(_profile, symbols);
            }
            else
            {
                BuildAPI.SetScriptingDefineSymbols(_target, symbols);
            }
        }

        [ApiForEditor]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        public static void ThrowIfInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("A valid BuildProfileOrTarget must be acquired from BuildAPI.ActiveProfileOrTarget.");
        }
    }
}

#endif
