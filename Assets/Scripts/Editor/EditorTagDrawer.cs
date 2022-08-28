using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TagAttribute))]
public class TagDrawer : PropertyDrawer
{
	public override void OnGUI(Rect Rect, SerializedProperty Property, GUIContent Label)
	{
		EditorGUI.BeginProperty(Rect, Label, Property);

		if (Property.propertyType == SerializedPropertyType.String)
		{
			List<string> Tags = new List<string>();
			Tags.Add("Untagged");
			Tags.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

			StringComparison Comparer = StringComparison.Ordinal;

			string PropAsString = Property.stringValue;
			int i = 0;
			for (int k = 1; k < Tags.Count; ++k)
			{
				if (Tags[k].Equals(PropAsString, Comparer))
				{
					i = k;
					break;
				}
			}

			int SelectedIndex = EditorGUI.Popup(Rect, Label.text, i, Tags.ToArray());

			string InspectorValue = SelectedIndex > 0 ? Tags[SelectedIndex].ToString() : string.Empty;

			if (!Property.stringValue.Equals(InspectorValue, Comparer))
			{
				Property.stringValue = InspectorValue;
			}
		}

		EditorGUI.EndProperty();
	}
}
