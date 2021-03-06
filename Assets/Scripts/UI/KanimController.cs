using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KanimController : MonoBehaviour {
	public class StoredFile {
		public string directory;
		public AnimDef animDef;
		public BuildDef buildDef;
		public Button button;
		public List<string> nameList;
	}

	public KanimViewer kanim;
	public Camera orthoCamera;
	public EventSystem eventSystem;
	public Text rateTxt;
	public Slider rateSlider;
	public Text idxTxt;
	public Slider idxSlider;
	public Button leftButton;
	public Button rightButton;
	public Button pauseButton;
	public Text pauseText;
	public Slider zoomSlider;
	public Button zoomUpButton;
	public Button zoomDownButton;
	public Button addFileButton;
	public Button quitButton;
	public Button fullScreenButton;
	public Button resetPositionButton;
	public Button listedFileTemplate;
	public FileBrowser fileBrowser;
	public Dropdown dropdown;
	public float fileListSpacing = 30;
	public UnloadFileConfirmer confirmer;

	private bool dragging = false;
	private Vector3 lastMousePosition;
	private StoredFile currentAnimDef;
	private List<StoredFile> listedFiles = new List<StoredFile>();

	// Start is called before the first frame update
	void Start() {
		rateSlider.onValueChanged.AddListener(RateChanged);
		idxSlider.onValueChanged.AddListener(FrameChanged);
		zoomSlider.onValueChanged.AddListener(ZoomChanged);
		ZoomChanged(zoomSlider.value);
		leftButton.onClick.AddListener(Left);
		rightButton.onClick.AddListener(Right);
		pauseButton.onClick.AddListener(Pause);
		zoomUpButton.onClick.AddListener(ZoomUp);
		zoomDownButton.onClick.AddListener(ZoomDown);
		resetPositionButton.onClick.AddListener(ResetPosition);
		quitButton.onClick.AddListener(Quit);
		addFileButton.onClick.AddListener(AddFile);
		listedFileTemplate.gameObject.SetActive(false);
		dropdown.onValueChanged.AddListener(OnDropDownChanged);
		fullScreenButton.onClick.AddListener(FullScreen);
	}

    // Update is called once per frame
    void Update() {
		idxTxt.text = "Frame Idx: " + kanim.GetFrameIdx();
		rateTxt.text = "Frames Per Second: " + kanim.frameRate;
		idxSlider.maxValue = kanim.GetAnimLength() - 1;
		idxSlider.value = kanim.GetFrameIdx();

		if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject()) {
			dragging = true;
			lastMousePosition = Input.mousePosition;
		} else if (!Input.GetMouseButton(0)) {
			dragging = false;
		} else if (dragging) {
			Vector3 mousePos = Input.mousePosition;
			Vector3 delta = lastMousePosition - mousePos;
			orthoCamera.transform.position += 2 * delta * orthoCamera.orthographicSize / Screen.height;
			lastMousePosition = mousePos;
		}

		float scroll = Input.mouseScrollDelta.y;
		if (scroll != 0) {
			float oldSize = orthoCamera.orthographicSize;
			float targetX = Input.mousePosition.x / Screen.height - 0.5f * orthoCamera.aspect;
			float targetY = Input.mousePosition.y / Screen.height - 0.5f;

			zoomSlider.value = Mathf.Clamp(zoomSlider.value + scroll, zoomSlider.minValue, zoomSlider.maxValue);
			ZoomChanged(zoomSlider.value);

			float newX = targetX * oldSize / orthoCamera.orthographicSize;
			float newY = targetY * oldSize / orthoCamera.orthographicSize;

			orthoCamera.transform.position += new Vector3(newX - targetX, newY - targetY) * 2 * orthoCamera.orthographicSize;
		} 
	}

	void RateChanged(float to) {
		int toInt = (int)to;
		kanim.SetFrameRate(toInt);
	}

	void FrameChanged(float to) {
		int toInt = (int)to;
		kanim.SetFrameIdx(toInt);
	}

	void ZoomChanged(float to) {
		orthoCamera.orthographicSize = Mathf.Pow(2, zoomSlider.maxValue + zoomSlider.minValue - to);
	}

	void ZoomUp() {
		zoomSlider.value = Mathf.Clamp(zoomSlider.value + 1, zoomSlider.minValue, zoomSlider.maxValue);
		ZoomChanged(zoomSlider.value);
	}

	void ZoomDown() {
		zoomSlider.value = Mathf.Clamp(zoomSlider.value - 1, zoomSlider.minValue, zoomSlider.maxValue);
		ZoomChanged(zoomSlider.value);
	}

	void ResetPosition() {
		orthoCamera.transform.position = new Vector3(0,0,-10);
	}

	void Left() {
		var i = kanim.GetFrameIdx() - 1;
		if (i < 0) {
			i = kanim.GetAnimLength() - 1;
		}
		kanim.SetFrameIdx(i);
	}

	void Right() {
		kanim.SetFrameIdx((kanim.GetFrameIdx() + 1) % kanim.GetAnimLength());
	}

	void Pause() {
		if (kanim.paused) {
			kanim.paused = false;
			pauseText.text = "||";
		} else {
			kanim.paused = true;
			pauseText.text = '\u00BB'.ToString();
		}
	}

	void TryAddFile(string fullDirectoryPath) {
		fullDirectoryPath = Regex.Replace(fullDirectoryPath, "\\\\", "/");

		StoredFile updateFile = null;
		foreach (var file in listedFiles) {
			if (file.directory == fullDirectoryPath) {
				updateFile = file;
				break;
			}
		}

		if (updateFile == null) {
			var build = kanim.LoadBuild(fullDirectoryPath);
			var anim = kanim.LoadAnimDef(fullDirectoryPath);
			if (build != null || anim != null) {
				updateFile = new StoredFile();
				updateFile.animDef = anim;
				updateFile.buildDef = build;
				updateFile.directory = fullDirectoryPath;
				updateFile.nameList = new List<string>();

				updateFile.button = Instantiate(listedFileTemplate,listedFileTemplate.transform.parent).GetComponent<Button>();
				updateFile.button.transform.GetChild(0).GetComponent<Text>().text = updateFile.directory;
				updateFile.button.transform.position -= new Vector3(0, fileListSpacing * listedFiles.Count);
				updateFile.button.onClick.AddListener( delegate { OnClickFile(updateFile); } );
				updateFile.button.gameObject.SetActive(true);

				var rect = (RectTransform)updateFile.button.transform.parent;
				rect.sizeDelta += new Vector2(0, fileListSpacing);

				listedFiles.Add(updateFile);
				if (anim != null) {
					foreach (var kanim in anim.kanims) {
						updateFile.nameList.Add(kanim.name);
					}

					dropdown.AddOptions(updateFile.nameList);
				}
			}
		}
	}

	void RefreshOptions() {
		dropdown.ClearOptions();
		foreach (var file in listedFiles) {
			dropdown.AddOptions(file.nameList);
		}
	}

	void AddFile() {
		if (!confirmer.isActiveAndEnabled && !fileBrowser.isActiveAndEnabled) {
			fileBrowser.callBack = TryAddFile;
			fileBrowser.ShowFileBrowser();
		}
	}

	void OnClickFile(StoredFile file) {
		if (!confirmer.isActiveAndEnabled && !fileBrowser.isActiveAndEnabled)
			confirmer.Show(file, TryRemoveFile);
	}

	void TryRemoveFile(StoredFile file) {
		var i = listedFiles.IndexOf(file);

		if (i >= 0) {
			kanim.RemoveBuildDef(file.buildDef);
			kanim.RemoveAnimDef(file.animDef);

			var rect = (RectTransform)file.button.transform.parent;
			rect.sizeDelta -= new Vector2(0, fileListSpacing);
			Destroy(file.button.gameObject);
			listedFiles.RemoveAt(i);

			for (; i < listedFiles.Count; i++) {
				listedFiles[i].button.transform.position += new Vector3(0, fileListSpacing);
			}

			RefreshOptions();
		}
	}

	void OnDropDownChanged(int i) {
		foreach (var file in listedFiles) {
			if (file.animDef != null) {
				if (i < file.animDef.kanims.Length) {
					kanim.SetCurrentKanim(file.animDef.kanims[i]);

					break;
				} else {
					i -= file.animDef.kanims.Length;
				}
			}
		}
	}

	void FullScreen() {
		Screen.fullScreen = !Screen.fullScreen;
	}

	void Quit() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
