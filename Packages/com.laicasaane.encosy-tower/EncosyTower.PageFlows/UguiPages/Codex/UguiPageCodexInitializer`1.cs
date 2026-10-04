#if UNITY_UGUI

using EncosyTower.Common;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    /// <summary>
    /// A base component that initializes a <see cref="UguiPageCodex"/> on the same GameObject
    /// with the scope collection <typeparamref name="TScopes"/>.
    /// </summary>
    /// <remarks>
    /// The codex sets the scopes before it calls <see cref="OnInitializeAsync(UguiPageCodex, TScopes)"/>.
    /// If the codex cannot set them, it logs the error and does not call the hook.
    /// </remarks>
    /// <example>
    /// <code>
    /// public sealed class GamePageCodex : UguiPageCodexInitializer&lt;GamePageFlowScopes&gt;
    /// {
    ///     protected override UnityTask OnInitializeAsync(UguiPageCodex codex, GamePageFlowScopes scopes)
    ///     {
    ///         // custom logic
    ///         return UnityTask.CompletedTask;
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class UguiPageCodexInitializer<TScopes> : MonoBehaviour, IUguiPageCodexOnInitialize
        where TScopes : struct, IPageFlowScopeCollection
    {
        private readonly PageFlowScopeCollectionApplier<TScopes> _flowScopesApplier = new();

        /// <summary>
        /// The scopes set by the codex, or none before the codex has initialized.
        /// </summary>
        protected Option<TScopes> FlowScopes => _flowScopesApplier.TryGet(out var scopes) ? scopes : Option.None;

        IPageFlowScopeCollectionApplier IUguiPageCodexOnInitialize.PageFlowScopeCollectionApplier
            => _flowScopesApplier;

        UnityTask IUguiPageCodexOnInitialize.OnInitializeAsync(UguiPageCodex codex)
            => _flowScopesApplier.TryGet(out var scopes)
                ? OnInitializeAsync(codex, scopes)
                : UnityTask.CompletedTask;

        /// <summary>
        /// Runs after the codex has created its flows and set <paramref name="scopes"/>.
        /// </summary>
        protected abstract UnityTask OnInitializeAsync(UguiPageCodex codex, TScopes scopes);
    }
}

#endif
