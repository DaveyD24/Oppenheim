using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Extensions;
using URandom = UnityEngine.Random; // Differentiate between System.Random and UnityEngine.Random.

public class Balloon : UniqueID, IDataInterface
{
	[SerializeField] private GameObject boxToAttach;
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] Transform AttachmentPoint;
	[SerializeField] bool bRandomiseColour;
	[SerializeField] Color BalloonColour;
	[Header("Rigidbody Values")]
	[SerializeField] private float boxMass;
	[SerializeField] private float boxDrag = 1;
	[SerializeField] private float boxAngularDrag = 1.5f;
	[SerializeField] private bool bAddNewPhysics = false;

	[SerializeField, ReadOnly] Material StandardMaterial;

	[SerializeField, Tooltip("The attached Box at the end of the String.")] GameObject Box;

	private Quaternion attachedBoxRotation;

	private float RandomBobSpeed;

	private void Start()
	{
		RandomBobSpeed = URandom.Range(.5f, 2.5f);

		if (!Box)
		{
			SpawnBox();
		}

		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, boxToAttach.transform.position);

		attachedBoxRotation = boxToAttach.transform.rotation;
		Destroy(boxToAttach.GetComponent<Collider>());

		if (TryGetComponent(out MeshRenderer MR))
		{
			MR.sharedMaterial = new Material(StandardMaterial);

			MR.sharedMaterial.SetColor("_Color", bRandomiseColour ? URandom.ColorHSV(.1f, .9f, 1f, 1f) : BalloonColour);
		}
	}

	void Update()
	{
		// Bob up and down like a Balloon.
		transform.position += new Vector3(0f, Mathf.Sin(Time.time * RandomBobSpeed) * Time.deltaTime, 0f);
		GroundRay();
	}

	void OnTriggerEnter(Collider Other)
	{
		// TODO: Set conditions for a Pop.
		Pop();
		Debug.Log("popping");
	}

	public void Pop()
	{
		// Detach the Box from the String.
		boxToAttach.transform.parent = null;

		// Enable Physics on the Box.
		if (bAddNewPhysics)
		{
			Rigidbody rb = boxToAttach.GetOrAddComponent<Rigidbody>();
			rb.useGravity = true;
			rb.mass = boxMass;
			rb.angularDrag = boxAngularDrag;
			rb.drag = boxDrag;
			boxToAttach.GetOrAddComponent<BoxCollider>();
		}
		else
        {
			boxToAttach.GetComponent<Rigidbody>().isKinematic = false;
			boxToAttach.AddComponent<BoxCollider>();
		}

#if !UNITY_EDITOR
		Dictionary<string, object> eventData = new Dictionary<string, object>();
		eventData.Add("Position", transform.position.ToString());
		AnalyticsService.Instance.CustomData("BalloonPop", eventData);
		AnalyticsService.Instance.Flush();
#endif

		// Mark this Balloon for destruction.
		gameObject.SetActive(false);
	}

	private void SpawnBox()
	{
		Box = Instantiate(boxToAttach, AttachmentPoint.position, transform.rotation);
		Box.transform.parent = AttachmentPoint;
	}

	private void GroundRay()
    {
		RaycastHit hit;
		Physics.Raycast(transform.position, Vector3.down * 100, out hit);
		lineRenderer.SetPosition(1, hit.point);
		lineRenderer.SetPosition(0, boxToAttach.transform.position);
	}

#pragma warning disable SA1202 // Elements should be ordered by access
	public void LoadData(SectionData data)
#pragma warning restore SA1202 // Elements should be ordered by access
	{
		if (data.Balloons.Contains(SaveID))
		{
			gameObject.SetActive(true);

			boxToAttach.GetComponent<Rigidbody>().isKinematic = true;
			if (boxToAttach.TryGetComponent(out BoxCollider boxCollider))
            {
				Destroy(boxCollider);
            }

			boxToAttach.transform.parent = AttachmentPoint;

			boxToAttach.transform.position = AttachmentPoint.position;
			boxToAttach.transform.rotation = attachedBoxRotation;
		}
		else
		{
			// as this item has been destroyed do not enable it to exist
			gameObject.SetActive(false);

			// handle the attached box as well
			boxToAttach.GetComponent<Rigidbody>().isKinematic = false;
			boxToAttach.GetOrAddComponent<BoxCollider>();
			boxToAttach.GetComponent<PushableBox>().LoadData(data);
		}
	}

	public void SaveData(SectionData data)
	{
		if (gameObject.activeSelf)
		{
			// only save this item if it has not been destroyed
			data.Balloons.Add(SaveID);
		}
	}
}
