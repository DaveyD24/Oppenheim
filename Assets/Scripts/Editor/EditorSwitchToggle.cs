using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Switch))]
public class EditorSwitchToggle : Editor
{
	public override void OnInspectorGUI()
	{
		Switch Switch = (Switch)target;

		if (GUILayout.Button("Toggle Switch") && Application.isPlaying)
		{
			Switch.OnTriggerEnter(null);
		}

		GUILayout.Space(20);

		DrawDefaultInspector();
	}
}
