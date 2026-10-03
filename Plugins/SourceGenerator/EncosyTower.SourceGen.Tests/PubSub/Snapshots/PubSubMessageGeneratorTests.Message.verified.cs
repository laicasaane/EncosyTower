#pragma warning disable 0219

using EncosyTower.PubSub;
using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__ST = global::System.Threading;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ET = global::EncosyTower.Common;
using g__ETL = global::EncosyTower.Logging;
using g__ETPS = global::EncosyTower.PubSub;
using g__ETUE = global::EncosyTower.UnityExtensions;
using g__ETT = global::EncosyTower.Tasks;

namespace TestProject
{


partial class Message : g__ETPS.IMessage
{
    [g__SCDC.GeneratedCode("EncosyTower.PubSub.Generators.PubSubMessageGenerator", "0.1.8-preview.1")]
    public readonly partial struct Async : g__ETPS.IMessage
    {
        private readonly global::TestProject.Message _value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        private Async(global::TestProject.Message value)
        {
            _value = value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator Async(global::TestProject.Message value)
        {
            return new Async(value);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator global::TestProject.Message(Async value)
        {
            return value._value;
        }
    }
}


}
