using System;
using System.Reflection;
using EncosyTower.Common;
using EncosyTower.Processing;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Processing
{
    public sealed class ThrowHelperTests
    {
        [Test]
        public void ProcessHandlerMap_NullOwner_ThrowsBeforeAssignment()
        {
            var mapType = typeof(Processor).Assembly.GetType(
                  "EncosyTower.Processing.Internals.ProcessHandlerMap"
                , throwOnError: true
            );
            var constructor = mapType.GetConstructor(
                  BindingFlags.Instance | BindingFlags.NonPublic
                , binder: null
                , new[] { typeof(Processor) }
                , modifiers: null
            );

            Assert.That(constructor, Is.Not.Null);

            var exception = Assert.Throws<TargetInvocationException>(
                () => constructor.Invoke(new object[] { null })
            );
            var argumentException = GetArgumentNullException(exception);

            Assert.That(argumentException.ParamName, Is.EqualTo("owner"));
        }

        [Test]
        public void ProcessRegistry_NullMap_ThrowsBeforeWeakReferenceConstruction()
        {
            var mapType = typeof(Processor).Assembly.GetType(
                  "EncosyTower.Processing.Internals.ProcessHandlerMap"
                , throwOnError: true
            );
            var handlerType = typeof(Processor).Assembly.GetType(
                  "EncosyTower.Processing.Internals.IProcessHandler"
                , throwOnError: true
            );
            var constructor = typeof(ProcessRegistry).GetConstructor(
                  BindingFlags.Instance | BindingFlags.NonPublic
                , binder: null
                , new[] { mapType, handlerType }
                , modifiers: null
            );

            Assert.That(constructor, Is.Not.Null);

            var exception = Assert.Throws<TargetInvocationException>(
                () => constructor.Invoke(new object[] { null, null })
            );
            var argumentException = GetArgumentNullException(exception);

            Assert.That(argumentException.ParamName, Is.EqualTo("map"));
        }

        [Test]
        public void ProcessRegistry_NullHandler_ThrowsAfterMapGuard()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var mapType = typeof(Processor).Assembly.GetType(
                  "EncosyTower.Processing.Internals.ProcessHandlerMap"
                , throwOnError: true
            );
            var handlerType = typeof(Processor).Assembly.GetType(
                  "EncosyTower.Processing.Internals.IProcessHandler"
                , throwOnError: true
            );
            var constructor = typeof(ProcessRegistry).GetConstructor(
                  BindingFlags.Instance | BindingFlags.NonPublic
                , binder: null
                , new[] { mapType, handlerType }
                , modifiers: null
            );
            var mapField = typeof(Processor.Hub<GlobalScope>).GetField(
                  "_map"
                , BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.That(constructor, Is.Not.Null);
            Assert.That(mapField, Is.Not.Null);

            var map = mapField.GetValue(hub);
            var exception = Assert.Throws<TargetInvocationException>(
                () => constructor.Invoke(new[] { map, null })
            );
            var argumentException = GetArgumentNullException(exception);

            Assert.That(argumentException.ParamName, Is.EqualTo("handler"));
        }

        [Test]
        public void Hub_NullRegistries_ThrowsBeforeAssignment()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var hubType = typeof(Processor.Hub<GlobalScope>);
            var mapField = hubType.GetField("_map", BindingFlags.Instance | BindingFlags.NonPublic);
            var constructor = Array.Find(
                  hubType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                , static candidate => candidate.GetParameters().Length == 3
            );

            Assert.That(mapField, Is.Not.Null);
            Assert.That(constructor, Is.Not.Null);

            var map = mapField.GetValue(hub);
            var exception = Assert.Throws<TargetInvocationException>(
                () => constructor.Invoke(new object[] { default(GlobalScope), map, null })
            );
            var argumentException = GetArgumentNullException(exception);

            Assert.That(argumentException.ParamName, Is.EqualTo("registries"));
        }

        [Test]
        public void Hub_NullProcessDelegate_ThrowsBeforeHandlerConstruction()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            Action<Request> process = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => hub.Register(process)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(process)));
        }

        [Test]
        public void Hub_NullState_ThrowsBeforeHubConstruction()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            State state = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => hub.WithState(state)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(state)));
        }

        [Test]
        public void Hub_DestroyedUnityState_ThrowsBeforeHubConstruction()
        {
            using var processor = new Processor();
            var hub = processor.Global();
            var state = new GameObject();
            UnityEngine.Object.DestroyImmediate(state);

            var exception = Assert.Throws<ArgumentNullException>(
                () => hub.WithState(state)
            );

            Assert.That(exception.ParamName, Is.EqualTo(nameof(state)));
        }

        private static ArgumentNullException GetArgumentNullException(TargetInvocationException exception)
        {
            var argumentException = exception.InnerException as ArgumentNullException;

            Assert.That(argumentException, Is.Not.Null);
            return argumentException;
        }

        private readonly struct Request : IRequest
        {
        }

        private sealed class State
        {
        }
    }
}
