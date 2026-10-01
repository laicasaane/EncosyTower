using System;
using EncosyTower.CodeGen;

namespace EncosyTower.PubSub
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class PubSubAttribute : Attribute
    {
        public PubSubAttribute(ApiMode mode)
        {
            Mode = mode;
        }

        public ApiMode Mode { get; }

        public StateMode State { get; set; } = StateMode.Both;

        public Type Scope { get; set; }
    }
}
