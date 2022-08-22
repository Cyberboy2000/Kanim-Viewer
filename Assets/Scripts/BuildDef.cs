using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Xml;
using System.IO;
using System.Globalization;
using UnityEngine;

public class BuildDef : CommonDef {
	public class Symbol {
		public class Frame {
			public int framenum;
			public int duration;
			public string image;
			public float w;
			public float h;
			public float x;
			public float y;
			public Sprite sprite;

			public override string ToString() {
				return string.Format("<Frame framenum=\"{0}\" duration=\"{1}\" image=\"{2}\" w=\"{3}\" h=\"{4}\" x=\"{5}\" y= \"{6}\"/>", framenum, duration, image, w, h, x, y);
			}
		}
		public string name;
		public Frame[] frames;
	}

	public Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

	Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
	Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

	public static BuildDef LoadBuildDef(BuildDef buildDef, string directory) {
		var fullFileName = directory + Path.DirectorySeparatorChar + "build.xml";
		if (File.Exists(fullFileName)) {
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.IgnoreProcessingInstructions = true;
			XmlReader reader = XmlReader.Create(fullFileName, settings);
			XmlDocument build = new XmlDocument();
			build.Load(reader);
			reader.Close();

			foreach (XmlNode symbolXml in build.DocumentElement.ChildNodes) {
				var symbolObj = new Symbol();
				symbolObj.name = symbolXml.Attributes.GetNamedItem("name").InnerText;
				symbolObj.frames = new Symbol.Frame[symbolXml.ChildNodes.Count];
				buildDef.symbols[symbolObj.name.ToLower()] = symbolObj;

				int i = 0;
				foreach (XmlNode frame in symbolXml.ChildNodes) {
					var frameObj = new Symbol.Frame();

					try {
						frameObj.framenum = int.Parse(frame.Attributes.GetNamedItem("framenum").InnerText);
						frameObj.duration = int.Parse(frame.Attributes.GetNamedItem("duration").InnerText);
						frameObj.w = int.Parse(frame.Attributes.GetNamedItem("w").InnerText);
						frameObj.h = int.Parse(frame.Attributes.GetNamedItem("h").InnerText);
						frameObj.x = float.Parse(frame.Attributes.GetNamedItem("x").InnerText, CultureInfo.InvariantCulture);
						frameObj.y = float.Parse(frame.Attributes.GetNamedItem("y").InnerText, CultureInfo.InvariantCulture);
						frameObj.image = frame.Attributes.GetNamedItem("image").InnerText;

						symbolObj.frames[i] = frameObj;

						try {
							if (!buildDef.sprites.ContainsKey(frameObj.image)) {
								byte[] pngBytes = File.ReadAllBytes(directory + "/" + frameObj.image + ".png");
								Texture2D tex = new Texture2D(2, 2);
								tex.LoadImage(pngBytes);
								buildDef.textures.Add(frameObj.image, tex);
								var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 1), 1, 0, SpriteMeshType.FullRect);
								sprite.name = frameObj.image;
								buildDef.sprites.Add(frameObj.image, sprite);
							}
							frameObj.sprite = buildDef.sprites[frameObj.image];
						} catch (System.Exception ex) {
							Debug.LogError("Failed to load file " + frameObj.image + " : " + ex);
						}
					} catch (System.Exception ex) {
						Debug.LogError("Failed to parse frame " + i + " of symbol " + symbolObj.name + " : " + ex);
					}

					i++;
				}
			}

			return buildDef;
		}

		return null;
	}

	public static BuildDef LoadBuildDef(string directory) {
		var fullFileName = directory + Path.DirectorySeparatorChar + "build.xml";
		if (File.Exists(fullFileName)) {
			var buildDef = new BuildDef();
			buildDef.InitWatcher(directory);

			if (LoadBuildDef(buildDef, directory) != null) {
				// Begin watching.
				buildDef.watcher.EnableRaisingEvents = true;
				return buildDef;
			}
		}

		return null;
	}

	protected override void HandleFileChanged(FileSystemEventArgs e) {
		string pattern = directory + "/(.+)\\.png";
		string input = Regex.Replace(e.FullPath, "\\\\", "/");
		Match match = Regex.Match(input, pattern);

		if (match.Success) {
			Group group = match.Groups[1];
			if (textures.ContainsKey(group.Value)) {
				KanimViewer.updateFrame = true;

				try {
					var tex = textures[group.Value];
					byte[] pngBytes = File.ReadAllBytes(e.FullPath);
					tex.LoadImage(pngBytes);
				} catch (System.Exception ex) {
					Debug.LogError("Failed to update file " + group.Value + " : " + ex);
				}
			}
		} else if (input == directory + "/" + "build.xml") {
			LoadBuildDef(this, directory);
			KanimViewer.updateFrame = true;
		}
	}
}