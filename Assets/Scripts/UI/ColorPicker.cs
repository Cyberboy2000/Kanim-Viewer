using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour {
	public Slider red;
	public Slider green;
	public Slider blue;
	public Button closeBtn;
	public Button resetBtn;

	private System.Action<Color> callback;
	private ColorPickerImage[] pickerImages;
	private Color32 color;

	// Start is called before the first frame update
	void Start() {
		gameObject.SetActive(false);
		red.onValueChanged.AddListener(OnRedChange);
		red.GetComponentInChildren<InputField>().onValueChanged.AddListener(OnRedChange);
		green.onValueChanged.AddListener(OnGreenChange);
		green.GetComponentInChildren<InputField>().onValueChanged.AddListener(OnGreenChange);
		blue.onValueChanged.AddListener(OnBlueChange);
		blue.GetComponentInChildren<InputField>().onValueChanged.AddListener(OnBlueChange);
		closeBtn.onClick.AddListener(Hide);
		resetBtn.onClick.AddListener(Default);
		pickerImages = GetComponentsInChildren<ColorPickerImage>();
	}

	void OnChange(Slider s, int i) {
		i = Mathf.Clamp(i, byte.MinValue, byte.MaxValue);
		s.value = i;
		s.GetComponentInChildren<InputField>().text = i.ToString();
		color = new Color32((byte)red.value, (byte)green.value, (byte)blue.value, 255);
		foreach (var img in pickerImages) {
			img.color = color;
		}

		callback(color);
	}

	void OnRedChange(string s) {
		OnChange(red, System.Convert.ToInt32(s));
	}

	void OnRedChange(float f) {
		OnChange(red, System.Convert.ToInt32(f));
	}

	void OnGreenChange(string s) {
		OnChange(green, System.Convert.ToInt32(s));
	}

	void OnGreenChange(float f) {
		OnChange(green, System.Convert.ToInt32(f));
	}

	void OnBlueChange(string s) {
		OnChange(blue, System.Convert.ToInt32(s));
	}

	void OnBlueChange(float f) {
		OnChange(blue, System.Convert.ToInt32(f));
	}

	public void Show(Color currentColor, System.Action<Color> callback) {
		this.callback = callback;
		gameObject.SetActive(true);
		OnRedChange(currentColor.r * 255);
		OnGreenChange(currentColor.g * 255);
		OnBlueChange(currentColor.b * 255);
	}

	void Hide() {
		gameObject.SetActive(false);
	}

	void Default() {
		OnRedChange(49);
		OnGreenChange(77);
		OnBlueChange(121);
	}
}
