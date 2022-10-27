using UnityEngine;

public struct PlayingData
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
