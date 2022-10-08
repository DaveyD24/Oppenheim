using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Audio Controller for Oppenheim.
/// </summary>
public class AudioController : MonoBehaviour
{
	[SerializeField] HashMap<string, AudioData> HashMap;
	Dictionary<string, AudioData> Map;
	Dictionary<int, AudioSource> Unique;
	Dictionary<string, PlayingData> Playing;

	void Awake()
	{
		HashMap.Construct(out Map);
		Unique = new Dictionary<int, AudioSource>();
		Playing = new Dictionary<string, PlayingData>();

		foreach (KeyValuePair<string, AudioData> PlayOnAwake in HashMap)
		{
			if (PlayOnAwake.Value.bPlayOnAwake)
			{
				Play(PlayOnAwake.Key);
			}
		}
	}

	/// Called with <see cref="UnityEngine.Events.UnityEvent"/> through <see cref="Interactable"/>.
	public void PlayFromInteractable(string SoundName)
	{
		EAudioPlayOptions Options = EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd;
		Play(SoundName, Options);
	}

	/// <summary>Plays <paramref name="SoundName"/> at <paramref name="Position"/>.</summary>
	/// <param name="SoundName">The name of the sound to play.</param>
	/// <param name="Position">The world coordinates of where this <paramref name="SoundName"/> should play.</param>
	/// <param name="PlaybackLead">The time in seconds to fast forward.</param>
	public AudioSource PlayAtPosition(string SoundName, Vector3 Position, float PlaybackLead = 0f)
	{
		if (Get(SoundName, out AudioData Data))
		{
			Spawn(out GameObject Spawned, false);

			Spawned.transform.position = Position;
			Data.Construct(Spawned, out AudioSource Source);

			Source.time = PlaybackLead;
			Source.Play();
			Destroy(Spawned, Data.ClipDuration());

			return Source;
		}

		return null;
	}

	/// <summary>Plays one instance of <paramref name="SoundName"/> with <paramref name="Options"/>.</summary>
	/// <remarks><b>Uniqueness is relative to this instance of <see cref="AudioController"/>, and NOT UNIQUE globally!</b></remarks>
	/// <param name="PlaybackLead">The time in seconds to fast forward.</param>
	public AudioSource PlayUnique(string SoundName, EAudioPlayOptions Options, float PlaybackLead = 0f)
	{
		int HashCode = SoundName.GetHashCode();
		bool bAllowedToPlay = true;

		// If this sound was uniquely played before.
		if (Unique.ContainsKey(HashCode))
		{
			AudioSource Source = Unique[HashCode];

			// If the Source has been destroyed, or is no longer playing, it is allowed to play.
			if (!Source || !Source.isPlaying)
			{
				Unique.Remove(HashCode);
			}
			// Otherwise, the sound still playing and is not unique.
			else
			{
				bAllowedToPlay = false;
			}
		}

		if (!bAllowedToPlay)
			return null;

		MaskOptions(Options, out byte OptionsAsByte, out bool bDesroyOnEnd);

		if (IsIllegalInput(OptionsAsByte))
			return null;

		AudioSource UniqueSource = PlayAudioSignature(SoundName, OptionsAsByte, bDesroyOnEnd, PlaybackLead);
		Unique.Add(HashCode, UniqueSource);

		return UniqueSource;
	}

	/// <summary>Plays <paramref name="SoundName"/> with <paramref name="Options"/>.</summary>
	/// <param name="SoundName">The name of the Sound to play.</param>
	/// <param name="Options">The options to play <paramref name="SoundName"/>
	/// either <see cref="EAudioPlayOptions.Global"/>ly,<br></br>
	/// <see cref="EAudioPlayOptions.AtTransformPosition"/>,
	/// or to <see cref="EAudioPlayOptions.FollowEmitter"/>,<br></br>
	/// and defines whether or not to destroy the AudioSource.</param>
	/// <param name="PlaybackLead">The time in seconds to fast forward.</param>
	/// <returns>The <see cref="AudioSource"/> used to play <paramref name="SoundName"/>.</returns>
	public AudioSource Play(string SoundName, EAudioPlayOptions Options, float PlaybackLead = 0f)
	{
		MaskOptions(Options, out byte OptionsAsByte, out bool bDestroyOnEnd);

		return PlayAudioSignature(SoundName, OptionsAsByte, bDestroyOnEnd, PlaybackLead);
	}

	AudioSource PlayAudioSignature(string SoundName, byte OptionsAsByte, bool bDestroyOnEnd, float PlaybackLead = 0f)
	{
		PrunePlaying();

		if ((OptionsAsByte & (byte)EAudioPlayOptions.Global) == (byte)EAudioPlayOptions.Global)
		{
			return Play(SoundName, PlaybackLead);
		}
		else if ((OptionsAsByte & (byte)EAudioPlayOptions.AtTransformPosition) == (byte)EAudioPlayOptions.AtTransformPosition)
		{
			Spawn(out GameObject AtPosition, false);
			return PlayAtTransformPosition(SoundName, AtPosition, bDestroyOnEnd, PlaybackLead);
		}
		else if ((OptionsAsByte & (byte)EAudioPlayOptions.FollowEmitter) == (byte)EAudioPlayOptions.FollowEmitter)
		{
			return PlayFollow(SoundName, gameObject, bDestroyOnEnd, PlaybackLead);
		}

		Debug.LogError($"Could not play {SoundName}!");
		return null;
	}

	/// <summary>Plays <paramref name="SoundName"/> globally (3D Spatial Sound is set to 2D).</summary>
	/// <param name="SoundName">The name of the Sound to play.</param>
	/// <param name="PlaybackLead">The time in seconds to fast forward.</param>
	public AudioSource Play(string SoundName, float PlaybackLead = 0f)
	{
		PrunePlaying();

		Spawn(out GameObject Spawned, true);
		AudioSource Global = PlayFollow(SoundName, Spawned, true, PlaybackLead);

		if (Global != null)
		{
			Global.spatialBlend = 0;
		}

		return Global;
	}

	/// <summary>Stops <paramref name="SoundName"/> if it is playing.</summary>
	/// <param name="SoundName"></param>
	public void Stop(string SoundName)
	{
		PrunePlaying();

		if (Playing.ContainsKey(SoundName))
		{
			PlayingData Data = Playing[SoundName];
			Data.Source.Stop();

			if (Data.bIsStandalone)
			{
				// Destroy the GameObject (PlayAtTransformPosition).
				Destroy(Data.Source.gameObject);
			}
			else
			{
				// Destroy the AudioSource Component (PlayFollow).
				Destroy(Data.Source);
			}
		}
	}

	/// <summary>Plays <paramref name="SoundName"/> at the world position of <paramref name="Emitter"/>.</summary>
	/// <param name="SoundName">The name of the Sound to play.</param>
	/// <param name="Emitter">The <see cref="Transform.position"/> <paramref name="SoundName"/> will play at.</param>
	/// <param name="bDestroyOnEnd">
	/// <see langword="true"/> to Destroy the new <see cref="GameObject"/>
	/// once <paramref name="SoundName"/> finishes playing.</param>
	/// <param name="PlaybackLead">The time in seconds to fast forward.</param>
	AudioSource PlayAtTransformPosition(string SoundName, GameObject Emitter, bool bDestroyOnEnd, float PlaybackLead = 0f)
	{
		if (Get(SoundName, out AudioData A))
		{
			A.Construct(Emitter, out AudioSource Source);

			Emitter.transform.position = transform.position;
			Source.time = PlaybackLead;
			Source.Play();

			if (bDestroyOnEnd && !Source.loop)
				Destroy(Source, A.ClipDuration());

			Playing.Add(SoundName, new PlayingData(A, Source, false));

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
	/// <param name="PlaybackLead">The time in seconds to fast forward.</param>
	AudioSource PlayFollow(string SoundName, GameObject Emitter, bool bDestroyComponentOnEnd, float PlaybackLead = 0f)
	{
		if (Get(SoundName, out AudioData A))
		{
			A.Construct(Emitter, out AudioSource Source);
			Source.time = PlaybackLead;
			Source.Play();

			if (bDestroyComponentOnEnd && !Source.loop)
				Destroy(Source, A.ClipDuration());

			Playing.Add(SoundName, new PlayingData(A, Source, true));

			return Source;
		}

		return null;
	}

	static void MaskOptions(EAudioPlayOptions Options, out byte OptionsAsByte, out bool bDestroyOnEnd)
	{
		OptionsAsByte = (byte)Options;
		bDestroyOnEnd = OptionsAsByte >> 3 == 1;
		OptionsAsByte &= 7;
	}

	static bool IsIllegalInput(byte OptionsAsByte)
	{
		if ((OptionsAsByte & 7) == 0)
		{
			Debug.LogError("Invalid Options! Must define ONE of Global, AtTransformPosition, or FollowEmitter.");
			return true;
		}

		return false;
	}

	/// <summary>Spawns a new GameObject.</summary>
	/// <param name="Spawned">Outs the new GameObject.</param>
	/// <param name="bTieToThis"><see langword="true"/> to parent <paramref name="Spawned"/> to this <see cref="AudioController"/>.</param>
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

	void PrunePlaying()
	{
		List<string> SoundsToPrune = new List<string>();

		foreach (KeyValuePair<string, PlayingData> P in Playing)
			if (P.Value.HasElapsed())
				SoundsToPrune.Add(P.Key);

		foreach (string S in SoundsToPrune)
			Playing.Remove(S);
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
}

struct PlayingData
{
	public AudioSource Source;
	public bool bIsStandalone;
	float StartTime;
	AudioData Data;

	public PlayingData(AudioData Data, AudioSource Source, bool bIsStandalone)
	{
		this.Source = Source;
		this.bIsStandalone = bIsStandalone;
		StartTime = Time.time;
		this.Data = Data;
	}

	public bool HasElapsed()
	{
		// Looped Sounds must be terminated manually.
		return !Data.bLoop && Time.time - StartTime >= Data.ClipDuration();
	}
}

public enum EAudioPlayOptions : byte
{
	Global = 1,
	AtTransformPosition = 2,
	FollowEmitter = 4,
	DestroyOnEnd = 8
}