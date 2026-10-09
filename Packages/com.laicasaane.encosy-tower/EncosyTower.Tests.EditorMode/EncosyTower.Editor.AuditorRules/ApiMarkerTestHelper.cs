#if UNITY_EDITOR && UNITY_TEST_FRAMEWORK && ENCOSY_TESTS_ENABLED

using Mono.Cecil;
using NUnit.Framework;

namespace EncosyTower.Tests.Editor.AuditorRules
{
    internal static class ApiMarkerTestHelper
    {
        internal static void AssertViolation(
              ViolationSearch findViolation
            , MethodDefinition caller
            , MemberReference target
            , bool expected
        )
        {
            var actual = findViolation(
                  caller: caller
                , target: target
                , authoringAllowed: false
                , api: out _
            );

            Assert.That(actual, Is.EqualTo(expected), target.FullName);
        }

        internal static TypeDefinition AddType(ModuleDefinition module, string name)
        {
            var type = new TypeDefinition("Fixtures", name, TypeAttributes.Public, module.TypeSystem.Object);
            module.Types.Add(type);
            return type;
        }

        internal static TypeDefinition AddNested(TypeDefinition owner, string name)
        {
            var type = new TypeDefinition("", name, TypeAttributes.NestedPublic, owner.Module.TypeSystem.Object);

            owner.NestedTypes.Add(type);
            return type;
        }

        internal static MethodDefinition AddMethod(TypeDefinition owner, string name)
        {
            var method = new MethodDefinition(name, MethodAttributes.Public, owner.Module.TypeSystem.Void);
            owner.Methods.Add(method);
            return method;
        }

        internal static CustomAttribute Mark(ModuleDefinition module, ICustomAttributeProvider target, string fullName)
        {
            var separator = fullName.LastIndexOf('.');
            var type = new TypeReference(fullName[..separator], fullName[(separator + 1)..], module, module);
            var constructor = new MethodReference(".ctor", module.TypeSystem.Void, type) { HasThis = true };
            var attribute = new CustomAttribute(constructor);
            target.CustomAttributes.Add(attribute);
            return attribute;
        }

        internal delegate bool ViolationSearch(
              MethodDefinition caller
            , MemberReference target
            , bool authoringAllowed
            , out MemberReference api
        );
    }
}

#endif
