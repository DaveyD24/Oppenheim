using UnityEngine;

public class FuelBackgroundPlacement : MonoBehaviour
{
	[SerializeField] PlayerController Target;
	[SerializeField] Vector3 ScreenOffset;
	Vector3 OriginalScreenOffset;

	Vector3 SmoothVelocity;

	void Start()
	{
		OriginalScreenOffset = ScreenOffset;
	}

	void Update()
	{
		if (gameObject.activeSelf)
		{
			if (Target.TrackingCamera && Target.TrackingCamera.CameraComponent)
			{
				SpringArm TrackingCamera = Target.TrackingCamera;
				Vector3 ScreenPos = TrackingCamera.CameraComponent.WorldToScreenPoint(Target.transform.localPosition);

				if (TrackingCamera.bIsSecondarySpringArm)
				{
					ScreenOffset.x = OriginalScreenOffset.x - Screen.width * .5f;
				}
				else
				{
					ScreenOffset = OriginalScreenOffset;
				}

				RectTransform RT = (RectTransform)transform;
				RT.anchoredPosition = Vector3.SmoothDamp(RT.anchoredPosition, ScreenPos + ScreenOffset, ref SmoothVelocity, .15f);
			}
		}
	}
}
