using System.Collections.Generic;
using System.IO;
using BepInEx;
using EpicLootAPI;
using Newtonsoft.Json;

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
    
    public readonly LegendarySetInfo set;
    public readonly List<LegendaryInfo> items = new();
    public List<SetBonusInfo> SetBonuses => set.SetBonuses;

    public MagicSet(string id, LegendaryType rarity)
    {
        set = new LegendarySetInfo(rarity, id, $"$mod_epicloot_{id.ToLower()}");
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

    private static void DeserializeSet(string filePath)
    {
        if (!File.Exists(filePath)) return;
        if (!sets.TryGetValue(filePath, out var sourceSet)) return;
        var json = File.ReadAllText(filePath);
        var set = JsonConvert.DeserializeObject<LegendarySetInfo>(json, deserializationSettings);
        sourceSet.CopyFieldsFrom(set);
        sourceSet.Update();
    }

    private void SerializeItems()
    {
        foreach (var item in items)
        {
            var fileName = $"{item.ID}.json";
            var filePath = Path.Combine(legendaryFolderPath, fileName);
            legendaries[filePath] = item;
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

    private static void DeserializeItem(string filePath)
    {
        if (!File.Exists(filePath)) return;
        if (!legendaries.TryGetValue(filePath, out var sourceItem)) return;
        var json = File.ReadAllText(filePath);
        var item = JsonConvert.DeserializeObject<LegendaryInfo>(json, deserializationSettings);
        sourceItem.CopyFieldsFrom(item);
        sourceItem.Update();
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