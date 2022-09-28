using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// convert and unconvert the dictionary to a format which can be saved to json.
/// </summary>
/// <typeparam name="TKey">the key of the dictionary.</typeparam>
/// <typeparam name="TValue">the value of the dictionary.</typeparam>
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public Dictionary<TKey, TValue> Dictionary
    {
        get => dictionary; set => dictionary = value;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        dictionary.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            dictionary.Add(keys[i], values[i]);
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> item in dictionary)
        {
            keys.Add(item.Key);
            values.Add(item.Value);
        }
    }
}