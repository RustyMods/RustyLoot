using System.Reflection;
using UnityEngine;

namespace RustyLoot;

public static class SpriteManager
{
    public static Sprite? RegisterSprite(string fileName, string folderName = "icons")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string path = $"{RustyLootPlugin.ModName}.{folderName}.{fileName}";
        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        _ = stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        Sprite? sprite = texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
        if (sprite != null) sprite.name = fileName;
        return sprite;
    }
}