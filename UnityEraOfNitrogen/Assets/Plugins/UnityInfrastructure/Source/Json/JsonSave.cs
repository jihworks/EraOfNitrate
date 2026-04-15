// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

#if INFRASTRUCTURE_USE_NEWTONSOFT_JSON

using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Jih.Unity.Infrastructure.Json
{
    /// <summary>
    /// Safety and encapsulation improved JSON serialization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using <c>Newtonsoft.Json</c> as back-end.
    /// </para>
    /// <para>
    /// The serializing target types must follow these rules:<br/>
    /// 1. The type must marked with <see cref="JsonObjectAttribute"/>.<br/>
    /// 2. <b>All members</b><i>(fields or properties)</i> of the type must marked with <see cref="JsonPropertyAttribute"/> or <see cref="JsonIgnoreAttribute"/>.<br/>
    /// - <c>private</c>s and <c>readonly</c>s are not exception.<br/>
    /// - Base class also no exception. Therefore, you may need to declare own types. It causes some inconvenience, but it is intended.<br/>
    /// - <see cref="JsonPropertyAttribute"/> for serializing member.<br/>
    /// - <see cref="JsonIgnoreAttribute"/> for non-serializing member.<br/>
    /// - Therefore, <see cref="JsonObjectAttribute.MemberSerialization"/> has no effect.<br/>
    /// 3. The type must have <b>default-constructor</b><i>(no parameters)</i> with <see cref="JsonConstructorAttribute"/>.<br/>
    /// - <c>private</c> default-constructors are also allowed.<br/>
    /// - Therefore, the default-constructor is always called when deserializing.<br/>
    /// 4. Serializing properties must have <b>setter</b>.<br/>
    /// - <c>private</c> setters are also allowed.<br/>
    /// <br/>
    /// * If detected prohibited case, <see cref="JsonSaveException"/> will throw.
    /// </para>
    /// <para>
    /// <c>runtimeRootNamespace</c> means, container <c>namespace</c> of the objects to serialize or deserialize in this app-context<i>(runtime)</i>.<br/>
    /// In other words, identifier with <c>namepace</c> of the objects must starts with <c>runtimeRootNamespace</c>.<br/>
    /// This feature provides portability of serialized-data even though, the objects are moved to another <c>namespace</c> for refactoring.
    /// </para>
    /// </remarks>
    public static class JsonSave
    {
        public static string SerializeObject(object obj, string? runtimeRootNamespace)
        {
            return JsonConvert.SerializeObject(obj, GetSettings(runtimeRootNamespace));
        }

        public static T? DeserializeObject<T>(string json, string? runtimeRootNamespace)
        {
            return JsonConvert.DeserializeObject<T>(json, GetSettings(runtimeRootNamespace));
        }

        public static void PopulateObject(string json, object target, string? runtimeRootNamespace)
        {
            JsonConvert.PopulateObject(json, target, GetSettings(runtimeRootNamespace));
        }

        static JsonSerializerSettings GetSettings(string? runtimeRootNamespace)
        {
            runtimeRootNamespace = RefineRuntimeRootNamespace(runtimeRootNamespace);

            if (_settings.TryGetValue(runtimeRootNamespace, out JsonSerializerSettings result))
            {
                return result;
            }

            result = new()
            {
                ContractResolver = new JsonSaveContractResolver(),

                // This option allows private constructors.
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,

                // This option will replace collection fields with new instances.
                ObjectCreationHandling = ObjectCreationHandling.Replace,

                PreserveReferencesHandling = PreserveReferencesHandling.Objects,

                TypeNameHandling = TypeNameHandling.Auto,
                // This option has no effect because of custom SerializationBinder.
                // TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                SerializationBinder = new JsonSaveSerializationBinder(runtimeRootNamespace),

                Formatting = Formatting.Indented,
            };
            _settings.TryAdd(runtimeRootNamespace, result);

            return result;
        }

        static string RefineRuntimeRootNamespace(string? runtimeRootNamespace)
        {
            if (runtimeRootNamespace is null)
            {
                return string.Empty;
            }
            
            if (runtimeRootNamespace.Length > 0 && runtimeRootNamespace[^1] != '.')
            {
                return runtimeRootNamespace + ".";
            }

            return runtimeRootNamespace;
        }

        static readonly ConcurrentDictionary<string, JsonSerializerSettings> _settings = new();
    }
}

#endif
