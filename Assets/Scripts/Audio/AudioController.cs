using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// An Audio Controller for Oppenheim.
/// </summary>
public class AudioController : MonoBehaviour
{
	[SerializeField] HashMap<string, AudioData> HashMap;
	Dictionary<string, AudioData> Map;
	Dictionary<int, AudioSource> Unique;

	void Awake()
	{
		HashMap.Construct(out Map);
		Unique = new Dictionary<int, AudioSource>();

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
		if ((OptionsAsByte & (byte)EAudioPlayOptions.Global) == (byte)EAudioPlayOptions.Global)
		{
			if (OptionsAsByte > (byte)EAudioPlayOptions.Global)
			{
				Debug.LogError($"{EAudioPlayOptions.Global} takes precedence over others. Use one or the other.");
			}

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
		Spawn(out GameObject Spawned, true);
		AudioSource Global = PlayFollow(SoundName, Spawned, true, PlaybackLead);

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
				Destroy(Emitter, A.ClipDuration());

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
		Spawned = new GameObject($"{name}");
		if (SceneManager.sceneCount > 1)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
				// as the transition scene is only temporary need to move them off it if they appear on it
				Scene scene = SceneManager.GetSceneAt(i);
				if (scene.name != "TransitionScene")
                {
					SceneManager.MoveGameObjectToScene(Spawned, scene);
                }
            }
        }

		if (bTieToThis)
        {
            Spawned.transform.parent = transform;
        }
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
