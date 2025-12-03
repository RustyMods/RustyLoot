using HarmonyLib;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RustyLoot;

[HarmonyPatch(typeof(DamageText), nameof(DamageText.Awake))]
public static class DamageText_Awake_Patch
{
    private static void Postfix(DamageText __instance)
    {
        __instance.gameObject.AddComponent<WorldText>();
    }
}

public class WorldText : MonoBehaviour
{
    public DamageText m_damageText = null!;
    public float m_textDuration = 1.5f;
    public float m_maxTextDistance = 30f;
    public float m_largeFontSize = 18f;
    public float m_smallFontSize = 14f;
    public float m_smallFontDistance = 10f;
    public GameObject m_worldTextBase = null!;

    public static WorldText? instance;

    public void Awake()
    {
        m_damageText = GetComponent<DamageText>();
        m_worldTextBase = m_damageText.m_worldTextBase;
        ZRoutedRpc.instance.Register<ZPackage>(nameof(RPC_AddWorldText), RPC_AddWorldText);
        instance = this;
    }

    public void OnDestroy()
    {
        instance = null;
    }

    public void ShowText(Vector3 pos, string text, Color color)
    {
        ZPackage pkg = new();
        pkg.Write(pos);
        pkg.Write(text);
        pkg.Write(Utils.ColorToVec3(color));
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, nameof(RPC_AddWorldText), pkg);
    }

    public void RPC_AddWorldText(long sender, ZPackage pkg)
    {
        Camera mainCamera = Utils.GetMainCamera();
        if (!mainCamera || Hud.IsUserHidden()) return;
        Vector3 pos = pkg.ReadVector3();
        string text = pkg.ReadString();
        Vector3 vector3 = pkg.ReadVector3();
        Color color = Utils.Vec3ToColor(vector3);
        float distance = Vector3.Distance(mainCamera.transform.position, pos);
        if (distance > m_maxTextDistance) return;
        AddInWorldText(pos, distance, text, color);
    }

    public void AddInWorldText(Vector3 pos, float distance, string text, Color color)
    {
        if (string.IsNullOrEmpty(text) || m_damageText.m_worldTexts.Count > 200) return;
        DamageText.WorldTextInstance worldText = new()
        {
            m_duration = m_textDuration,
            m_worldPos = pos + Random.insideUnitSphere * 0.5f,
            m_gui = Instantiate(m_worldTextBase, transform)
        };
        worldText.m_textField = worldText.m_gui.GetComponent<TMP_Text>();
        m_damageText.m_worldTexts.Add(worldText);
        text = Localization.instance.Localize(text);
        worldText.m_textField.color = color;
        worldText.m_textField.fontSize = distance <= m_smallFontDistance ? m_largeFontSize : m_smallFontSize;
        worldText.m_textField.text = text;
        worldText.m_timer = 0.0f;
    }
}