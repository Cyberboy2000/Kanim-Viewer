using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Reflection;

[ExecuteInEditMode]
public class ColorPickerImage : Image {
	[System.Serializable]
	public enum Component {
		red,
		green,
		blue
	}

	public Component component;
	FieldInfo colorsField;
	FieldInfo vertexField;

	protected override void OnEnable() {
		base.OnEnable();

		colorsField = typeof(VertexHelper).GetField("m_Colors", BindingFlags.NonPublic | BindingFlags.Instance);
		vertexField = typeof(VertexHelper).GetField("m_Positions", BindingFlags.NonPublic | BindingFlags.Instance);

		SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper toFill) {
		base.OnPopulateMesh(toFill);

		List<UIVertex> vertices = new List<UIVertex>();

		List<Color32> colors = (List<Color32>)colorsField.GetValue(toFill);
		List<Vector3> positions = (List<Vector3>)vertexField.GetValue(toFill);

		var r = GetPixelAdjustedRect();

		for (int i = 0; i < positions.Count; i++) {
			Vector3 p = positions[i];
			float v = (p.x - r.x) / r.width;

			Color32 c = colors[i];

			if (component == Component.red) {
				c.r = (byte)(255 * v);
			} else if (component == Component.green) {
				c.g = (byte)(255 * v);
			} else  {
				c.b = (byte)(255 * v);
			}

			colors[i] = c;
		}
	}
}
