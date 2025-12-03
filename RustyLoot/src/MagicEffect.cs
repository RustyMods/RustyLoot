using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using EpicLootAPI;
using Newtonsoft.Json;
using ServerSync;

namespace RustyLoot;

public class MagicEffect
{
    private static JsonSerializerSettings serializationSettings => Extensions.serializationSettings;
    private static JsonSerializerSettings deserializationSettings => Extensions.deserializationSettings;
    
    private const string folderName = "RustyLoot";
    private const string effectFolderName = "MagicEffects";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);
    private static readonly string effectFolderPath = Path.Combine(folderPath, effectFolderName);
    
    private static readonly Dictionary<string, MagicItemEffectDefinition> effects = new();
    private static readonly Dictionary<string, ConfigEntry<Toggle>> noRollConfigs = new();
    private static readonly Dictionary<string, ConfigEntry<Toggle>> showLogConfigs = new();

    public static readonly List<MagicEffect> MagicEffects = new();

    public static Dictionary<MagicItemEffectDefinition, CustomSyncedValue<string>> syncedEffects = new();

    public readonly MagicItemEffectDefinition definition;
    public MagicItemEffectRequirements Requirements => definition.Requirements;
    public ValuesPerRarityDef ValuesPerRarity => definition.ValuesPerRarity;
    public string Ability 
    {
        set => definition.Ability = value;
    }

    public ConfigEntry<Toggle>? noRollConfig;
    public ConfigEntry<Toggle>? showLogs;
    
    public MagicEffect(string effectType)
    {
        string type = effectType.ToLower();
        string displayName = $"$mod_epicloot_{type}";
        string description = $"$mod_epicloot_{type}_desc";
        definition = new MagicItemEffectDefinition(effectType, displayName, description);
        MagicEffects.Add(this);
    }

    public bool IsEnabled() => noRollConfig?.Value is Toggle.On;
    public bool ShowLogs() => showLogs?.Value is Toggle.On;

    public void Register(bool serialize = true, bool configs = true)
    {
        definition.Register();
        if (serialize) Serialize();
        if (configs) SetupConfigs();
    }

    private void SetupConfigs()
    {
        noRollConfig = RustyLootPlugin.config("Magic Effects", definition.Type, Toggle.On, $"If on, {definition.Type} has chance to roll");
        noRollConfig.SettingChanged += (_, _) =>
        {
            definition.Requirements.NoRoll = noRollConfig.Value is Toggle.Off;
            definition.Update();
        };
        noRollConfigs[definition.Type] = noRollConfig;
        
        showLogs = RustyLootPlugin.config("Logs", definition.Type, Toggle.Off, $"If on, {definition.Type} will display detailed logs");
        showLogConfigs[definition.Type] = showLogs;
    }


    private void Serialize()
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        if (!Directory.Exists(effectFolderPath)) Directory.CreateDirectory(effectFolderPath);
        
        string fileName = $"{definition.Type}.json";
        string filePath = Path.Combine(effectFolderPath, fileName);
        
        effects[filePath] = definition;
        CustomSyncedValue<string> syncedValue = new(RustyLootPlugin.ConfigSync, $"RustyLoot.SyncedFiles.MagicEffects.{definition.Type}", "");
        syncedValue.ValueChanged += () => OnServerSyncFileChanged(definition);
        syncedEffects[definition] = syncedValue;
        if (File.Exists(filePath))
        {
            DeserializeEffect(filePath);
        }
        else
        {
            string json = JsonConvert.SerializeObject(definition, serializationSettings);
            File.WriteAllText(filePath, json);
        }
    }

    public static void SyncEffect(MagicItemEffectDefinition definition)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!syncedEffects.TryGetValue(definition, out var sync)) return;
        string json = JsonConvert.SerializeObject(definition, Extensions.serializationSettings);
        sync.Value = json;
    }

    private static void OnServerSyncFileChanged(MagicItemEffectDefinition definition)
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (!syncedEffects.TryGetValue(definition, out var sync)) return;
        if (string.IsNullOrEmpty(sync.Value)) return;
            
        var data = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(sync.Value, deserializationSettings);
        if (data == null) return;
        definition.CopyFieldsFrom(data);
        definition.Update();
        RustyLootPlugin.LogDebug($"Updated {definition.Type}");
    }

    private static void DeserializeEffect(string filePath)
    {
        if (!File.Exists(filePath)) return;
        if (!effects.TryGetValue(filePath, out MagicItemEffectDefinition? sourceDef)) return;
        string json = File.ReadAllText(filePath);
        MagicItemEffectDefinition? definition = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json, deserializationSettings);
        sourceDef.CopyFieldsFrom(definition);
        sourceDef.Update();
        SyncEffect(sourceDef);
    }
    
    public static void SetupWatcher()
    {
        FileSystemWatcher watcher = new FileSystemWatcher(effectFolderPath, "*.json");
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Changed += (_, e) =>
        {
            RustyLootPlugin.RustyLootLogger.LogDebug($"File changed: {Path.GetFileName(e.FullPath)}");
            DeserializeEffect(e.FullPath);
        };
    }
    
    public static bool IsEnabled(string effectType) =>
        noRollConfigs.TryGetValue(effectType, out ConfigEntry<Toggle> noRollConfig) && noRollConfig.Value is Toggle.On;
    public static bool ShowLogs(string effectType) => showLogConfigs.TryGetValue(effectType, out  ConfigEntry<Toggle> showLogs) && showLogs.Value is Toggle.On;

}