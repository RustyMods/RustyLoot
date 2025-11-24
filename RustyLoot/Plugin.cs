using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EpicLootAPI;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
using UnityEngine;

namespace RustyLoot;

public enum Toggle
{
    On = 1,
    Off = 0
}
[BepInDependency("RustyMods.Seasonality", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("randyknapp.mods.epicloot")]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class RustyLootPlugin : BaseUnityPlugin
{
    internal const string ModName = "RustyLoot";
    internal const string ModVersion = "0.0.01";
    internal const string Author = "RustyMods";
    private const string ModGUID = Author + "." + ModName;
    private static readonly string ConfigFileName = ModGUID + ".cfg";
    private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
    internal static string ConnectionError = "";
    private readonly Harmony _harmony = new(ModGUID);
    public static readonly ManualLogSource RustyLootLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
    private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
    
    private static ConfigEntry<Toggle> _serverConfigLocked = null!;
    public static RustyLootPlugin instance = null!;
    public static GameObject root = null!;

    public void Awake()
    {
        instance = this;
        _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
        _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

        root = new GameObject("RustyLoot_Root");
        DontDestroyOnLoad(root);
        root.SetActive(false);

        Localizer.Load();
        
        RustyLootLogger.LogWarning("BETA RustyLoot is loaded!");
        RustyLootLogger.LogWarning("Enchanting items with RustyLoot effects, then removing mod, will result in empty enchantments");
        
        EpicLoot.logger.OnWarning += RustyLootLogger.LogWarning;
        EpicLoot.logger.OnError += RustyLootLogger.LogError;
        // EpicLoot.logger.OnDebug += RustyLootLogger.LogDebug;


        SetupMagicEffects();
        
        MagicAbilities.Setup();
        
        Assembly assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
        SetupWatcher();
        DefinitionExtensions.SetupMagicEffectWatcher();
    }

    public void SetupMagicEffects()
    {
        PiercingShot.Setup();
        MagicProjectiles.SetupMagicArrow();
        MagicProjectiles.SetupMagicBolt();
        Seasonality.Setup();
        MistVision.Setup();
        ModifyTrinketDuration.Setup();
        SeaWolf.Setup();
        Rejuvenate.Setup();
        IronMaiden.Setup();
        ModifyAdrenaline.Setup();
        ModifyAdrenalineCost.Setup();
        Honeybound.Setup();
        AddArmor.Setup();
        Lifebloom.Setup();
    }

    private void OnDestroy()
    {
        Config.Save();
    }

    private void SetupWatcher()
    {
        FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(ConfigFileFullPath)) return;
        try
        {
            RustyLootLogger.LogDebug("ReadConfigValues called");
            Config.Reload();
        }
        catch
        {
            RustyLootLogger.LogError($"There was an issue loading your {ConfigFileName}");
            RustyLootLogger.LogError("Please check your config entries for spelling and format!");
        }
    }

    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigDescription extendedDescription =
            new(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
        ConfigEntry<T> configEntry = instance.Config.Bind(group, name, value, extendedDescription);
        SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    public static ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }

    private class ConfigurationManagerAttributes
    {
        [UsedImplicitly] public int? Order = null!;
        [UsedImplicitly] public bool? Browsable = null!;
        [UsedImplicitly] public string? Category = null!;
        [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
    }
}
