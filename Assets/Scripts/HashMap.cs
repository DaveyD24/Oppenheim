using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Cheat the Unity Inspector to show <see cref="Dictionary{TKey, TValue}"/>.</summary>
[Serializable]
public class HashMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
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

	public int Num { get => Entries.Length; }

	public bool Construct(out Dictionary<TKey, TValue> HashMap)
	{
		Internal_Dictionary = new Dictionary<TKey, TValue>();
		HashMap = Internal_Dictionary; // Point to the same address.

		foreach (Entry KeyValue in Entries)
		{
			if (Internal_Dictionary.ContainsKey(KeyValue.Key))
			{
				Debug.LogError($"Key: {KeyValue.Key} already exists!");
				return false;
			}

			Internal_Dictionary.Add(KeyValue.Key, KeyValue.Value);
		}

		bIsConstructed = true;

		return true;
	}

	// C# Dictionary API.
	public bool Contains(TKey K) => Internal_Dictionary.ContainsKey(K);
	public void Push(TKey K, TValue V) => Internal_Dictionary.Add(K, V);
	public void Pull(TKey K) => Internal_Dictionary.Remove(K);
	public void Clear() => Internal_Dictionary.Clear();
	public bool TryGet(TKey K, out TValue V) => Internal_Dictionary.TryGetValue(K, out V);

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<TKey, TValue>>)Internal_Dictionary).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)Internal_Dictionary).GetEnumerator();
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