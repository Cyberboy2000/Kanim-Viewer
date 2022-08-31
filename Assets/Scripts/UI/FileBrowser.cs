using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour {
	public delegate void OnCallBack(string fullDirectoryName, string fileName);
	public delegate bool ShouldShowFile(string fileName);
	public Button buttonTemplate;
	public float spacing = 30;
	public Button acceptButton;
	public Button cancelButton;
	public Button goUpButton;
	public Text pathText;

	private OnCallBack _callBack;
	private ShouldShowFile _shouldShowFile;
	private string _directory;
	private List<Button> _directoryContents = new List<Button>();
	private string _directorySaveLocation;
	private int _filesShown;


	// Start is called before the first frame update
	void Start() {
		gameObject.SetActive(false);
		buttonTemplate.gameObject.SetActive(false);
		acceptButton.onClick.AddListener(() => { OnClickFile(""); });
		cancelButton.onClick.AddListener(HideFileBrowser);
		goUpButton.onClick.AddListener(GoUp);
	}

	private bool ShowFile(int i, string fullPath, bool isDirectory) {
		var fileName = fullPath.Substring(fullPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
		if (fileName == "") {
			fileName = fullPath;
		}

		if (isDirectory || _shouldShowFile(fileName)) {
			if (_directoryContents.Count <= i) {
				var newBtn = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
				newBtn.transform.localPosition -= new Vector3(0, spacing * _directoryContents.Count);
				_directoryContents.Add(newBtn);
			}

			var btn = _directoryContents[i];
			btn.gameObject.SetActive(true);
			btn.onClick.RemoveAllListeners();
			var text = btn.transform.GetChild(0).GetComponent<Text>();
			text.text = fileName;
			if (isDirectory) {
				btn.onClick.AddListener(delegate () { OnClickDirectory(fullPath); });
			} else {
				btn.onClick.AddListener(() => { OnClickFile(fileName); });
			}

			return true;
		} else {
			return false;
		}
	}

	private void UpdateFileBrowser() {
		IEnumerable<string> subdirs;
		IEnumerable<string> files;

		if (_directory == "") {
			subdirs = Directory.GetLogicalDrives();
			files = new string[0];
		} else if (_directory.IndexOf(Path.DirectorySeparatorChar) <= 0) {
			subdirs = Directory.EnumerateDirectories(_directory + Path.DirectorySeparatorChar);
			files = Directory.EnumerateFiles(_directory + Path.DirectorySeparatorChar);
		} else {
			subdirs = Directory.EnumerateDirectories(_directory);
			files = Directory.EnumerateFiles(_directory);
		}

		pathText.text = _directory;

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
		rect.sizeDelta += new Vector2(0, spacing * (i - _filesShown));
		_filesShown = i;

		for (; i < _directoryContents.Count; i++) {
			var btn = _directoryContents[i];
			btn.gameObject.SetActive(false);
		}
	}

	public void ShowFileBrowser(OnCallBack callback, ShouldShowFile shouldShowFile, string directorySaveLocation = "LastDirectory") {
		if (Directory.Exists(PlayerPrefs.GetString(directorySaveLocation))) {
			_directory = PlayerPrefs.GetString(directorySaveLocation);
		} else if (Directory.Exists(PlayerPrefs.GetString("LastDirectory"))) {
			_directory = PlayerPrefs.GetString("LastDirectory");
		} else {
			_directory = Application.dataPath;
		}

		_directorySaveLocation = directorySaveLocation;
		_directory = _directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		pathText.text = _directory;

		_callBack = callback;
		_shouldShowFile = shouldShowFile;
		gameObject.SetActive(true);
		UpdateFileBrowser();
	}

	public void HideFileBrowser() {
		gameObject.SetActive(false);
	}

	private void OnClickDirectory(string fullPath) {
		_directory = fullPath;
		UpdateFileBrowser();
	}

	private void OnClickFile(string fileName) {
		PlayerPrefs.SetString(_directorySaveLocation, _directory);
		PlayerPrefs.Save();
		_callBack(_directory, fileName);
		HideFileBrowser();
	}

	private void GoUp() {
		var i = _directory.LastIndexOf(Path.DirectorySeparatorChar);
		if (i > 0) {
			_directory = _directory.Substring(0, i);
		} else {
			_directory = "";
		}

		UpdateFileBrowser();
	}
}
