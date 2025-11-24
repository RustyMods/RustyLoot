using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RustyLoot;

public class Clone
{
    internal static readonly Dictionary<string, GameObject> registeredPrefabs = new();
    private GameObject? Prefab;
    private readonly string PrefabName;
    private readonly string NewName;
    private bool Loaded;
    private GameObject? Source;
    public event Action<GameObject>? OnCreated;

    public Clone(string prefabName, string newName)
    {
        PrefabName = prefabName;
        NewName = newName;
        PrefabManager.Clones.Add(this);
    }

    public Clone(GameObject source, string newName)
    {
        Source = source;
        NewName = newName;
    }

    internal void Create()
    {
        if (Loaded) return;

        if (Source is null)
        {
            if (Helpers.GetPrefab(PrefabName) is not { } prefab) return;
            Prefab = Object.Instantiate(prefab, RustyLootPlugin.root.transform, false);
        }
        else
        {
            Prefab = Object.Instantiate(Source, RustyLootPlugin.root.transform, false);
        }
        
        Prefab.name = NewName;
        PrefabManager.RegisterPrefab(Prefab);
        OnCreated?.Invoke(Prefab);
        registeredPrefabs[Prefab.name] = Prefab;
        Loaded = true;
    }
}