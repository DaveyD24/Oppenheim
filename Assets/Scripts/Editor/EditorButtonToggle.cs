using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AButton))]
public class EditorButtonToggle : Editor
{
	public override void OnInspectorGUI()
	{
		AButton Button = (AButton)target;

		if (GUILayout.Button("Activate Button") && Application.isPlaying)
		{
			Button.BroadcastActive(null);
		}

		if (GUILayout.Button("Deactivate Button") && Application.isPlaying)
		{
			Button.BroadcastDeactive(null);
		}

		GUILayout.Space(20);

		DrawDefaultInspector();
	}
}
