using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Extensions;

/// <summary>
/// An Audio Controller for Oppenheim.
/// </summary>
public class AudioController : MonoBehaviour
{
	[SerializeField] HashMap<string, AudioData> HashMap;
	Dictionary<string, AudioData> Map;

	void Awake()
	{
		HashMap.Construct(out Map);
	}

	/// <summary>Plays <paramref name="SoundName"/> with <paramref name="Options"/>.</summary>
	/// <param name="SoundName">The name of the Sound to play.</param>
	/// <param name="Options">The options to play <paramref name="SoundName"/>
	/// either <see cref="EAudioPlayOptions.Global"/>ly,<br></br>
	/// <see cref="EAudioPlayOptions.AtTransformPosition"/>,
	/// or to <see cref="EAudioPlayOptions.FollowEmitter"/>,<br></br>
	/// and defines whether or not to destroy the AudioSource.</param>
	/// <returns>The <see cref="AudioSource"/> used to play <paramref name="SoundName"/>.</returns>
	public AudioSource Play(string SoundName, EAudioPlayOptions Options)
	{
		byte OptionsAsByte = (byte)Options;
		bool bDestroyOnEnd = OptionsAsByte >> 3 == 1;

		OptionsAsByte &= 7;

		if ((OptionsAsByte & 7) == 0)
		{
			Debug.LogError("Invalid Options! Must define ONE of Global, AtTransformPosition, or FollowEmitter.");
			return null;
		}

		if ((OptionsAsByte & (OptionsAsByte - 1)) != 0)
		{
			Debug.LogError("Invalid Options! Must define ONE of Global, AtTransformPosition, or FollowEmitter.");
			return null;
		}

		if ((OptionsAsByte & (byte)EAudioPlayOptions.Global) == (byte)EAudioPlayOptions.Global)
		{
			return Play(SoundName);
		}
		else if ((OptionsAsByte & (byte)EAudioPlayOptions.AtTransformPosition) == (byte)EAudioPlayOptions.AtTransformPosition)
		{
			Spawn(out GameObject AtPosition, false);
			return PlayAtTransformPosition(SoundName, AtPosition, bDestroyOnEnd);
		}
		else if ((OptionsAsByte & (byte)EAudioPlayOptions.FollowEmitter) == (byte)EAudioPlayOptions.FollowEmitter)
		{
			return PlayFollow(SoundName, gameObject, bDestroyOnEnd);
		}

		Debug.LogError($"Could not play {SoundName}!");
		return null;
	}

	/// <summary>Plays <paramref name="SoundName"/> globally (3D Spatial Sound is set to 2D).</summary>
	/// <param name="SoundName">The name of the Sound to play.</param>
	public AudioSource Play(string SoundName)
	{
		Spawn(out GameObject Spawned, true);
		AudioSource Global = PlayFollow(SoundName, Spawned, true);

		if (Global != null)
		{
			Global.spatialBlend = 0;
		}

		return Global;
	}

	/// <summary>Plays <paramref name="SoundName"/> at the world position of <paramref name="Emitter"/>.</summary>
	/// <param name="SoundName">The name of the Sound to play.</param>
	/// <param name="Emitter">The <see cref="Transform.position"/> <paramref name="SoundName"/> will play at.</param>
	/// <param name="bDestroyOnEnd">
	/// <see langword="true"/> to Destroy the new <see cref="GameObject"/>
	/// once <paramref name="SoundName"/> finishes playing.</param>
	public AudioSource PlayAtTransformPosition(string SoundName, GameObject Emitter, bool bDestroyOnEnd)
	{
		if (Get(SoundName, out AudioData A))
		{
			A.Construct(Emitter, out AudioSource Source);

			Emitter.transform.position = transform.position;
			Source.Play();

			if (bDestroyOnEnd)
				Destroy(Source, A.ClipDuration());

			return Source;
		}

		return null;
	}

	/// <summary>Plays <paramref name="SoundName"/> from <paramref name="Emitter"/>.</summary>
	/// <param name="SoundName">The name of the Sound to play.</param>
	/// <param name="Emitter">The <see cref="GameObject"/> <paramref name="SoundName"/> will play from.</param>
	/// <param name="bDestroyComponentOnEnd">
	/// <see langword="true"/> to destroy the newly added <see cref="AudioSource"/> component
	/// once <paramref name="SoundName"/> finishes playing.
	/// </param>
	public AudioSource PlayFollow(string SoundName, GameObject Emitter, bool bDestroyComponentOnEnd)
	{
		if (Get(SoundName, out AudioData A))
		{
			A.Construct(Emitter, out AudioSource Source);
			Source.Play();

			if (bDestroyComponentOnEnd)
				Destroy(Source, A.ClipDuration());

			return Source;
		}

		return null;
	}

	void Spawn(out GameObject Spawned, bool bTieToThis)
	{
		Spawned = new GameObject();

		if (bTieToThis)
			Spawned.transform.parent = transform;
	}

	bool Get(string Name, out AudioData Data)
	{
		if (Map.ContainsKey(Name))
		{
			Data = Map[Name];
			return true;
		}

		Data = null;
		Debug.LogError($"{name}'s Audio Controller could not find Sound {Name}!");
		return false;
	}
}

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

	[Space]
	[Range(0, 256)] public int Priority = 128;
	[Range(0, 1)] public float Volume = 1f;
	[Range(-3, 3)] public float Pitch = 1f;
	[Range(-1, 1)] public float StereoPan = 0f;
	[Range(0, 1)] public float SpatialBlend = .75f;
	[Range(0, 1.1f)] public float ReverbZoneMix = 1f;

	/// <summary>Replicates <see cref="AudioSource"/>.</summary>
	/// <param name="Emitter">Gets or attaches an <see cref="AudioSource"/> component.</param>
	/// <param name="Source"><see langword="out"/> the attached <see cref="AudioSource"/> component.</param>
	public void Construct(GameObject Emitter, out AudioSource Source)
	{
		Source = Emitter.GetOrAddComponent<AudioSource>();

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
}

public enum EAudioPlayOptions : byte
{
	Global = 1,
	AtTransformPosition = 2,
	FollowEmitter = 4,
	DestroyOnEnd = 8
}