using System;
using UnityEngine;
/// <summary>A dodgy <see cref="AudioSource"/> replica.</summary>
[Serializable]
public class AudioData
{
	public AudioClip Clip;
	public bool bMute;
	public bool bBypassEffects;
	public bool bBypassReverbZones;
	public bool bPlayOnAwake;
	public bool bLoop;

	[Header("Fade")]
	public bool bUseFader;
	public AudioFade Fader;

	[Space]
	[Range(0, 256)] public int Priority = 128;
	[Range(0, 1)] public float Volume = 1f;
	[Range(-3, 3)] public float Pitch = 1f;
	[Range(-1, 1)] public float StereoPan = 0f;
	[Range(0, 1)] public float SpatialBlend = .75f;
	[Range(0, 1.1f)] public float ReverbZoneMix = 1f;

	/// <summary>Replicates attaching an <see cref="AudioSource"/> component.</summary>
	/// <param name="Emitter">Gets or attaches an <see cref="AudioSource"/> component.</param>
	/// <param name="Source"><see langword="out"/> the attached <see cref="AudioSource"/> component.</param>
	public void Construct(GameObject Emitter, out AudioSource Source)
	{
		Source = Emitter.AddComponent<AudioSource>();

		Source.clip = Clip;
		Source.mute = bMute;
		Source.bypassEffects = bBypassEffects;
		Source.bypassReverbZones = bBypassReverbZones;
		Source.playOnAwake = bPlayOnAwake;
		Source.loop = bLoop;

		Source.priority = Priority;
		Source.volume = Volume;
		Source.pitch = Pitch;
		Source.panStereo = StereoPan;
		Source.spatialBlend = SpatialBlend;
		Source.reverbZoneMix = ReverbZoneMix;
	}

	/// <summary>The duration of this <see cref="Clip"/> in seconds.</summary>
	/// <param name="BufferTimeLeeway">+- dead audio offset.</param>
	/// <returns>The duration in seconds.</returns>
	public float ClipDuration(float BufferTimeLeeway = 0f)
	{
		return Clip.length + BufferTimeLeeway;
	}

	public void IfFadeThenFade(MonoBehaviour Emitter, AudioSource Source, AudioData Data)
	{
		if (bUseFader && Fader.TimeToFull != 0f)
		{
			Fader.Construct(Emitter, Source, Data);
			Fader.Execute();
		}
	}
}
