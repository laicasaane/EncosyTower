#if UNITY_UGUI

using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.UguiPages.Codex
{
    /// <summary>
    /// This interface allows providing <see cref="UguiPageCodex"/> these necessities:
    /// <list type="bullet">
    /// <item>
    /// <see cref="PageFlowScopeCollectionApplier"/> to allow passing a user-defined
    /// <see cref="IPageFlowScopeCollection"/> around the <see cref="UguiPageCodex"/> system.
    /// </item>
    /// <item>
    /// <see cref="OnInitializeAsync(UguiPageCodex)"/> to run additional logic once
    /// the <see cref="UguiPageCodex"/> system is fully initialized.
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// To simplify user code, <see cref="PageFlowScopeCollectionApplier"/> should return an instance of
    /// <see cref="PageFlowScopeCollectionApplier{TFlowScopes}"/> unique to a <see cref="IUguiPageCodexOnInitialize"/>.
    /// </remarks>
    /// <seealso cref="PageFlowScopeCollectionApplier{TFlowScopes}"/>
    /// <seealso cref="IPageFlowScopeCollection"/>
    /// <example>
    /// <code>
    /// public class GamePageCodex : MonoBehaviour, IUguiPageCodexOnInitialize
    /// {
    ///     private readonly PageFlowScopeCollectionApplier&lt;GamePageFlowScopes&gt; _flowScopesApplier = new();
    ///
    ///     private UguiPageCodex _codex;
    ///
    ///     public IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier => _flowScopesApplier;
    ///
    ///     public UnityTask OnInitializeAsync(UguiPageCodex codex)
    ///     {
    ///         // custom logic
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IUguiPageCodexOnInitialize
    {
        /// <summary>Gets the applier that builds the scope collection of the codex.</summary>
        /// <remarks>Return the same instance every time.</remarks>
        IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier { get; }

        /// <summary>Runs once the codex has created its flows and built the scope collection.</summary>
        /// <param name="codex">The codex that initialized.</param>
        /// <returns>A task that ends when the initializer finishes.</returns>
        /// <remarks>
        /// The codex awaits the task in <see cref="UguiPageCodex.InitializeAsync"/>.
        /// <see cref="UguiPageCodex.Initialize"/> does not wait for it.
        /// </remarks>
        UnityTask OnInitializeAsync(UguiPageCodex codex);
    }
}

#endif
