using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FrameOverride : MonoBehaviour {
	public bool write;
	public string frameName;
	public int frameIdx;
	public float a;
	public float b;
	public float c;
	public float d;
	public float tx;
	public float ty;
	public Color colorMult;
	public Color colorAdd;
	public bool copy;

	public AnimDef.Kanim.Frame.Element Override(AnimDef.Kanim.Frame.Element element, int currentFrame) {
		if (copy) {
			copy = false;
			string s = $"new AnimatedState() {'{'} frame = {frameIdx}, a = {a}, b = {b}, c = {c}, d = {d}, tx = {tx}, ty = {ty} {'}'}";
			GUIUtility.systemCopyBuffer = s;
		}

		frameIdx = currentFrame;

		if (element.name == frameName) {
			if (isActiveAndEnabled && write) {
				var newElement = new AnimDef.Kanim.Frame.Element();

				newElement.a =  a;
				newElement.b =  b;
				newElement.c =  c;
				newElement.d =  d;
				newElement.tx = tx;
				newElement.ty = ty;
				newElement.colorMult = colorMult;
				newElement.colorAdd = colorAdd;
				newElement.frame = element.frame;
				newElement.name = element.name;

				return newElement;
			} else {
				a = element.a;
				b = element.b;
				c = element.c;
				d = element.d;
				tx = element.tx;
				ty = element.ty;
				colorMult = element.colorMult;
				colorAdd = element.colorAdd;
				return element;
			}
		} else {
			return element;
		}
	}

#if UNITY_EDITOR
	private void OnValidate() {
		if (write && isActiveAndEnabled && Application.isPlaying) {
			KanimViewer.updateFrame = true;
		}
	}
#endif
}
