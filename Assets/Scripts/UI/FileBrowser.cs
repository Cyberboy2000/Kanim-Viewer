using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour {
	public delegate void OnCallBack(string fullDirectoryName);
	public Button buttonTemplate;
	public float spacing = 30;
	public Button acceptButton;
	public Button cancelButton;
	public Button goUpButton;
	public OnCallBack callBack;
	public Text pathText;
	private string directory;
	private List<Button> directoryContents = new List<Button>();
	private int filesShown;


    // Start is called before the first frame update
    void Start() {
		directory = Application.dataPath;
		directory = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		pathText.text = directory;
		gameObject.SetActive(false);
		buttonTemplate.gameObject.SetActive(false);
		acceptButton.onClick.AddListener(OnClickFile);
		cancelButton.onClick.AddListener(HideFileBrowser);
		goUpButton.onClick.AddListener(GoUp);
	}

	private bool ShowFile(int i, string fullPath, bool isDirectory) {
		var fileName = fullPath.Substring(fullPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
		if (fileName == "") {
			fileName = fullPath;
		}

		if (isDirectory || fileName == "animation.xml" || fileName == "build.xml") {
			if (directoryContents.Count <= i) {
				var newBtn = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
				newBtn.transform.localPosition -= new Vector3(0, spacing * directoryContents.Count);
				directoryContents.Add(newBtn);
			}

			var btn = directoryContents[i];
			btn.gameObject.SetActive(true);
			btn.onClick.RemoveAllListeners();
			var text = btn.transform.GetChild(0).GetComponent<Text>();
			text.text = fileName;
			if (isDirectory) {
				btn.onClick.AddListener(delegate () { OnClickDirectory(fullPath); });
			} else {
				btn.onClick.AddListener(OnClickFile);
			}

			return true;
		} else {
			return false;
		}
	}

	public void ShowFileBrowser() {
		IEnumerable<string> subdirs;
		IEnumerable<string> files;

		if (directory == "") {
			subdirs = Directory.GetLogicalDrives();
			files = new string[0];
		} else if (directory.IndexOf(Path.DirectorySeparatorChar) <= 0) {
			subdirs = Directory.EnumerateDirectories(directory + Path.DirectorySeparatorChar);
			files = Directory.EnumerateFiles(directory + Path.DirectorySeparatorChar);
		} else {
			subdirs = Directory.EnumerateDirectories(directory);
			files = Directory.EnumerateFiles(directory);
		}

		pathText.text = directory;

		int i = 0;
		foreach (var subdir in subdirs) {
			if (ShowFile(i, subdir, true))
				i++;
		}
		foreach (var file in files) {
			if (ShowFile(i, file, false))
				i++;
		}

		var rect = (RectTransform)buttonTemplate.transform.parent;
		rect.sizeDelta += new Vector2(0, spacing * (i - filesShown));
		filesShown = i;

		for (;i < directoryContents.Count; i++) {
			var btn = directoryContents[i];
			btn.gameObject.SetActive(false);
		}

		gameObject.SetActive(true);
	}

	public void HideFileBrowser() {
		gameObject.SetActive(false);
	}

	private void OnClickDirectory(string fullPath) {
		directory = fullPath;
		ShowFileBrowser();
	}

	private void OnClickFile() {
		callBack(directory);
		HideFileBrowser();
	}

	private void GoUp() {
		var i = directory.LastIndexOf(Path.DirectorySeparatorChar);
		if (i > 0) {
			directory = directory.Substring(0,i);
		} else {
			directory = "";
		}

		ShowFileBrowser();
	}
}
