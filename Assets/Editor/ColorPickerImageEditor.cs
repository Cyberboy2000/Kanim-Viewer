using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ColorPickerImage))]
public class ColorPickerImageEditor : ImageEditor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();

		var enumProperty = serializedObject.FindProperty("component");

		int i = EditorGUILayout.Popup("Component",enumProperty.enumValueIndex,enumProperty.enumDisplayNames);
		enumProperty.enumValueIndex = i;
		serializedObject.ApplyModifiedProperties();
	}
}
