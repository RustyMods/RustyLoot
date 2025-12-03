using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using EpicLootAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RustyLoot;

public static class Extensions
{
    public static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Formatting = Formatting.Indented,
        ContractResolver = new IgnoreEmptyValuesResolver(),
        Converters =
        {
            new Newtonsoft.Json.Converters.StringEnumConverter()
        }
    };
    
    public static readonly JsonSerializerSettings deserializationSettings = new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
        ObjectCreationHandling = ObjectCreationHandling.Replace,
        DefaultValueHandling = DefaultValueHandling.Populate,
        Converters =
        {
            new Newtonsoft.Json.Converters.StringEnumConverter()
        }
    };
    
    public static void CopyFieldsFrom<T>(this T target, T source)
    {
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object? value = field.GetValue(source);
            if (value == null) continue;
            field.SetValue(target, value);
        }
    }

    public static void All(this List<ItemRarity> list)
    {
        list.Add(ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic);
    }
    
    public class IgnoreEmptyValuesResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // Skip empty strings
            if (property.PropertyType == typeof(string))
            {
                property.ShouldSerialize = instance =>
                {
                    string? value = property.ValueProvider?.GetValue(instance) as string;
                    return !string.IsNullOrWhiteSpace(value);
                };

                return property;
            }

            // Skip empty collections
            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                property.ShouldSerialize = instance =>
                {
                    object? value = property.ValueProvider?.GetValue(instance);
                    if (value == null) return false;

                    if (value is not IEnumerable enumerable) return true;
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    using var enumerator1 = enumerator as IDisposable;
                    return enumerator.MoveNext(); // only serialize if non-empty
                };
            }

            return property;
        }
    }
}