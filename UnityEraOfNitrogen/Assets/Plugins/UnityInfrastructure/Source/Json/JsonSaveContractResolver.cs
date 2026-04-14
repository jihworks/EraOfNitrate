// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

#if INFRASTRUCTURE_USE_NEWTONSOFT_JSON

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Jih.Unity.Infrastructure.Json
{
    class JsonSaveContractResolver : DefaultContractResolver
    {
        // Value is dummy.
        static readonly ConcurrentDictionary<Type, bool> _checkedTypes = new();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            static void CheckIncludeOrExclude(Type reportType, MemberInfo member, out bool hasInclude, out bool hasExclude)
            {
                hasInclude = member.GetCustomAttribute<JsonPropertyAttribute>() is not null;
                hasExclude = member.GetCustomAttribute<JsonIgnoreAttribute>() is not null;
                if (!hasInclude && !hasExclude)
                {
                    throw new JsonSaveException($"Member '{member.Name}' in '{reportType.FullName}' missing JsonProperty or JsonIgnore attribute. All members must be marked explicitly.", reportType, member);
                }
            }

            static void CheckConstructors(Type reportType, IReadOnlyList<ConstructorInfo> constructors)
            {
                bool anyFound = false;
                foreach (var constructor in constructors)
                {
                    if (constructor.GetParameters().Length > 0)
                    {
                        continue;
                    }
                    if (constructor.GetCustomAttribute<JsonConstructorAttribute>() is null)
                    {
                        continue;
                    }

                    anyFound = true;
                }

                if (!anyFound)
                {
                    throw new JsonSaveException($"Type '{reportType.FullName}' must have explicit default-constructor with JsonConstructor attribute.", reportType);
                }
            }

            static void CheckType(Type reportType, Type checkType)
            {
                if (checkType.GetCustomAttribute<JsonObjectAttribute>() is null)
                {
                    throw new JsonSaveException($"JSON save type '{checkType.FullName}' must marked with JsonObjectAttribute.", checkType);
                }

                FieldInfo[] fields = checkType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    bool compilerGenerated = field.GetCustomAttribute<CompilerGeneratedAttribute>() is not null;
                    if (compilerGenerated)
                    {
                        continue;
                    }

                    CheckIncludeOrExclude(reportType, field, out _, out _);
                }

                PropertyInfo[] properties = checkType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    CheckIncludeOrExclude(reportType, property, out _, out bool hasExclude);
                    if (hasExclude)
                    {
                        continue;
                    }

                    if (!property.CanRead)
                    {
                        throw new JsonSaveException($"JSON save Property '{property.Name}' in type '{checkType.FullName}' missing getter.", checkType, property);
                    }
                    if (!property.CanWrite)
                    {
                        throw new JsonSaveException($"JSON save Property '{property.Name}' in type '{checkType.FullName}' missing setter.", checkType, property);
                    }
                }
            }

            // Skip if already checked type.
            if (!_checkedTypes.TryAdd(type, true))
            {
                goto SKIP;
            }

            if (type.IsValueType)
            {
                CheckType(type, type);
            }
            else
            {
                ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                CheckConstructors(type, constructors);

                Type? currentType = type;
                while (currentType is not null && currentType != typeof(object))
                {
                    CheckType(type, currentType);

                    currentType = currentType.BaseType;
                }
            }

        SKIP:
            return base.CreateProperties(type, memberSerialization);
        }
    }
}

#endif
