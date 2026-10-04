#if UNITY_UGUI

using EncosyTower.Tasks;

namespace EncosyTower.PageFlows.UguiPages
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
        IPageFlowScopeCollectionApplier PageFlowScopeCollectionApplier { get; }

        UnityTask OnInitializeAsync(UguiPageCodex codex);
    }
}

#endif
