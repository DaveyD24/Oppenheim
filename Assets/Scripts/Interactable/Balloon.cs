using System;
using UnityEngine;
using UnityEngine.Extensions;
using URandom = UnityEngine.Random; // Differentiate between System.Random and UnityEngine.Random.

public class Balloon : MonoBehaviour
{
	public static Action<Balloon> OnBalloonPopped;

	[SerializeField] GameObject BoxToAttach;
	[SerializeField] Transform AttachmentPoint;
	[SerializeField, ReadOnly] Material StandardMaterial;

	[SerializeField, ReadOnly] GameObject Box;

	float RandomBobSpeed;

	void Start()
	{
		RandomBobSpeed = URandom.Range(.5f, 2.5f);

		if (!Box)
		{
			SpawnBox();
		}

		if (TryGetComponent(out MeshRenderer MR))
		{
			MR.sharedMaterial = new Material(StandardMaterial);
			MR.sharedMaterial.SetColor("_Color", URandom.ColorHSV(.6f, 1f, .8f, 1f));
		}
	}

	void Update()
	{
		// Bob up and down like a Balloon.
		transform.position += new Vector3(0f, Mathf.Sin(Time.time * RandomBobSpeed) * Time.deltaTime, 0f);
	}

	void OnTriggerEnter(Collider Other)
	{
		Pop();
	}

	public void Pop()
	{
		// Mark this Balloon for destruction.
		Destroy(gameObject);

		// Detach the Box from the String.
		Box.transform.parent = null;

		// Enable Physics on the Box.
		Box.GetOrAddComponent<Rigidbody>().useGravity = true;
		Box.GetOrAddComponent<BoxCollider>();
		
		Debug.Break();

		OnBalloonPopped?.Invoke(this);
	}

	void SpawnBox()
	{
		Box = Instantiate(BoxToAttach, AttachmentPoint.position, transform.rotation);
		Box.transform.parent = AttachmentPoint;
	}

	void OnValidate()
	{
		// Spawn a Box in Editor.
		if (!Box && AttachmentPoint && BoxToAttach)
		{
			SpawnBox();
		}
	}
}
