using System;
using EncosyTower.CodeGen;

namespace EncosyTower.Processing
{
    [AttributeUsage(
          AttributeTargets.Class | AttributeTargets.Struct
        , AllowMultiple = true
        , Inherited = false
    )]
    public sealed class ProcessingAttribute : Attribute
    {
        public ProcessingAttribute(ApiMode mode)
        {
            Mode = mode;
        }

        public ApiMode Mode { get; }

        public StateMode State { get; set; } = StateMode.Both;

        public Type Scope { get; set; }
    }
}
