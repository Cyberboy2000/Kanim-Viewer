using System.Collections.Concurrent;
using System.IO;

public abstract class CommonDef : System.IDisposable {
	protected ConcurrentQueue<FileSystemEventArgs> fileChangedQueue = new ConcurrentQueue<FileSystemEventArgs>();
	protected FileSystemWatcher watcher;
	protected string directory;

	protected void InitWatcher(string directory) {
		this.directory = directory;
		watcher = new FileSystemWatcher(directory);
		watcher.NotifyFilter = NotifyFilters.LastWrite
							 | NotifyFilters.FileName
							 | NotifyFilters.DirectoryName;

		// Add event handlers.
		watcher.Changed += OnFileChanged;
		watcher.Created += OnFileChanged;
		watcher.Renamed += OnFileChanged;
	}

	public void Dispose() {
		if (watcher != null) {
			watcher.Dispose();
			watcher = null;
		}
	}

	private void OnFileChanged(object source, FileSystemEventArgs e) {
		fileChangedQueue.Enqueue(e);
	}

	protected abstract void HandleFileChanged(FileSystemEventArgs e);

	public void OnUpdate() {
		FileSystemEventArgs e;
		if (fileChangedQueue.TryDequeue(out e)) {
			HandleFileChanged(e);
		}
	}
}
