using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.MonoPages
{
    /// <summary>
    /// This interface allows providing <see cref="MonoPageCodex"/> these necessities:
    /// <list type="bullet">
    /// <item>
    /// <see cref="PageFlowScopeCollectionApplier"/> to allow passing a user-defined
    /// <see cref="IPageFlowScopeCollection"/> around the <see cref="MonoPageCodex"/> system.
    /// </item>
    /// <item>
    /// <see cref="OnInitializeAsync(MonoPageCodex)"/> to run additional logic once
    /// the <see cref="MonoPageCodex"/> system is fully initialized.
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// To simplify user code, <see cref="PageFlowScopeCollectionApplier"/> should return an instance of
    /// <see cref="PageFlowScopeCollectionApplier{TFlowScopes}"/> unique to a <see cref="IMonoPageCodexOnInitialize"/>.
    /// </remarks>
    /// <seealso cref="PageFlowScopeCollectionApplier{TFlowScopes}"/>
    /// <seealso cref="IPageFlowScopeCollection"/>
    /// <example>
    /// <code>
    /// public class GamePageCodex : MonoBehaviour, IMonoPageCodexOnInitialize
    /// {
    ///     private readonly PageFlowScopeCollectionApplier&lt;GamePageFlowScopes&gt; _flowScopesApplier = new();
    ///
    ///     private MonoPageCodex _codex;
    ///
    ///     public IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier => _flowScopesApplier;
    ///
    ///     public UnityTask OnInitializeAsync(MonoPageCodex codex)
    ///     {
    ///         // custom logic
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IMonoPageCodexOnInitialize
    {
        IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier { get; }

        UnityTask OnInitializeAsync(MonoPageCodex codex);
    }
}
