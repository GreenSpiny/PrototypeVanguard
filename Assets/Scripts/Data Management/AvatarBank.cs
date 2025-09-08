using UnityEngine;
using System.Collections.Generic;

public class AvatarBank : MonoBehaviour
{
    [SerializeField] public List<Sprite> sprites;
    private Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
    public const string defaultAvatar = "leafy";
    public const string fallbackAvatar = "shadowarmy";

    private void Awake()
    {
        foreach (Sprite sprite in sprites)
        {
            spriteDict[sprite.name.ToLower()] = sprite;
        }
    }

    public Sprite GetSprite(string spriteName)
    {
        if (spriteName != null)
        {
            string lowerName = spriteName.ToLower();
            if (spriteDict.ContainsKey(lowerName))
            {
                return spriteDict[lowerName];
            }
        }
        if (spriteDict.ContainsKey(fallbackAvatar))
        {
            return spriteDict[fallbackAvatar];
        }
        return null;
    }

}
