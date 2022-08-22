using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogCallback : MonoBehaviour {
	public Text text;

	// Start is called before the first frame update
	void Start() {
		Application.logMessageReceived += Log;
	}

	void Log(string condition, string stackTrace, LogType logType) {
		text.text = condition + "\n" + stackTrace;
		switch (logType) {
			case LogType.Assert:
			case LogType.Exception:
				text.color = Color.red;
				break;
			case LogType.Log:
				text.color = Color.grey;
				break;
			case LogType.Warning:
				text.color = Color.yellow;
				break;
		}
	}
}
