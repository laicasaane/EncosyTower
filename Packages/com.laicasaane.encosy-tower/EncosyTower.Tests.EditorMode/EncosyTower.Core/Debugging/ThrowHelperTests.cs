using System;
using EncosyTower.Common;
using EncosyTower.Initialization;
using NUnit.Framework;
using UnityEngine;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Tests.Core.Debugging
{
    public sealed class ThrowHelperTests
    {
        [Test]
        public void ThrowIfNull_Null_ThrowsWithCallerArgumentName()
        {
            object argument = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => DebuggingThrowHelper.ThrowIfNull(argument)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(argument)));
        }

        [Test]
        public void ThrowIfNull_NonNull_DoesNotThrow()
        {
            var argument = new object();

            Assert.DoesNotThrow(() => DebuggingThrowHelper.ThrowIfNull(argument));
        }

        [TestCase(null)]
        [TestCase("")]
        public void ThrowIfNullOrEmpty_Invalid_ThrowsWithCallerArgumentName(string argument)
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => DebuggingThrowHelper.ThrowIfNullOrEmpty(argument)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(argument)));
        }

        [Test]
        public void ThrowIfUnityObjectInvalid_Null_ThrowsWithCallerArgumentName()
        {
            UnityEngine.Object argument = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => DebuggingThrowHelper.ThrowIfUnityObjectInvalid(argument)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(argument)));
        }

        [Test]
        public void ThrowIfUnityObjectInvalid_Destroyed_ThrowsWithCallerArgumentName()
        {
            var argument = new GameObject();
            UnityEngine.Object.DestroyImmediate(argument);

            var exception = Assert.Throws<ArgumentNullException>(
                () => DebuggingThrowHelper.ThrowIfUnityObjectInvalid(argument)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(argument)));
        }

        [Test]
        public void ThrowIfUnityObjectInvalid_Live_DoesNotThrow()
        {
            var argument = new GameObject();

            try
            {
                Assert.DoesNotThrow(() => DebuggingThrowHelper.ThrowIfUnityObjectInvalid(argument));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(argument);
            }
        }

        [Test]
        public void ThrowIfNullOrUnityObjectInvalid_Null_Throws()
        {
            object argument = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(argument)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(argument)));
        }

        [Test]
        public void ThrowIfNullOrUnityObjectInvalid_Destroyed_Throws()
        {
            var unityObject = new GameObject();
            UnityEngine.Object.DestroyImmediate(unityObject);

            var exception = Assert.Throws<ArgumentNullException>(
                () => DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(unityObject)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(unityObject)));
        }

        [Test]
        public void ThrowIfNullOrUnityObjectInvalid_ValidReferences_DoNotThrow()
        {
            var argument = new object();
            var unityObject = new GameObject();

            try
            {
                Assert.DoesNotThrow(() => DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(argument));
                Assert.DoesNotThrow(() => DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(unityObject));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(unityObject);
            }
        }

        [Test]
        public void ThrowIfNullOrUnityObjectInvalid_ValueType_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(1));
        }

        [Test]
        public void ThrowIfNotCreated_NotCreated_ThrowsWithCallerArgumentName()
        {
            var argument = new CreatedState(isCreated: false);

            var exception = Assert.Throws<ArgumentException>(
                () => DebuggingThrowHelper.ThrowIfNotCreated(argument)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(argument)));
        }

        [Test]
        public void ThrowIfNotInitialized_NotInitialized_ThrowsWithCallerArgumentName()
        {
            var argument = new InitializedState(isInitialized: false);

            var exception = Assert.Throws<ArgumentException>(
                () => DebuggingThrowHelper.ThrowIfNotInitialized(argument)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(argument)));
        }

        private readonly struct CreatedState : IIsCreated
        {
            public CreatedState(bool isCreated)
            {
                IsCreated = isCreated;
            }

            public bool IsCreated { get; }
        }

        private readonly struct InitializedState : IIsInitialized
        {
            public InitializedState(bool isInitialized)
            {
                IsInitialized = isInitialized;
            }

            public bool IsInitialized { get; }
        }
    }
}
