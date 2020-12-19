using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Globalization;
using UnityEngine;

public class AnimDef {
	public class Kanim {
		public class Frame {
			public class Element {
				public string name;
				public int frame;
				public float a;
				public float b;
				public float c;
				public float d;
				public float tx;
				public float ty;
				public Color colorMult;
				public Color colorAdd;
			}

			public Element[] elements;
		}

		public string name;
		public Frame[] frames;
	}
	public Kanim[] kanims;

	public static AnimDef LoadAnimDef(string directory) {
		var fullFileName = directory + Path.DirectorySeparatorChar + "animation.xml";
		if (File.Exists(fullFileName)) {
			XmlDocument anim = new XmlDocument();
			anim.Load(fullFileName);
			var animDef = new AnimDef();
			animDef.kanims = new Kanim[anim.DocumentElement.ChildNodes.Count];

			int aIdx = 0;
			foreach (XmlNode animXml in anim.DocumentElement.ChildNodes) {
				var kanim = new Kanim();

				kanim.name = animXml.Attributes.GetNamedItem("name").InnerText;
				kanim.frames = new Kanim.Frame[animXml.ChildNodes.Count];
				animDef.kanims[aIdx] = kanim;
				aIdx++;

				int i = 0;
				foreach (XmlNode frame in animXml.ChildNodes) {
					var frameObj = new Kanim.Frame();
					kanim.frames[i] = frameObj;
					frameObj.elements = new Kanim.Frame.Element[frame.ChildNodes.Count];
					i++;

					int j = 0;
					foreach (XmlNode element in frame.ChildNodes) {
						var elem = new Kanim.Frame.Element();
						frameObj.elements[j] = elem;
						j++;

						elem.name = element.Attributes.GetNamedItem("name").InnerText;
						elem.frame = int.Parse(element.Attributes.GetNamedItem("frame").InnerText);
						// Parent Transform
						var p_a = float.Parse(element.Attributes.GetNamedItem("m0_a").InnerText, CultureInfo.InvariantCulture);
						var p_b = float.Parse(element.Attributes.GetNamedItem("m0_b").InnerText, CultureInfo.InvariantCulture);
						var p_c = float.Parse(element.Attributes.GetNamedItem("m0_c").InnerText, CultureInfo.InvariantCulture);
						var p_d = float.Parse(element.Attributes.GetNamedItem("m0_d").InnerText, CultureInfo.InvariantCulture);
						var p_tx = float.Parse(element.Attributes.GetNamedItem("m0_tx").InnerText, CultureInfo.InvariantCulture);
						var p_ty = float.Parse(element.Attributes.GetNamedItem("m0_ty").InnerText, CultureInfo.InvariantCulture);
						// Child Transform
						var c_a = float.Parse(element.Attributes.GetNamedItem("m1_a").InnerText, CultureInfo.InvariantCulture);
						var c_b = float.Parse(element.Attributes.GetNamedItem("m1_b").InnerText, CultureInfo.InvariantCulture);
						var c_c = float.Parse(element.Attributes.GetNamedItem("m1_c").InnerText, CultureInfo.InvariantCulture);
						var c_d = float.Parse(element.Attributes.GetNamedItem("m1_d").InnerText, CultureInfo.InvariantCulture);
						var c_tx = float.Parse(element.Attributes.GetNamedItem("m1_tx").InnerText, CultureInfo.InvariantCulture);
						var c_ty = float.Parse(element.Attributes.GetNamedItem("m1_ty").InnerText, CultureInfo.InvariantCulture);
						// P * C
						elem.a = p_a * c_a + p_c * c_b;
						elem.b = p_b * c_a + p_d * c_b;
						elem.c = p_a * c_c + p_c * c_d;
						elem.d = p_b * c_c + p_d * c_d;
						elem.tx = p_a * c_tx + p_c * c_ty + p_tx;
						elem.ty = p_b * c_tx + p_d * c_ty + p_ty;

						var r = float.Parse(element.Attributes.GetNamedItem("c_00").InnerText, CultureInfo.InvariantCulture);
						var g = float.Parse(element.Attributes.GetNamedItem("c_11").InnerText, CultureInfo.InvariantCulture);
						var b = float.Parse(element.Attributes.GetNamedItem("c_22").InnerText, CultureInfo.InvariantCulture);
						var alpha = float.Parse(element.Attributes.GetNamedItem("c_33").InnerText, CultureInfo.InvariantCulture);
						elem.colorMult = new Color(r, g, b, alpha);

						var rAdd = float.Parse(element.Attributes.GetNamedItem("c_04").InnerText, CultureInfo.InvariantCulture);
						var gAdd = float.Parse(element.Attributes.GetNamedItem("c_14").InnerText, CultureInfo.InvariantCulture);
						var bAdd = float.Parse(element.Attributes.GetNamedItem("c_24").InnerText, CultureInfo.InvariantCulture);
						var aAdd = float.Parse(element.Attributes.GetNamedItem("c_34").InnerText, CultureInfo.InvariantCulture);
						elem.colorAdd = new Color(rAdd, gAdd, bAdd, aAdd);
					}
				}
			}

			return animDef;
		}

		return null;
	}
}