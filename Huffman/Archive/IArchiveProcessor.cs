using Huffman.data;

namespace Huffman.Archive; 
public interface IArchiveProcessor {
    void Compress(List<FolderFileEntry> files, string outputArchivePath,IProgress<ProgressReport>? progress,CancellationToken cancellationToken, ManualResetEventSlim _pauseEvent);
    List<FolderFileEntry> Decompress(string archivePath, string outputBaseDirectory,IProgress<ProgressReport>? progress,CancellationToken cancellationToken, ManualResetEventSlim _pauseEvent);
}