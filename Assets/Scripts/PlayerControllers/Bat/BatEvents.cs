using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BatEvents : MonoBehaviour
{
	public Action<EAnimationState> OnAnimationStateChanged;

	Bat Bat;

	Animator Animator;

	AudioSource WindGush;
	IEnumerator WindGushFalloff;

	void Awake()
	{
		OnAnimationStateChanged += OnAnimationEStateChanged;
	}

	void Start()
	{
		Bat = GetComponent<Bat>();

		Animator = GetComponent<Animator>();
	}

	// void OnTriggerEnter(Collider Other)
	// {
		// if (Other.CompareTag(Bat.Food))
		// {
		//	OnMangoCollected();
		// }
	// }

	public EAnimationState GetCurrentAnimState()
	{
		int State = Animator.GetInteger("Behaviour");
		if (State > 3)
			return EAnimationState.Fail;

		return (EAnimationState)State;
	}

	void OnAnimationEStateChanged(EAnimationState NewState)
	{
		Animator.SetInteger("Behaviour", (int)NewState);
	}

	public void OnMangoCollected()
	{
		// Debug.Log("Mango Collected!");

		// Bat.AdjustEnergy(10f);
		// Bat.AdjustHealth(10f);

		Bat.Audio.Play("Munch", EAudioPlayOptions.Global | EAudioPlayOptions.DestroyOnEnd);
	}

	public void PlayGlidingWindGush()
	{
		if (WindGush != null)
		{
			StopAllCoroutines();
			WindGush.Stop();
			Destroy(WindGush);
		}

		WindGush = Bat.Audio.Play("Wind Gush", EAudioPlayOptions.FollowEmitter);
	}

	public void StopGlidingWindGush()
	{
		if (WindGush)
		{
			if (WindGushFalloff != null)
				StopCoroutine(WindGushFalloff);

			WindGushFalloff = GraduallyStopWindGush();
			StartCoroutine(WindGushFalloff);

			//if (Bat.Audio.Playing.ContainsKey("Wind Gush"))
			//	Bat.Audio.Playing.Remove("Wind Gush");
		}
	}


	IEnumerator GraduallyStopWindGush()
	{
		if (WindGush)
		{
			while (!BatMathematics.IsZero(WindGush.volume))
			{
				WindGush.volume -= Time.deltaTime;

				yield return null;
			}

			WindGush.Stop();
			Destroy(WindGush);
			WindGush = null;

			WindGushFalloff = null;
		}
	}

}

public enum EAnimationState : int
{
	StandIdle = 0,
	Walking = 1,
	WingedFlight = 2,
	Gliding = 3,
	Fail = -1
}