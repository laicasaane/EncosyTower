using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.Types;
using UnityEngine;

namespace Samples.TypeFlags
{
    [TypeFlag]
    public sealed partial class AudioManager : MonoBehaviour
    {
        private void Awake()
        {
            s_typeFlag.TryRegister(this);
        }

        private void OnDestroy()
        {
            s_typeFlag.TryUnregister(this);
        }

        public void Play(AudioClip clip)
        {
            if (s_typeFlag.IsEnabled == false)
            {
                return;
            }
        }
    }

    [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
    public readonly partial struct GraphicsPreset
    {
        public readonly int Width;
        public readonly int Height;

        public GraphicsPreset(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void Publish()
        {
            TypeFlag.SetValue(this);
            TypeFlag.Enable();
        }
    }

    [TypeFlag]
    public sealed partial class MenuScreen : MonoBehaviour
    {
        private void OnEnable()
        {
            s_typeFlag.Enable();
        }

        private void OnDisable()
        {
            s_typeFlag.Disable();
            Initialized.TypeFlag.Disable();
        }

        private void OnPanelsInitialized()
        {
            Initialized.TypeFlag.Enable();
        }

        [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.State)]
        public readonly partial struct Initialized { }
    }

    public sealed class SoundProfile : ScriptableObject { }

    [TypeFlag(Api = TypeFlagApi.Related)]
    public sealed partial class SettingsLoader : MonoBehaviour
    {
        private SoundProfile _soundProfile;

        private void Awake()
        {
            _soundProfile = ScriptableObject.CreateInstance<SoundProfile>();
            s_typeFlag.TryAddObject(_soundProfile);
            s_typeFlag.Enable();
        }

        private void OnDestroy()
        {
            s_typeFlag.Disable();
            s_typeFlag.TryRemoveObject(_soundProfile);
        }
    }

    [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.Self)]
    public sealed partial class SessionService { }

    internal static class AppBootstrap
    {
        public static void Install()
        {
            SessionService.TypeFlag.TryRegister(new SessionService());
        }
    }

    public static class Consumers
    {
        public static bool IsMenuReady()
            => MenuScreen.TypeFlag.IsEnabled && MenuScreen.Initialized.TypeFlag.IsEnabled;

        public static void PlayIfReady(AudioClip clip)
        {
            if (AudioManager.TypeFlag.TryGetInstance(out var audio))
            {
                audio.Play(clip);
            }
        }

        public static async UnityTask PlayWhenReadyAsync(AudioClip clip, CancellationToken token)
        {
            var audio = await AudioManager.TypeFlag.GetInstanceAsync(token);
            audio.Play(clip);
        }

        public static SoundProfile GetSoundProfile()
            => SettingsLoader.TypeFlag.GetObjectOrThrow<SoundProfile>();

        public static int GetPresetWidth()
            => GraphicsPreset.TypeFlag.GetValueOrThrow().Width;

        public static void OverridePreset()
        {
            GraphicsPreset.TypeFlag.SetValue(new GraphicsPreset(width: 1920, height: 1080));
        }

        public static SessionService GetSession()
            => SessionService.TypeFlag.GetInstanceOrThrow();
    }

    public readonly struct BestScore
    {
        public readonly int Value;

        public BestScore(int value)
        {
            Value = value;
        }
    }

    [TypeFlag(UseExtensions = true)]
    public sealed partial class ScoreBoard : MonoBehaviour
    {
        private void Awake()
        {
            s_typeFlag.TryRegister(this);
            s_typeFlag.GetLink<BestScore>().SetValue(new BestScore(value: 0));
        }

        private void OnDestroy()
        {
            s_typeFlag.GetLink<BestScore>().TryRemoveValue(out _);
            s_typeFlag.TryUnregister(this);
        }
    }

    [TypeFlag(UseExtensions = true, WriteAccess = TypeFlagAccess.Internal)]
    public readonly partial struct Difficulty
    {
        public readonly int Level;

        public Difficulty(int level)
        {
            Level = level;
        }
    }

    public static class ScoreConsumers
    {
        public static bool TryGetBoard(out ScoreBoard board)
            => ScoreBoard.TypeFlag.TryGetInstance(out board);

        public static int GetBestScore()
            => ScoreBoard.TypeFlag.GetLink<BestScore>().GetValueOrThrow().Value;

        public static void SetDifficulty(int level)
        {
            Difficulty.s_typeFlag.SetValue(new Difficulty(level));
            Difficulty.s_typeFlag.Enable();
        }

        public static int GetDifficultyLevel()
            => Difficulty.TypeFlag.GetValueOrThrow().Level;
    }
}
