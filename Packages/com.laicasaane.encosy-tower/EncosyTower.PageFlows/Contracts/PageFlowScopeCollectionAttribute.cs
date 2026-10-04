using System;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// Marks a <c>partial struct</c> whose <see cref="PageFlowScope"/> properties are the scopes of page flows.
    /// The source generator implements <see cref="IPageFlowScopeCollection"/> for the struct.
    /// </summary>
    /// <remarks>
    /// A scope is an instance property of type <see cref="PageFlowScope"/> with a setter that is not <c>init</c>.
    /// Its identifier is the property name.
    /// </remarks>
    /// <example>
    /// <code>
    /// [PageFlowScopeCollection]
    /// public partial struct GamePageFlowScopes
    /// {
    ///     public PageFlowScope Screen { get; private set; }
    ///
    ///     public PageFlowScope Popup { get; private set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class PageFlowScopeCollectionAttribute : Attribute { }
}
