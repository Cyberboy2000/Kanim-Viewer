using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

public class LogCallback : MonoBehaviour {
	private struct LogMessage {
		public string message;
		public string stackTrace;
		public LogType logType;
		public LogMessage(string message, string stackTrace, LogType logType) {
			this.message = message;
			this.stackTrace = stackTrace;
			this.logType = logType;
		}
	}

	public Text text;
	private ConcurrentQueue<LogMessage> _logQueue = new ConcurrentQueue<LogMessage>();

	// Start is called before the first frame update
	void Start() {
		Application.logMessageReceivedThreaded += ReceiveLog;
	}

	void ReceiveLog(string message, string stackTrace, LogType logType) {
		_logQueue.Enqueue(new LogMessage(message, stackTrace, logType));
	}

	private void Update() {
		if (_logQueue.TryDequeue(out var message)) {
			Log(message.message, message.stackTrace, message.logType);
		}
	}

	void Log(string message, string stackTrace, LogType logType) {
		text.text = message + "\n" + stackTrace;
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
