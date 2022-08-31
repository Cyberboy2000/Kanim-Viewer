using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundChanger : MonoBehaviour {
	public Camera orthoCamera;
	public ColorPicker colorPicker;
	public FileBrowser fileBrowser;
	public SpriteRenderer bgSprite;
	public Button colorBtn;
	public Button imageBtn;
	public Button closeBtn;
	public Button resetBtn;
	public Slider scaleSlider;

	private void Start() {
		closeBtn.onClick.AddListener(Hide);
		colorBtn.onClick.AddListener(ShowBGColor);
		imageBtn.onClick.AddListener(ShowFileBrowser);
		resetBtn.onClick.AddListener(Reset);
		scaleSlider.onValueChanged.AddListener(ChangeBGScale);
		gameObject.SetActive(false);
	}

	void ChangeBGColor(Color color) {
		orthoCamera.backgroundColor = color;
	}

	void ChangeBGScale(float value) {
		bgSprite.transform.localScale = new Vector3(value, value, value);
	}

	bool ShouldShowFile(string filename) {
		var extension = filename.Substring(filename.LastIndexOf('.') + 1);
		return extension == "png" || extension == "jpg";
	}

	void ChangeFile(string fullDirectoryName, string fileName) {
		if (!string.IsNullOrEmpty(fileName)) {
			string filePath = fullDirectoryName + Path.DirectorySeparatorChar + fileName;

			if (File.Exists(filePath)) {
				byte[] fileData = File.ReadAllBytes(filePath);
				Texture2D texture = new Texture2D(2, 2);
				if (texture.LoadImage(fileData)) {
					Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);
					bgSprite.sprite = newSprite;
				} else {
					Debug.LogError("Failed to load image " + filePath);
				}
			} else {
				Debug.LogError("Couldn't find file " + filePath);
			}
		}
	}

	void ShowBGColor() {
		colorPicker.Show(orthoCamera.backgroundColor, ChangeBGColor);
	}

	void ShowFileBrowser() {
		fileBrowser.ShowFileBrowser(ChangeFile, ShouldShowFile, "BackgroundDirectory");
	}

	public void Show() {
		gameObject.SetActive(true);
	}

	void Hide() {
		gameObject.SetActive(false);
	}

	private void Reset() {
		colorPicker.Default();
		bgSprite.sprite = null;
		scaleSlider.value = 1;
		bgSprite.transform.localScale = Vector3.one;
	}
}
