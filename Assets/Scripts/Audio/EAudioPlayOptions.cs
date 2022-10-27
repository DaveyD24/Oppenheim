

/// <summary>Flags that handle the behaviour of <see cref="AudioController.Play(string, EAudioPlayOptions, float)"/>.</summary>
public enum EAudioPlayOptions : byte
{
	/// <summary>Plays a sound regardless of where the Scene's <see cref="UnityEngine.AudioListener"/> is.</summary>
	Global = 1,
	/// <summary>Plays a sound at the calling GameObject's <see cref="UnityEngine.Transform.position"/>.</summary>
	AtTransformPosition = 2,
	/// <summary>Plays a sound that follows the calling GameObjec's <see cref="UnityEngine.Transform.position"/>.</summary>
	FollowEmitter = 4,
	/// <summary>
	/// Flag to automatically <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)"/> the corresponding
	/// <see cref="UnityEngine.AudioSource"/> component, or the Source <see cref="UnityEngine.GameObject"/>.
	/// </summary>
	DestroyOnEnd = 8
}