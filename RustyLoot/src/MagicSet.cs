using System.Collections.Generic;
using System.IO;
using BepInEx;
using EpicLootAPI;
using Newtonsoft.Json;
using ServerSync;

namespace RustyLoot;

public class MagicSet
{
    private static JsonSerializerSettings serializationSettings => Extensions.serializationSettings;
    private static JsonSerializerSettings deserializationSettings => Extensions.deserializationSettings;
    private const string folderName = "RustyLoot";
    private const string itemFolderName = "LegendaryItems";
    private const string setFolderName = "LegendarySets";
    private static readonly string folderPath = Path.Combine(Paths.ConfigPath, folderName);
    private static readonly string legendaryFolderPath = Path.Combine(folderPath, itemFolderName);
    private static readonly string setFolderPath = Path.Combine(folderPath, setFolderName);
    private static readonly Dictionary<string, LegendarySetInfo> sets = new();
    private static readonly Dictionary<string, LegendaryInfo> legendaries = new();

    public static readonly List<MagicSet> MagicSets = new();

    public static readonly Dictionary<LegendarySetInfo, CustomSyncedValue<string>> syncedSets = new();
    public static readonly Dictionary<LegendaryInfo, CustomSyncedValue<string>> syncedItems = new();
    
    public readonly LegendarySetInfo set;
    public readonly List<LegendaryInfo> items = new();
    public List<SetBonusInfo> SetBonuses => set.SetBonuses;

    public MagicSet(string id, LegendaryType rarity)
    {
        set = new LegendarySetInfo(rarity, id, $"$mod_epicloot_{id.ToLower()}");
        MagicSets.Add(this);
    }

    public void AddItems(params LegendaryInfo[] infos)
    {
        foreach (var item in infos)
        {
            item.IsSetItem = true;
            items.Add(item);
            set.LegendaryIDs.Add(item.ID);
        }
    }

    public void Register()
    {
        foreach (LegendaryInfo? item in items)
        {
            item.Register();
        }

        set.Register();
    }

    public void Serialize()
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        if (!Directory.Exists(setFolderPath)) Directory.CreateDirectory(setFolderPath);
        if (!Directory.Exists(legendaryFolderPath)) Directory.CreateDirectory(legendaryFolderPath);
        
        SerializeSet();
        SerializeItems();
    }

    private void SerializeSet()
    {
        var fileName = $"{set.ID}.json";
        var filePath = Path.Combine(setFolderPath, fileName);
        sets[filePath] = set;

        CustomSyncedValue<string> syncedSet = new(RustyLootPlugin.ConfigSync, $"RustyLoot.SyncedFiles.Sets.{set.ID}");
        syncedSet.ValueChanged += () => OnServerSyncSetChanged(set);
        syncedSets[set] = syncedSet;
        
        if (File.Exists(filePath))
        {
            DeserializeSet(filePath);
        }
        else
        {
            var json = JsonConvert.SerializeObject(set, serializationSettings);
            File.WriteAllText(filePath, json);
        }
    }

    private static void OnServerSyncSetChanged(LegendarySetInfo set)
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (!syncedSets.TryGetValue(set, out var sync)) return;
        if (string.IsNullOrEmpty(sync.Value)) return;
        
        var data = JsonConvert.DeserializeObject<LegendarySetInfo>(sync.Value, deserializationSettings);
        if (data == null) return;

        set.CopyFieldsFrom(data);
        set.Update();
        
        RustyLootPlugin.LogDebug($"Updated {set.ID}");
    }

    private static void DeserializeSet(string filePath)
    {
        if (!File.Exists(filePath)) return;
        if (!sets.TryGetValue(filePath, out var sourceSet)) return;
        var json = File.ReadAllText(filePath);
        var set = JsonConvert.DeserializeObject<LegendarySetInfo>(json, deserializationSettings);
        sourceSet.CopyFieldsFrom(set);
        sourceSet.Update();
        SyncSet(sourceSet);
    }

    private void SerializeItems()
    {
        foreach (LegendaryInfo? item in items)
        {
            var fileName = $"{item.ID}.json";
            var filePath = Path.Combine(legendaryFolderPath, fileName);
            legendaries[filePath] = item;
            var sync = new CustomSyncedValue<string>(RustyLootPlugin.ConfigSync,
                $"RustyLoot.SyncedFiles.LegendaryItems.{item.ID}");
            sync.ValueChanged += () =>
            {
                if (!ZNet.instance || ZNet.instance.IsServer()) return;

                if (string.IsNullOrEmpty(sync.Value)) return;
                
                var data = JsonConvert.DeserializeObject<LegendaryInfo>(sync.Value, deserializationSettings);
                if (data == null) return;

                item.CopyFieldsFrom(data);
                item.Update();
                
                RustyLootPlugin.LogDebug($"Updated {item.ID}");
            };
            syncedItems[item] = sync;
            
            
            if (File.Exists(filePath))
            {
                DeserializeItem(filePath);
            }
            else
            {
                var json = JsonConvert.SerializeObject(item, serializationSettings);
                File.WriteAllText(filePath, json);
            }
        }
    }

    public static void SyncSet(LegendarySetInfo set)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!syncedSets.TryGetValue(set, out var sync)) return;
        string json = JsonConvert.SerializeObject(set, Extensions.serializationSettings);
        sync.Value = json;
    }

    public static void SyncItem(LegendaryInfo info)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!syncedItems.TryGetValue(info, out var sync)) return;
        var json = JsonConvert.SerializeObject(info, Extensions.serializationSettings);
        sync.Value = json;
    }
    

    private static void DeserializeItem(string filePath)
    {
        if (!File.Exists(filePath)) return;
        if (!legendaries.TryGetValue(filePath, out LegendaryInfo? sourceItem)) return;
        var json = File.ReadAllText(filePath);
        var item = JsonConvert.DeserializeObject<LegendaryInfo>(json, deserializationSettings);
        sourceItem.CopyFieldsFrom(item);
        sourceItem.Update();
        SyncItem(sourceItem);
    }

    public static void SetupWatcher()
    {
        SetupSetWatcher();
        SetItemWatcher();
    }

    private static void SetupSetWatcher()
    {
        var watcher = new FileSystemWatcher(setFolderPath, "*.json");
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Changed += (_, e) =>
        {
            RustyLootPlugin.RustyLootLogger.LogDebug($"File changed: {Path.GetFileName(e.FullPath)}");
            DeserializeSet(e.FullPath);
        };
    }

    private static void SetItemWatcher()
    {
        var watcher = new FileSystemWatcher(legendaryFolderPath, "*.json");
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Changed += (_, e) =>
        {
            RustyLootPlugin.RustyLootLogger.LogDebug($"File changed: {Path.GetFileName(e.FullPath)}");
            DeserializeItem(e.FullPath);
        };
    }
}