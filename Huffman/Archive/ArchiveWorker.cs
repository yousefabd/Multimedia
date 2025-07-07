using Huffman.Archive;
using Huffman.data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ArchiveWorker {
    public IArchiveProcessor Processor { get; private set; }
    public CancellationTokenSource CancelTokenSource { get; private set; }
    private ManualResetEventSlim pauseEvent = new(true);
    bool isPaused = false;

    public event Action<int>? CompressionCompleted;
    public event Action<Exception>? CompressionFailed;

    public event Action<List<FolderFileEntry>>? DecompressionCompleted;
    public event Action<Exception>? DecompressionFailed;

    public event Action<ProgressReport>? ProgressChanged;
    public event Action? OperationCancelled;

    public Progress<ProgressReport> progress;

    public ArchiveWorker(IArchiveProcessor processor) {
        Processor = processor;
        progress = new Progress<ProgressReport>(report => {
            ProgressChanged?.Invoke(report);
        });
        CancelTokenSource = new CancellationTokenSource();
    }

    public void SetProcessor(IArchiveProcessor processor) {
        Processor = processor;
    }

    public Task CompressAsync(List<FolderFileEntry> files, string outputArchivePath) {
        //reset variables
        CancelTokenSource.Dispose();
        CancelTokenSource = new CancellationTokenSource();
        pauseEvent.Dispose();
        pauseEvent = new(true);
        return Task.Run(() => {
            try {
                int ratioInt = Processor.Compress(files, outputArchivePath,progress,CancelTokenSource.Token, pauseEvent);
                CompressionCompleted?.Invoke(ratioInt);
            }
            catch (OperationCanceledException) {
                OperationCancelled?.Invoke();
            }
            catch (Exception ex) {
                CompressionFailed?.Invoke(ex);
            }
        });
    }

    public Task DecompressAsync(string archivePath, string outputBaseDirectory) {
        //reset variables
        CancelTokenSource.Dispose();
        CancelTokenSource = new CancellationTokenSource();
        pauseEvent.Dispose();
        pauseEvent = new(true);
        return Task.Run(() => {
            try {
                var entries = Processor.Decompress(archivePath, outputBaseDirectory,progress,CancelTokenSource.Token,pauseEvent);
                DecompressionCompleted?.Invoke(entries);
            }
            catch (OperationCanceledException) {
                OperationCancelled?.Invoke();
            }
            catch (Exception ex) {
                DecompressionFailed?.Invoke(ex);
            }
        });
    }

    public void Cancel() {
        CancelTokenSource.Cancel();
    }
    private void Pause() => pauseEvent.Reset();
    private void Resume() => pauseEvent.Set();
    public void TogglePause() {
        if (isPaused) {
            Resume();
        }
        else {
            Pause();
        }
        isPaused = !isPaused;
    }
}

