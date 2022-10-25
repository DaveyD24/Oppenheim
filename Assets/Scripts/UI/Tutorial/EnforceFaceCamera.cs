using UnityEngine;

/// <summary>The Face Camera Billboard Effect used in Antipede.</summary>
public class EnforceFaceCamera : MonoBehaviour
{
	[SerializeField] EFaceMethod FaceMethod;

	Transform BillboardTarget;

	void Start()
	{
		if (FaceMethod == EFaceMethod.OnStart)
			FaceMainCamera();
	}

	void Update()
	{
		if (FaceMethod == EFaceMethod.OnUpdate)
			FaceMainCamera();
	}

	void LateUpdate()
	{
		if (FaceMethod == EFaceMethod.OnLateUpdate)
			FaceMainCamera();
	}

	public void SetBillboardTarget(Transform Target)
	{
		BillboardTarget = Target;
	}

	void FaceMainCamera()
	{
		if (BillboardTarget)
			transform.LookAt(transform.position + BillboardTarget.rotation * Vector3.forward, Vector3.up);
	}
}

public enum EFaceMethod : byte
{
	OnStart, OnUpdate, OnLateUpdate
}
