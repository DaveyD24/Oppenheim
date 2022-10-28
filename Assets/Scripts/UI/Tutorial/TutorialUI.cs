using System.Collections;
using EventSystem;
using UnityEngine;
using TMP = TMPro.TextMeshProUGUI;

public class TutorialUI : MonoBehaviour
{
	public RectTransform Rect;

	[SerializeField] TMP Title;
	[SerializeField] TMP Contents;
	public GameObject Blur;

	[SerializeField] EEquation InEquation;
	[SerializeField] EEquation OutEquation;
	[SerializeField] public float TimeToTarget = 2.5f;

	[SerializeField] private SetControlsUI setControlsUI;

	private bool bIsInWorld = false;

	Vector2 Origin, Target;

	bool bIsShowing = true;
	bool bInterpolatable = false;

	void Awake()
	{
		Rect = (RectTransform)transform;

		Origin = Rect.anchoredPosition;
		Target = Origin;
		Target.x *= -1f;
	}

	float t = 0;

	void Update()
	{
		if (!bInterpolatable)
			return;

		t += Time.deltaTime / TimeToTarget;

		if (bIsShowing)
		{
			if (bIsInWorld)
			{
				Rect.localScale = Vector3.Lerp(Vector3.zero, new Vector3(0.25f, 0.25f, 1), Interpolate.Ease(InEquation, 0, 1, t));
			}
			else
			{
				Rect.anchoredPosition = Vector2.Lerp(Origin, Target, Interpolate.Ease(InEquation, 0, 1, t));
			}
		}
		else
		{
			if (bIsInWorld)
			{
				Rect.localScale = Vector3.Lerp(new Vector3(0.25f, 0.25f, 1), Vector3.zero, Interpolate.Ease(OutEquation, 0, 1, t));
			}
			else
			{
				Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, Origin, Interpolate.Ease(OutEquation, 0, 1, t));
			}
		}
	}

	public void Set(string TitleText, string ContentsText, float Duration = 10f, bool bShowInstructions = false, string controlsTitle = "", bool bIsInWorld = false)
	{
		Title.text = TitleText;
		Contents.text = ContentsText;

		this.bIsInWorld = bIsInWorld;
		if (bIsInWorld)
		{
			bInterpolatable = true;
			bIsShowing = true;
		}

		Invoke(nameof(Hide), Duration);
		Invoke(nameof(Destroy), Duration + 1f);

		if (bShowInstructions)
		{
			Contents.gameObject.SetActive(false);
			string[] inputDeviceNames = UIEvents.GetInputTypes();
			if (inputDeviceNames.Length >= 2)
			{
				setControlsUI.SetControlsActive(controlsTitle, inputDeviceNames.Length, inputDeviceNames[0], inputDeviceNames[1]);
			}
			else if (inputDeviceNames.Length == 1)
			{
				setControlsUI.SetControlsActive(controlsTitle, inputDeviceNames.Length, inputDeviceNames[0], string.Empty);
			}

			Debug.Log("Setup Images Properly: " + inputDeviceNames.Length);
		}
	}

	public void Show() { bInterpolatable = true; bIsShowing = true; t = 0f; }

	public void Hide() { bIsShowing = false; t = 0f; }

	void Destroy()
	{
		Destroy(gameObject);
	}
}
