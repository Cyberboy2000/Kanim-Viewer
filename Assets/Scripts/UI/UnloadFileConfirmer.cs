using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnloadFileConfirmer : MonoBehaviour {
	public Text text;
	public Button cancelBtn;
	public Button unloadBtn;
	public delegate void Callback(KanimController.StoredFile file);

    // Start is called before the first frame update
    void Start() {
		gameObject.SetActive(false);
		cancelBtn.onClick.AddListener(Cancel);
	}

	public void Show(KanimController.StoredFile file, Callback callback) {
		gameObject.SetActive(true);
		text.text = "Do you want to unload directory?\n" + file.directory;
		unloadBtn.onClick.RemoveAllListeners();
		unloadBtn.onClick.RemoveAllListeners();
		unloadBtn.onClick.AddListener(delegate {
			callback(file);
			gameObject.SetActive(false);
		});
	}

	void Cancel() {
		gameObject.SetActive(false);
	}
}
