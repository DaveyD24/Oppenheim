using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Balloon))]
public class EditorBalloonPopper : Editor
{
	public override void OnInspectorGUI()
	{
		Balloon Balloon = (Balloon)target;

		if (GUILayout.Button("Pop!") && Application.isPlaying)
		{
			Balloon.Pop();
		}

		DrawDefaultInspector();
	}
}
