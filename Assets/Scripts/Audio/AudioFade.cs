﻿using System;
using System.Collections;
using UnityEngine;

[Serializable, Obsolete("Not used anymore with new Audio Assets.")]
public struct AudioFade
{
	[Tooltip("The time it takes for this Audio Clip to reach maximum Volume.")]
	public float TimeToFull;
	MonoBehaviour Emitter;
	AudioSource Source;
	AudioData Data;

	IEnumerator Fade;

	public void Construct(MonoBehaviour Emitter, AudioSource Source, AudioData Data)
	{
		this.Emitter = Emitter;
		this.Source = Source;
		this.Data = Data;
	}

	public void Execute()
	{
		if (Fade != null)
		{
			Emitter.StopCoroutine(Fade);
		}

		Fade = StartFade();
		Emitter.StartCoroutine(Fade);
	}

	IEnumerator StartFade()
	{
		float InverseTime = 1f / TimeToFull;
		float T = 0f;
		Source.volume = 0f;

		while (T <= 1f)
		{
			T += Time.deltaTime * InverseTime;
			Source.volume = Mathf.Lerp(0f, Data.Volume, T);

			yield return null;
		}
	}
}
