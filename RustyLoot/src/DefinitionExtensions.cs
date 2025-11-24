using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using EpicLootAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace RustyLoot;

public static class DefinitionExtensions
{
    private static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings
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
    
    private static readonly JsonSerializerSettings deserializationSettings = new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Populate,
        Converters =
        {
            new Newtonsoft.Json.Converters.StringEnumConverter()
        }
    };


    private const string folderName = "RustyLoot";
    private const string effectFolderName = "MagicEffects";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);
    private static readonly string effectFolderPath = Path.Combine(folderPath, effectFolderName);

    private static readonly Dictionary<string, MagicItemEffectDefinition> files = new();
    private static readonly Dictionary<string, ConfigEntry<Toggle>> noRollConfigs = new();

    public static bool IsEnabled(string effectType) =>
        noRollConfigs.TryGetValue(effectType, out ConfigEntry<Toggle> noRollConfig) && noRollConfig.Value is Toggle.On;

    public static void Serialize(this MagicItemEffectDefinition definition)
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        if (!Directory.Exists(effectFolderPath)) Directory.CreateDirectory(effectFolderPath);
        string fileName = $"{definition.Type}.json";
        string filePath = Path.Combine(effectFolderPath, fileName);
        files[filePath] = definition;
        if (File.Exists(filePath))
        {
            Deserialize(filePath);
        }
        else
        {
            string json = JsonConvert.SerializeObject(definition, serializationSettings);
            File.WriteAllText(filePath, json);
        }

        ConfigEntry<Toggle> noRollConfig = RustyLootPlugin.config("Magic Effects", definition.Type, Toggle.On, $"If on, {definition.Type} has chance to roll");
        noRollConfig.SettingChanged += (_, _) =>
        {
            definition.Requirements.NoRoll = noRollConfig.Value is Toggle.Off;
            definition.Update();
        };
        noRollConfigs[definition.Type] = noRollConfig;
    }

    public static void Deserialize(string filePath)
    {
        if (!File.Exists(filePath)) return;
        if (!files.TryGetValue(filePath, out MagicItemEffectDefinition? sourceDef)) return;
        string json = File.ReadAllText(filePath);
        MagicItemEffectDefinition? definition = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json, deserializationSettings);
        sourceDef.CopyFieldsFrom(definition);
        sourceDef.Update();
        RustyLootPlugin.RustyLootLogger.LogDebug($"Found {Path.GetFileName(filePath)}, reading values");
    }

    public static void SetupMagicEffectWatcher()
    {
        FileSystemWatcher watcher = new FileSystemWatcher(effectFolderPath, "*.json");
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Changed += OnChanged;

    }

    public static void OnChanged(object sender, FileSystemEventArgs e)
    {
        RustyLootPlugin.RustyLootLogger.LogDebug($"File changed: {Path.GetFileName(e.FullPath)}");
        Deserialize(e.FullPath);
    }
    
    
    private static void CopyFieldsFrom<T>(this T target, T source)
    {
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object? value = field.GetValue(source);
            field.SetValue(target, value);
        }
    }
    
    public class StringListConfig
    {
        public readonly List<string> list;
        public StringListConfig(List<string> items) => list = items;
        public StringListConfig(string items) => list = items.Split(',').ToList();
        public static void Draw(ConfigEntryBase cfg)
        {
            bool locked = cfg.Description.Tags
                .Select(a =>
                    a.GetType().Name == "ConfigurationManagerAttributes"
                        ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                        : null).FirstOrDefault(v => v != null) ?? false;
            bool wasUpdated = false;
            List<string> strings = new();
            GUILayout.BeginVertical();
            foreach (var prefab in new StringListConfig((string)cfg.BoxedValue).list)
            {
                GUILayout.BeginHorizontal();
                var prefabName = prefab;
                var nameField = GUILayout.TextField(prefab);
                if (nameField != prefab && !locked)
                {
                    wasUpdated = true;
                    prefabName = nameField;
                }

                if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
                {
                    wasUpdated = true;
                }
                else
                {
                    strings.Add(prefabName);
                }

                if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
                {
                    strings.Add("");
                    wasUpdated = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            if (wasUpdated)
            {
                cfg.BoxedValue = new StringListConfig(strings).ToString();
            }
        }

        public override string ToString() => string.Join(",", list);
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