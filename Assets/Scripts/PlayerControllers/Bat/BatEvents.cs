using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BatEvents : MonoBehaviour
{
	public Action<EAnimationState> OnAnimationStateChanged;

	Bat Bat;

	Animator Animator;

	void Awake()
	{
		OnAnimationStateChanged += OnAnimationEStateChanged;
	}

	void Start()
	{
		Bat = GetComponent<Bat>();

		Animator = GetComponent<Animator>();
	}

	void OnTriggerEnter(Collider Other)
	{
		//if (Other.CompareTag(Bat.Food))
		//{
		//	OnMangoCollected();
		//}
	}

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

	//void OnMangoCollected()
	//{
	//	Debug.Log("Mango Collected!");

	//	// Bat.AdjustEnergy(10f);
	//	// Bat.AdjustHealth(10f);
	//}
}

public enum EAnimationState : int
{
	StandIdle = 0,
	Walking = 1,
	WingedFlight = 2,
	Gliding = 3,
	Fail = -1
}