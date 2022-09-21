using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Cheat the Unity Inspector to show <see cref="Dictionary{TKey, TValue}"/>.</summary>
[Serializable]
public class HashMap<TKey, TValue>
{
	[Serializable]
	public class Entry
	{
		public TKey Key;
		public TValue Value;
	}

	[SerializeField] Entry[] Entries;
	internal Dictionary<TKey, TValue> Internal_Dictionary;
	bool bIsConstructed = false;

	public bool Construct(out Dictionary<TKey, TValue> HashMap)
	{
		HashMap = new Dictionary<TKey, TValue>();
		foreach (Entry KeyValue in Entries)
		{
			if (HashMap.ContainsKey(KeyValue.Key))
			{
				Debug.LogError($"Key: {KeyValue.Key} already exists!");
				return false;
			}

			HashMap.Add(KeyValue.Key, KeyValue.Value);
		}

		bIsConstructed = true;
		Internal_Dictionary = HashMap; // Point to the same address.

		return true;
	}

	public TValue this[TKey K]
	{
		get
		{
			if (!bIsConstructed)
				return default;

			return Internal_Dictionary[K];
		}
	}
}