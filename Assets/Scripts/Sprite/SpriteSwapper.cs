using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwapper : MonoBehaviour
{
    public Texture2D spriteSheetTexture;
    SpriteRenderer spriteRen;
    Dictionary<string, Sprite> spriteDict;

    // Start is called before the first frame update
    void Start()
    {
        spriteRen = GetComponent<SpriteRenderer>();
        if (spriteRen == null) Debug.LogError("Sprite renderer should be populated");

        if (spriteSheetTexture != null)
        {
            ReloadSpritesheet();
        }
        else
        {
            Debug.LogError("Sprite sheet is null!");
        }
    }

    public void ReloadSpritesheet()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(spriteSheetTexture.name);
        spriteDict = new Dictionary<string, Sprite>();
        foreach (var s in sprites)
        {
            spriteDict.Add(s.name, s);
        }
    }

    private void LateUpdate()
    {
        spriteRen.sprite = spriteDict[spriteRen.sprite.name];
    }
}
