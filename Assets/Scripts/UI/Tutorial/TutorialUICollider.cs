using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialUICollider : MonoBehaviour
{
	[Header("Tutorial UI Settings")]
	public string Title;
	[TextArea] public string Contents;

	[Header("Popup Position Settings")]
	[SerializeField] bool bShowInCorner;

	[Tooltip("If true, the title must match the ID of the control sprite from the control sprite map on the in place template")]
	[SerializeField] bool bShowControls;
	[SerializeField] string controlsTitle; // the title to display for the controls text

	[SerializeField] TutorialUI CornerTemplate;
	[SerializeField] TutorialUI InPlaceTemplate;
	[SerializeField] Vector3 InPlacePosition;

	TutorialUI Current;
	int PlayerCount = 0;

	const float kAVeryLongTime = 86400f; // 1 Day.

	Camera MainCamera;

	void Start()
	{
		if (TryGetComponent(out BoxCollider BoxCollider))
		{
			BoxCollider.isTrigger = true;
		}

		MainCamera = Camera.main;
	}

	void Update()
	{
		if (bShowInCorner || !Current)
		{
			return;
		}

		Current.Rect.position = MainCamera.WorldToScreenPoint(transform.position + InPlacePosition);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Current == null && other.CompareTag("Player"))
		{
			++PlayerCount;

			if (bShowInCorner)
			{
				Current = TutorialUIManager.Get().Show(Title, Contents, kAVeryLongTime, bShowControls, controlsTitle);
			}
			else
			{
				ShowInPlace();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			--PlayerCount;

			if (Current && PlayerCount == 0)
			{
				Destroy(Current.gameObject);
				Current = null;
			}
		}
	}

	private void ShowInPlace()
	{
		TutorialUI InPlace = Instantiate(InPlaceTemplate, TutorialUIManager.Get().TutorialCanvasParent.transform);
		InPlace.Set(Title, Contents, kAVeryLongTime, bShowControls, controlsTitle);

		InPlace.Rect.anchorMin = InPlace.Rect.anchorMax = InPlace.Rect.localScale = new Vector2(.5f, .5f);
		InPlace.Rect.anchoredPosition = Vector2.zero;
		InPlace.Blur.SetActive(false);

		Current = InPlace;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position + InPlacePosition, .15f);
	}
#endif
}
