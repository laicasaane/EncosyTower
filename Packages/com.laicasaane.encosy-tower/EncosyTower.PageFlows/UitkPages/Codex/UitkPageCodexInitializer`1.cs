using EncosyTower.Common;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>
    /// A base component that initializes a <see cref="UitkPageCodex"/> on the same GameObject
    /// with the scope collection <typeparamref name="TScopes"/>.
    /// </summary>
    /// <remarks>
    /// The codex sets the scopes before it calls <see cref="OnInitializeAsync(UitkPageCodex, TScopes)"/>.
    /// If the codex cannot set them, it logs the error and does not call the hook.
    /// </remarks>
    public abstract class UitkPageCodexInitializer<TScopes> : MonoBehaviour, IUitkPageCodexOnInitialize
        where TScopes : struct, IPageFlowScopeCollection
    {
        private readonly PageFlowScopeCollectionApplier<TScopes> _flowScopesApplier = new();

        /// <summary>
        /// The scopes set by the codex, or none before the codex has initialized.
        /// </summary>
        protected Option<TScopes> FlowScopes => _flowScopesApplier.TryGet(out var scopes) ? scopes : Option.None;

        IPageFlowScopeCollectionApplier IUitkPageCodexOnInitialize.PageFlowScopeCollectionApplier
            => _flowScopesApplier;

        UnityTask IUitkPageCodexOnInitialize.OnInitializeAsync(UitkPageCodex codex)
            => _flowScopesApplier.TryGet(out var scopes)
                ? OnInitializeAsync(codex, scopes)
                : UnityTask.CompletedTask;

        /// <summary>
        /// Runs after the codex has created its flows and set <paramref name="scopes"/>.
        /// </summary>
        protected abstract UnityTask OnInitializeAsync(UitkPageCodex codex, TScopes scopes);
    }
}
