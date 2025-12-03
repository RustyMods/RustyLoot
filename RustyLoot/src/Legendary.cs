using System.IO;
using EpicLootAPI;
using Newtonsoft.Json;
using ServerSync;

namespace RustyLoot;

public class Legendary
{
    private static JsonSerializerSettings serializationSettings => Extensions.serializationSettings;
    private static JsonSerializerSettings deserializationSettings => Extensions.deserializationSettings;
    private static string legendaryFolderPath => MagicSet.legendaryFolderPath;
    
    public readonly LegendaryInfo info;
    
    public Legendary(LegendaryType type, string id)
    {
        var lower = id.ToLower();
        info = new LegendaryInfo(type, id, $"$mod_epicloot_{lower}", $"$mod_epicloot_{lower}_desc");
    }

    public void Register(bool serialize = true)
    {
        info.Register();
        if (serialize) Serialize();
    }

    public void Serialize()
    {
        var fileName = $"{info.ID}.json";
        var filePath = Path.Combine(legendaryFolderPath, fileName);
        MagicSet.legendaries[filePath] = info;
        var sync = new CustomSyncedValue<string>(RustyLootPlugin.ConfigSync,
            $"RustyLoot.SyncedFiles.LegendaryItems.{info.ID}");
        sync.ValueChanged += () =>
        {
            if (!ZNet.instance || ZNet.instance.IsServer()) return;

            if (string.IsNullOrEmpty(sync.Value)) return;
                
            var data = JsonConvert.DeserializeObject<LegendaryInfo>(sync.Value, deserializationSettings);
            if (data == null) return;

            info.CopyFieldsFrom(data);
            info.Update();
                
            RustyLootPlugin.LogDebug($"Updated {info.ID}");
        };
        MagicSet.syncedItems[info] = sync;
            
            
        if (File.Exists(filePath))
        {
            MagicSet.DeserializeItem(filePath);
        }
        else
        {
            var json = JsonConvert.SerializeObject(info, serializationSettings);
            File.WriteAllText(filePath, json);
        }
    }
}