using System.Collections;
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
	[SerializeField] float TimeToTarget = 2.5f;


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
			Rect.anchoredPosition = Vector2.Lerp(Origin, Target, Interpolate.Ease(InEquation, 0, 1, t));
		}
		else
		{
			Rect.anchoredPosition = Vector2.Lerp(Rect.anchoredPosition, Origin, Interpolate.Ease(OutEquation, 0, 1, t));
		}
	}

	public void Set(string TitleText, string ContentsText, float Duration = 10f)
	{
		Title.text = TitleText;
		Contents.text = ContentsText;

		Invoke(nameof(Hide), Duration);
		Invoke(nameof(Destroy), Duration + 1f);
	}

	public void Show() { bInterpolatable = true; bIsShowing = true; t = 0f; }

	public void Hide() { bIsShowing = false; t = 0f; }

	void Destroy()
	{
		Destroy(gameObject);
	}
}
