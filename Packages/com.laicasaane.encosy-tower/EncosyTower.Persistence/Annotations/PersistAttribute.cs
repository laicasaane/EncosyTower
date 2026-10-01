using System;

namespace EncosyTower.Persistences
{
    /// <summary>
    /// Applying <see cref="PersistAttribute"/> to a class or struct to generate
    /// implementation for <see cref="IPersist"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class PersistAttribute : Attribute
    {
    }
}
