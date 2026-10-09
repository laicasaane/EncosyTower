#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.ProjectAuditor.Editor;
using Unity.ProjectAuditor.Editor.Core;

namespace EncosyTower.Editor.AuditorRules
{
    internal static class ApiMarkerAnalyzerAPI
    {
        private const string SRCS = "System.Runtime.CompilerServices";
        private const string GENERATED_MARKER = $"{SRCS}.CompilerGeneratedAttribute";
        private const string ASYNC_MARKER = $"{SRCS}.AsyncStateMachineAttribute";
        private const string ITERATOR_MARKER = $"{SRCS}.IteratorStateMachineAttribute";
        private const string ASYNC_ITERATOR_MARKER = $"{SRCS}.AsyncIteratorStateMachineAttribute";

        private static readonly OpCode[] s_opCodes = {
            OpCodes.Call,
            OpCodes.Callvirt,
            OpCodes.Newobj,
            OpCodes.Ldfld,
            OpCodes.Ldflda,
            OpCodes.Stfld,
            OpCodes.Ldsfld,
            OpCodes.Ldsflda,
            OpCodes.Stsfld,
            OpCodes.Ldftn,
            OpCodes.Ldvirtftn,
            OpCodes.Ldtoken,
            OpCodes.Box,
            OpCodes.Newarr,
            OpCodes.Castclass,
            OpCodes.Isinst,
        };

        internal static IReadOnlyCollection<OpCode> InstructionOpCodes => s_opCodes;

        internal static bool TryFindViolation(
              MethodDefinition caller
            , MemberReference target
            , string marker
            , ReadOnlySpan<string> exemptionMarkers
            , out MemberReference api
        )
        {
            if (TryFindMarkedApi(target, marker, out api) == false)
            {
                return false;
            }

            var count = exemptionMarkers.Length;

            for (var i = 0; i < count; i++)
            {
                if (IsExempt(caller, exemptionMarkers[i]))
                {
                    api = default;
                    return false;
                }
            }

            return true;
        }

        internal static ReportItemBuilder CreateIssue(
              InstructionAnalysisContext context
            , Descriptor descriptor
            , MemberReference api
        )
            => context.CreateIssue(IssueCategory.Code, descriptor.Id, api.FullName);

        private static bool TryFindMarkedApi(MemberReference target, string marker, out MemberReference api)
        {
            if (TryResolve(target, out var definition) && HasMemberMarker(definition, marker))
            {
                api = target;
                return true;
            }

            if (target is GenericInstanceMethod method)
            {
                var arguments = method.GenericArguments;
                var count = arguments.Count;

                for (var i = 0; i < count; i++)
                {
                    if (TryFindMarkedApi(arguments[i], marker, out api))
                    {
                        return true;
                    }
                }
            }

            var type = target as TypeReference ?? target.DeclaringType;

            while (type != null)
            {
                if (type is GenericInstanceType generic)
                {
                    var arguments = generic.GenericArguments;
                    var count = arguments.Count;

                    for (var i = 0; i < count; i++)
                    {
                        if (TryFindMarkedApi(arguments[i], marker, out api))
                        {
                            return true;
                        }
                    }
                }

                if (type is TypeSpecification specification)
                {
                    type = specification.ElementType;

                    if (TryResolve(type, out var element) && HasMemberMarker(element, marker))
                    {
                        api = type;
                        return true;
                    }
                }
                else
                {
                    type = type.DeclaringType;
                }
            }

            api = default;
            return false;
        }

        private static bool TryResolve(MemberReference reference, out IMemberDefinition definition)
        {
            try
            {
                definition = reference switch {
                    MethodReference method => method.Resolve(),
                    FieldReference field => field.Resolve(),
                    TypeReference type when type.IsGenericParameter == false => type.Resolve(),
                    _ => default,
                };

                return definition != null;
            }
            catch (AssemblyResolutionException)
            {
                definition = default;
                return false;
            }
            catch (ResolutionException)
            {
                definition = default;
                return false;
            }
        }

        private static bool HasMemberMarker(IMemberDefinition member, string marker)
        {
            if (HasDirectMarker(member, marker))
            {
                return true;
            }

            if (member is MethodDefinition method && HasAssociatedMarker(method, marker))
            {
                return true;
            }

            for (var type = member.DeclaringType; type != null; type = type.DeclaringType)
            {
                if (HasDirectMarker(type, marker))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAssociatedMarker(MethodDefinition method, string marker)
        {
            var type = method.DeclaringType;

            if (type == null || method.IsSpecialName == false)
            {
                return false;
            }

            var properties = type.Properties;
            var propertyCount = properties.Count;

            for (var i = 0; i < propertyCount; i++)
            {
                var property = properties[i];

                if ((property.GetMethod == method || property.SetMethod == method) && HasDirectMarker(property, marker))
                {
                    return true;
                }
            }

            var events = type.Events;
            var eventCount = events.Count;

            for (var i = 0; i < eventCount; i++)
            {
                var item = events[i];

                if ((item.AddMethod == method || item.RemoveMethod == method || item.InvokeMethod == method)
                    && HasDirectMarker(item, marker)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasDirectMarker(ICustomAttributeProvider provider, string marker)
        {
            if (provider == null || provider.HasCustomAttributes == false)
            {
                return false;
            }

            var attributes = provider.CustomAttributes;
            var count = attributes.Count;

            for (var i = 0; i < count; i++)
            {
                if (attributes[i].AttributeType.FullName == marker)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsExempt(MethodDefinition caller, string marker)
        {
            if (HasMemberMarker(caller, marker))
            {
                return true;
            }

            if (IsGenerated(caller) == false)
            {
                return false;
            }

            return HasMarkedSource(caller, marker, new HashSet<MethodDefinition>());
        }

        private static bool IsGenerated(MethodDefinition method)
            => HasDirectMarker(method, GENERATED_MARKER) || HasDirectMarker(method.DeclaringType, GENERATED_MARKER);

        private static bool HasMarkedSource(MethodDefinition caller, string marker, HashSet<MethodDefinition> visited)
        {
            if (visited.Add(caller) == false)
            {
                return false;
            }

            for (var type = caller.DeclaringType; type != null; type = type.DeclaringType)
            {
                var methods = type.Methods;
                var count = methods.Count;

                for (var i = 0; i < count; i++)
                {
                    var source = methods[i];

                    if (source == caller || IsSourceOf(source, caller) == false)
                    {
                        continue;
                    }

                    if (HasMemberMarker(source, marker)
                        || (IsGenerated(source) && HasMarkedSource(source, marker, visited))
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsSourceOf(MethodDefinition source, MethodDefinition generated)
        {
            var attributes = source.CustomAttributes;
            var attributeCount = attributes.Count;

            for (var i = 0; i < attributeCount; i++)
            {
                var attribute = attributes[i];
                var name = attribute.AttributeType.FullName;

                if (name is ASYNC_MARKER or ITERATOR_MARKER or ASYNC_ITERATOR_MARKER
                    && attribute.ConstructorArguments.Count == 1
                    && attribute.ConstructorArguments[0].Value is TypeReference stateMachine
                    && stateMachine.GetElementType().FullName == generated.DeclaringType.FullName
                )
                {
                    return true;
                }
            }

            if (source.HasBody == false)
            {
                return false;
            }

            var instructions = source.Body.Instructions;
            var instructionCount = instructions.Count;

            for (var i = 0; i < instructionCount; i++)
            {
                if (instructions[i].Operand is MethodReference method
                    && method.Name == generated.Name
                    && TryResolve(method, out var resolved)
                    && resolved == generated
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}

#endif
