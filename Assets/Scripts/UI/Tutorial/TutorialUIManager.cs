using UnityEngine;
using EventSystem;

public class TutorialUIManager : MonoBehaviour
{
	static TutorialUIManager Instance;

	public Canvas TutorialCanvasParent;
	[SerializeField] TutorialUI CornerTemplate;
	//[SerializeField] TutorialUI FullScreenTemplate;

	TutorialUI CurrentCornerShowing;

	bool bUseBillboardEffects;

	void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError($"Ensure there is only one {nameof(TutorialUIManager)} in the scene!");
			Debug.Break();
		}
	}

	void Update()
	{
		if (!bUseBillboardEffects)
			return;


	}

	public static TutorialUIManager Get() => Instance;

	[Exec(Description = "Makes Oppenheim pop out of the top right corner to say stuff")]
	/// <summary>
	/// Makes Oppenheim pop out of the top right corner to say
	/// <paramref name="Title"/> with <paramref name="Contents"/>
	/// for <paramref name="Duration"/>.
	/// </summary>
	public TutorialUI Show(string Title, string Contents, float Duration = 10f, bool bShowInstruction = false, string controlsTitle = "")
	{
		if (CurrentCornerShowing)
			CurrentCornerShowing.Hide();

		TutorialUI Corner = Instantiate(CornerTemplate, TutorialCanvasParent.transform);
		Corner.Set(Title, Contents, Duration, bShowInstruction, controlsTitle);
		Corner.Show();

		CurrentCornerShowing = Corner;

		return Corner;
	}

	public void ShowSample()
	{
#pragma warning disable SA1118 // Parameter should not span multiple lines
		Show(
			  "Some Title",
			  "Some Text for the Tutorial scene to teach players how to play the game!\n\n" +
			  "I hope this can be a very long piece of text and the text box will automatically " +
			  "resize itself accordingly. Otherwise, I might need to adjust the Canvas Scaler " +
			  "so that everything looks better. I also hate UI and am running out of things to say.",
#pragma warning restore SA1118 // Parameter should not span multiple lines
			  4f);
	}

	public void ShowInstruction(string title, string text, float duration, bool bShowInstruction, string controlsTitle)
	{
		Show(title, text, duration, bShowInstruction, controlsTitle);
	}

	private void OnEnable()
	{
		UIEvents.OnTutorialUIPopupShow += ShowInstruction;
	}

	private void OnDisable()
	{
		UIEvents.OnTutorialUIPopupShow -= ShowInstruction;
	}
}
