using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using UnityEngine;

public class AnimDef : CommonDef {
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

	public static AnimDef LoadAnimDef(AnimDef animDef, string directory) {
		var fullFileName = directory + Path.DirectorySeparatorChar + "animation.xml";
		if (File.Exists(fullFileName)) {
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.IgnoreProcessingInstructions = true;
			XmlReader reader = XmlReader.Create(fullFileName, settings);
			XmlDocument anim = new XmlDocument();
			anim.Load(reader);
			reader.Close();

			animDef.kanims = new Kanim[anim.DocumentElement.ChildNodes.Count];

			int aIdx = 0;
			foreach (XmlNode animXml in anim.DocumentElement.ChildNodes) {
				var kanim = new Kanim();

				kanim.name = ReadString(animXml,"name");
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

						elem.name = ReadString(element,"name");
						elem.frame = ReadInt(element,"frame");
						// Parent Transform
						var p_a = ReadFloat(element,"m0_a", 1);
						var p_b = ReadFloat(element,"m0_b");
						var p_c = ReadFloat(element,"m0_c");
						var p_d = ReadFloat(element,"m0_d", 1);
						var p_tx = ReadFloat(element,"m0_tx");
						var p_ty = ReadFloat(element,"m0_ty");
						// Child Transform
						var c_a = ReadFloat(element,"m1_a", 1);
						var c_b = ReadFloat(element,"m1_b");
						var c_c = ReadFloat(element,"m1_c");
						var c_d = ReadFloat(element,"m1_d", 1);
						var c_tx = ReadFloat(element,"m1_tx");
						var c_ty = ReadFloat(element,"m1_ty");
						// P * C
						elem.a = p_a * c_a + p_c * c_b;
						elem.b = p_b * c_a + p_d * c_b;
						elem.c = p_a * c_c + p_c * c_d;
						elem.d = p_b * c_c + p_d * c_d;
						elem.tx = p_a * c_tx + p_c * c_ty + p_tx;
						elem.ty = p_b * c_tx + p_d * c_ty + p_ty;

						var r = ReadFloat(element,"c_00", 1);
						var g = ReadFloat(element,"c_11", 1);
						var b = ReadFloat(element,"c_22", 1);
						var alpha = ReadFloat(element,"c_33", 1);
						elem.colorMult = new Color(r, g, b, alpha);

						var rAdd = ReadFloat(element,"c_04");
						var gAdd = ReadFloat(element,"c_14");
						var bAdd = ReadFloat(element,"c_24");
						var aAdd = ReadFloat(element,"c_34");
						elem.colorAdd = new Color(rAdd, gAdd, bAdd, aAdd);
					}
				}
			}

			return animDef;
		}

		return null;
	}

	public static AnimDef LoadAnimDef(string directory) {
		var fullFileName = directory + Path.DirectorySeparatorChar + "animation.xml";
		if (File.Exists(fullFileName)) {
			var animDef = new AnimDef();
			animDef.InitWatcher(directory);

			if (LoadAnimDef(animDef, directory) != null) {
				// Begin watching.
				animDef.watcher.EnableRaisingEvents = true;
				return animDef;
			}
		}

		return null;
	}

	protected override void HandleFileChanged(FileSystemEventArgs e) {
		string pattern = directory + "/(.+)\\.png";
		string input = Regex.Replace(e.FullPath, "\\\\", "/");
		
		if (input == directory + "/" + "animation.xml") {
			LoadAnimDef(this, directory);
			KanimController.updatedAnimDef = this;
		}
	}
}