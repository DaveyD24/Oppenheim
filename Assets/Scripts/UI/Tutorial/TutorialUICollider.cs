using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialUICollider : MonoBehaviour
{
	[Header("Tutorial UI Settings")]
	public string Title;
	[TextArea] public string Contents;

	[Header("Popup Position Settings")]
	[SerializeField] bool bShowInCorner;
	[SerializeField] TutorialUI CornerTemplate;
	[SerializeField] TutorialUI InPlaceTemplate;
	[SerializeField] Vector3 InPlacePosition;

	TutorialUI Current;
	int PlayerCount = 0;

	const float kAVeryLongTime = 86400f; // 1 Day.

	void Start()
	{
		if (TryGetComponent(out BoxCollider BoxCollider))
		{
			BoxCollider.isTrigger = true;
		}
	}

	void Update()
	{
		if (bShowInCorner || !Current)
			return;

		Current.Rect.position = Camera.main.WorldToScreenPoint(transform.position + InPlacePosition);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			++PlayerCount;

			if (bShowInCorner)
			{
				Current = TutorialUIManager.Get().Show(Title, Contents, kAVeryLongTime);
			}
			else
			{
				ShowInPlace();
			}
		}
	}

	void OnTriggerExit(Collider other)
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

	void ShowInPlace()
	{
		TutorialUI InPlace = Instantiate(InPlaceTemplate, TutorialUIManager.Get().TutorialCanvasParent.transform);
		InPlace.Set(Title, Contents, kAVeryLongTime);

		InPlace.Rect.anchorMin = InPlace.Rect.anchorMax = InPlace.Rect.localScale = new Vector2(.5f, .5f);
		InPlace.Rect.anchoredPosition = Vector2.zero;
		InPlace.Blur.SetActive(false);

		Current = InPlace;
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position + InPlacePosition, .15f);
	}
#endif
}
