using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSystem;

public class KeySpriteMaps : MonoBehaviour
{
    [SerializeField] private HashMap<string, HashMap<string, Sprite>> controlSpriteMap;
    private Dictionary<string, Dictionary<string, Sprite>> controlSpriteDictionary = new Dictionary<string, Dictionary<string, Sprite>>();

    private void Awake()
    {
        // convert the editor's version of a hashmap into a dictionary for use in this script
        Dictionary<string, HashMap<string, Sprite>> d;
        controlSpriteMap.Construct(out d);

        foreach (var item in d)
        {
            Dictionary<string, Sprite> dd;
            item.Value.Construct(out dd);

            controlSpriteDictionary.Add(item.Key, dd);
        }
    }

    private Sprite GetSprite(string control, string name)
    {
        return controlSpriteDictionary[control][name];
    }

    private void OnEnable()
    {
        UIEvents.OnGetControlSprite += GetSprite;
    }

    private void OnDisable()
    {
        UIEvents.OnGetControlSprite -= GetSprite;
    }
}
