using System;
using UnityEngine;
using UnityEngine.Extensions;
using URandom = UnityEngine.Random; // Differentiate between System.Random and UnityEngine.Random.

public class Balloon : MonoBehaviour
{
	[SerializeField] GameObject BoxToAttach;
	[SerializeField] Transform AttachmentPoint;
	[SerializeField] bool bRandomiseColour;
	[SerializeField] Color BalloonColour;

	[SerializeField, ReadOnly] Material StandardMaterial;

	[SerializeField, Tooltip("The attached Box at the end of the String."), ReadOnly] GameObject Box;

	float RandomBobSpeed;

	void Start()
	{
		RandomBobSpeed = URandom.Range(.5f, 2.5f);

		if (!Box)
		{
			SpawnBox();
		}
	}

	void Update()
	{
		// Bob up and down like a Balloon.
		transform.position += new Vector3(0f, Mathf.Sin(Time.time * RandomBobSpeed) * Time.deltaTime, 0f);
	}

	void OnTriggerEnter(Collider Other)
	{
		// TODO: Set conditions for a Pop.
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

		if (!Application.isPlaying && TryGetComponent(out MeshRenderer MR))
		{
			MR.sharedMaterial = new Material(StandardMaterial);

			MR.sharedMaterial.SetColor("_Color",
				bRandomiseColour
					? URandom.ColorHSV(.1f, .9f, 1f, 1f)
					: BalloonColour
			);
		}
	}
}
