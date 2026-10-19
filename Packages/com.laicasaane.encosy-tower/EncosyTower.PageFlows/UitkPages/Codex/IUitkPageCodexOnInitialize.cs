using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.UitkPages.Codex
{
    /// <summary>
    /// This interface allows providing <see cref="UitkPageCodex"/> these necessities:
    /// <list type="bullet">
    /// <item>
    /// <see cref="PageFlowScopeCollectionApplier"/> to allow passing a user-defined
    /// <see cref="IPageFlowScopeCollection"/> around the <see cref="UitkPageCodex"/> system.
    /// </item>
    /// <item>
    /// <see cref="OnInitializeAsync(UitkPageCodex)"/> to run additional logic once
    /// the <see cref="UitkPageCodex"/> system is fully initialized.
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// To simplify user code, <see cref="PageFlowScopeCollectionApplier"/> should return an instance of
    /// <see cref="PageFlowScopeCollectionApplier{TFlowScopes}"/> unique to a <see cref="IUitkPageCodexOnInitialize"/>.
    /// </remarks>
    /// <seealso cref="PageFlowScopeCollectionApplier{TFlowScopes}"/>
    /// <seealso cref="IPageFlowScopeCollection"/>
    public interface IUitkPageCodexOnInitialize
    {
        /// <summary>Gets the applier that builds the scope collection of the codex.</summary>
        /// <remarks>Return the same instance every time.</remarks>
        IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier { get; }

        /// <summary>Runs once the codex has created its flows and built the scope collection.</summary>
        /// <param name="codex">The codex that initialized.</param>
        /// <returns>A task that ends when the initializer finishes.</returns>
        /// <remarks>
        /// The codex awaits the task in <see cref="UitkPageCodex.InitializeAsync"/>.
        /// <see cref="UitkPageCodex.Initialize"/> does not wait for it.
        /// </remarks>
        UnityTask OnInitializeAsync(UitkPageCodex codex);
    }
}
